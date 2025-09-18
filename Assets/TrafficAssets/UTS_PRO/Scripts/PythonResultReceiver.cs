using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class PythonResultReceiver : MonoBehaviour
{
    Thread listenerThread;
    TcpClient client;
    TcpListener listener;
    string receivedData = "";

    // Unity UI Text (要拖到 Inspector)
    public TextMeshProUGUI outputNum;

    void Start()
    {
        listenerThread = new Thread(new ThreadStart(ListenForData));
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    void ListenForData()
    {
        try
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9100);
            listener.Start();

            client = listener.AcceptTcpClient();

            Byte[] bytes = new Byte[1024];
            while (true)
            {
                NetworkStream stream = client.GetStream();
                int length;
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var incommingData = new byte[length];
                    Array.Copy(bytes, 0, incommingData, 0, length);
                    receivedData = Encoding.UTF8.GetString(incommingData);
                }
            }
        }
        catch (Exception e)
        {
            //Debug.LogError("Socket error: " + e);
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(receivedData))
        {
            // 假設 Python 傳過來是字典字串: {"Camera1": 2, "Camera2": 3}
            try
            {
                // 只抓數字 (簡單做法)
                int total = 0;
                foreach (var part in receivedData.Split(','))
                {
                    foreach (char c in part)
                    {
                        if (char.IsDigit(c))
                        {
                            total += int.Parse(c.ToString());
                        }
                    }
                }

                // 更新 UI
                outputNum.text = "Total: " + total;
            }
            catch (Exception e)
            {
                //Debug.LogWarning("Parse error: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        listener?.Stop();
        client?.Close();
        listenerThread?.Abort();
    }
}
