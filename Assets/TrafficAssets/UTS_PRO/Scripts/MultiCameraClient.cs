using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Concurrent;
using System.Collections;
using UnityEditor.Search;

[Serializable]
public class DetectedObject
{
    public int id;
    public int x, y, w, h;
    public float conf;
}

[Serializable]
public class InferenceResult
{
    public int camera_id;
    public int object_count;
    public List<DetectedObject> objects;
}

public class MultiCameraClient : MonoBehaviour
{
    public Camera[] cameras;             // Inspector 拖拉 6 個攝影機
    public RawImage[] displays;          // Inspector 拖拉 6 個 RawImage 顯示畫面
    public TextMeshPro[] countTexts;     // 顯示每路數量
    public TextMeshProUGUI countNumber; // 顯示總數

    private int[] ports = { 5001, 5002, 5003, 5004, 5005, 5006 };
    private TcpClient[] clients;
    private NetworkStream[] streams;
    private Thread[] recvThreads;
    private Dictionary<int, InferenceResult> results = new Dictionary<int, InferenceResult>();

    private int[] frameCounters;  // 每個 camera 的幀計數器
    public int sendEveryNFrames = 5;  // 每 5 幀傳一次
    private Thread recvT;
    private List<ConcurrentQueue<byte[]>> queue = new();
    private ConcurrentQueue<string> stringQueue = new();

    void Start()
    {
        frameCounters = new int[cameras.Length];
        int camCount = ports.Length;
        clients = new TcpClient[camCount];
        streams = new NetworkStream[camCount];
        recvThreads = new Thread[camCount];
        recvT = new Thread(Hello);


        for (int i = 0; i < camCount; i++)
        {
            queue.Add(new ConcurrentQueue
                <byte[]>());
            ConnectCamera(i);
        }

        recvT.Start();
    }

    void Hello() {
        while (true)
        {
            for (int camId = 0; camId < 6; camId++)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(id =>
                    {
                        int camidclosure = (int)id;
                        var q = queue[camidclosure];
                        if (q.TryDequeue(out var imgBytes))
                        {
                            // 傳送資料 
                            stringQueue.Enqueue($"Camara:{camidclosure} {imgBytes.Length}");
                            NetworkStream stream = streams[camidclosure];
                            byte[] lengthPrefix = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(imgBytes.Length));
                            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                            stream.Write(imgBytes, 0, imgBytes.Length);
                            stream.Flush();
                        }
                    }, camId);

                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Camera {camId}] 連線失敗: {e.Message}");
                }
            }
            Thread.Sleep(300);
        }
    }
    // ---------------- 嘗試連線攝影機 ----------------
    void ConnectCamera(int camId)
    {
        try
        {
            clients[camId]?.Close();
            clients[camId] = new TcpClient("127.0.0.1", ports[camId]);
            streams[camId] = clients[camId].GetStream();

            // 啟動接收資料的 Thread
            recvThreads[camId] = new Thread(() => ReceiveLoop(camId));
            recvThreads[camId].IsBackground = true;
            recvThreads[camId].Start();

            //Debug.Log($"[Camera {camId}] 已連線到 Python Server {ports[camId]}");
        }
        catch (Exception e)
        {
            //Debug.LogWarning($"[Camera {camId}] 連線失敗: {e.Message}");
        }
    }

    // ---------------- 捕捉與傳送影像 ----------------
    void CaptureAndSendFrame(int camId)
    {
        if (cameras == null || camId >= cameras.Length || cameras[camId] == null)
        {
            //Debug.LogError($"[Camera {camId}] Camera 未設定！");
            return;
        }

        RenderTexture rt = cameras[camId].targetTexture;
        if (rt == null)
        {
            //Debug.LogError($"[Camera {camId}] RenderTexture 未設定！");
            return;
        }

        if (clients[camId] == null || !clients[camId].Connected)
        {
            ConnectCamera(camId); // 自動嘗試重連
            return;
        }

        try
        {
            // 抓取畫面
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] imgBytes = tex.EncodeToPNG();
            Destroy(tex);

            Debug.Log($"[Camera {camId}] send {imgBytes.Length} bytes");
            queue[camId].Enqueue(imgBytes);
        }
        catch (Exception ex)
        {
            //Debug.LogWarning($"[Camera {camId}] 傳送錯誤: {ex.Message}");
            ConnectCamera(camId); // 傳送失敗，自動重連
        }
    }

    // ---------------- 接收 Python Server 回傳 ----------------
    void ReceiveLoop(int camId)
    {
        NetworkStream stream = streams[camId];
        while (true)
        {
            try
            {
                byte[] rawLen = new byte[4];
                if (stream.Read(rawLen, 0, 4) == 0) break;
                int msgLen = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(rawLen, 0));

                byte[] buffer = new byte[msgLen];
                int read = 0;
                while (read < msgLen)
                {
                    read += stream.Read(buffer, read, msgLen - read);
                }

                string json = Encoding.UTF8.GetString(buffer);
                InferenceResult result = JsonUtility.FromJson<InferenceResult>(json);

                results[result.camera_id - 1] = result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Camera {camId}] 接收錯誤: {e.Message}\r\n{e.StackTrace}");

                //ConnectCamera(camId); // 自動重連
            }

        }
        
    }

    // ---------------- 每幀更新 ----------------
    void Update()
    {
        int totalCount = 0;

        for (int i = 0; i < cameras.Length; i++)
        {
            frameCounters[i]++;
            if (frameCounters[i] >= sendEveryNFrames)
            {
                CaptureAndSendFrame(i);
                frameCounters[i] = 0;
            }

            if (results.ContainsKey(i))
            {
                countTexts[i].text = $"Count: {results[i].object_count}";
                totalCount += results[i].object_count;
            }

        }

        if (countNumber != null)
        {
            countNumber.text = $"Total: {totalCount}";
        }

        while (stringQueue.Count > 0) 
        {
            if (stringQueue.TryDequeue(out var item))
            {
                //Debug.Log(item);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var v in clients) 
        {
            v.Close();
        }
        recvT.Abort();
    }
}
