using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class RTTcpStreamer : MonoBehaviour
{
    public Camera renderCam;
    public int port = 7777;
    public int targetFps = 20;
    [Range(1, 100)] public int jpegQuality = 10;

    //想降低延遲，jpegQuality 降低、解析度降低、targetFps 合理（20~30）。

    private RenderTexture rt;
    private Texture2D readTex;
    private float nextCaptureTime;
    private ConcurrentQueue<byte[]> frameQueue = new ConcurrentQueue<byte[]>();

    private Thread serverThread;
    private volatile bool running;
    private TcpListener listener;

    void Start()
    {
        Application.runInBackground = true;

        rt = renderCam.targetTexture;
        if (rt == null)
        {
            rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            rt.Create();
            renderCam.targetTexture = rt;
        }

        readTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

        running = true;
        serverThread = new Thread(ServerLoop) { IsBackground = true };
        serverThread.Start();
    }

    void Update()
    {
        // 以 targetFps 為節奏擷取
        if (Time.time >= nextCaptureTime)
        {
            nextCaptureTime = Time.time + 1f / Mathf.Max(1, targetFps);
            EnqueueCurrentFrame();
        }
    }

    void EnqueueCurrentFrame()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        readTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        readTex.Apply(false);
        RenderTexture.active = prev;

        byte[] jpg = readTex.EncodeToJPG(jpegQuality);
        frameQueue.Enqueue(jpg);
    }

    void ServerLoop()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Debug.Log($"TCP Streamer listening on {port}");

            while (running)
            {
                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                {
                    client.NoDelay = true;
                    Debug.Log("Python client connected.");

                    while (running && client.Connected)
                    {
                        if (!frameQueue.TryDequeue(out var frameBytes))
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        // 先送 4 bytes 大端序長度，再送影像本體
                        var lenBytes = BitConverter.GetBytes(frameBytes.Length);
                        if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                        stream.Write(lenBytes, 0, 4);
                        stream.Write(frameBytes, 0, frameBytes.Length);
                        stream.Flush();
                    }

                    Debug.Log("Python client disconnected.");
                }
            }
        }
        catch (SocketException e)
        {
            Debug.LogError(e);
        }
        finally
        {
            listener?.Stop();
        }
    }

    void OnDestroy()
    {
        running = false;
        listener?.Stop();
        if (serverThread != null && serverThread.IsAlive) serverThread.Join();
        if (readTex != null) Destroy(readTex);
    }
}
