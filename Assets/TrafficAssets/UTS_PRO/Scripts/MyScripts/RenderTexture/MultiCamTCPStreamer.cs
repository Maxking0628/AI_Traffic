using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System;

[System.Serializable]
public class CamStream
{
    public Camera cam;
    public int port;
    [HideInInspector] public RenderTexture rt;
    [HideInInspector] public Texture2D readTex;
    [HideInInspector] public ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();
}

public class MultiCamTCPStreamer : MonoBehaviour
{
    public CamStream[] cams;
    public int targetFps = 20;
    [Range(1, 100)] public int jpegQuality = 80;

    private float nextCaptureTime;
 
    void Start()
    {
        Application.runInBackground = true;

        foreach (var c in cams)
        {
            // 確認 RenderTexture
            c.rt = c.cam.targetTexture;
            if (c.rt == null)
            {
                c.rt = new RenderTexture(1280, 720, 24);
                c.cam.targetTexture = c.rt;
            }
            c.readTex = new Texture2D(c.rt.width, c.rt.height, TextureFormat.RGB24, false);

            // 開 TCP server
            Thread t = new Thread(() => ServerLoop(c)) { IsBackground = true };
            t.Start();
        }
    }

    void Update()
    {
        if (Time.time >= nextCaptureTime)
        {
            nextCaptureTime = Time.time + 1f / Mathf.Max(1, targetFps);

            foreach (var c in cams)
            {
                CaptureFrame(c);
            }
        }
    }

    void CaptureFrame(CamStream c)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = c.rt;
        c.readTex.ReadPixels(new Rect(0, 0, c.rt.width, c.rt.height), 0, 0);
        c.readTex.Apply();
        RenderTexture.active = prev;

        byte[] jpg = c.readTex.EncodeToPNG();
        c.queue.Enqueue(jpg);
    }

    void ServerLoop(CamStream c)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, c.port);
        listener.Start();
        Debug.Log($"TCP Streamer listening on {c.port}");

        while (true)
        {
            try
            {
                //檢查有沒有 client
                if (!listener.Pending())
                {
                    Thread.Sleep(10); // 沒有 client 時休息 10ms，避免 CPU 跑滿
                    continue;
                }

                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                {
                    client.NoDelay = true;
                    Debug.Log($"Client connected on port {c.port}");

                    while (client.Connected)
                    {
                        if (!c.queue.TryDequeue(out var frameBytes))
                        {
                            Thread.Sleep(5);
                            continue;
                        }

                        // 送 4 bytes 長度 + JPEG
                        var lenBytes = BitConverter.GetBytes(frameBytes.Length);
                        if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                        stream.Write(lenBytes, 0, 4);
                        stream.Write(frameBytes, 0, frameBytes.Length);
                        stream.Flush();
                    }

                    Debug.Log($"Client disconnected from port {c.port}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

}
