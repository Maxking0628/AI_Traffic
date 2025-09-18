using UnityEngine;
using System.IO;

public class SaveRTFrames : MonoBehaviour
{
    public Camera renderCam;             // 指向有 targetTexture 的相機
    public int captureFps = 5;           // 每秒存幾張
    public string outputFolder = "D:\\CodeSet\\AI_Race\\OutputFrame"; // 共用資料夾（Windows 範例）

    private float timer;
    private Texture2D readTex;
    private RenderTexture rt;

    void Start()
    {
        Application.runInBackground = true; // 讓 Unity 退到背景仍會跑
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        rt = renderCam.targetTexture;
        if (rt == null)
        {
            // 萬一你忘了在 Inspector 指定，這裡自動建立一個
            rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            rt.Create();
            renderCam.targetTexture = rt;
        }

        // 用 RGB24 就好（比 RGBA32 小）
        readTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / captureFps)
        {
            timer = 0f;
            CaptureAndSave();
        }
    }

    void CaptureAndSave()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        // 從 GPU 把像素讀回 CPU
        readTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        readTex.Apply(false);

        RenderTexture.active = prev;

        // 存圖（JPG 檔案較小，90 品質夠用）
        byte[] jpg = readTex.EncodeToJPG(90);
        string filename = Path.Combine(outputFolder, $"frame_{Time.frameCount:D08}.jpg");
        File.WriteAllBytes(filename, jpg);
    }

    void OnDestroy()
    {
        if (readTex != null) Destroy(readTex);
    }
}
