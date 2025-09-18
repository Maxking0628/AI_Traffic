using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [System.Serializable]
    public class TrafficLightControl
    {
        [Tooltip("對應 StandardCrossroad 下的 SemaphoreSimulator")]
        public SemaphoreSimulator semaphore;

        [Tooltip("綠燈秒數")]
        public float greenTime = 10f;

        [Tooltip("黃燈秒數")]
        public float yellowTime = 3f;

        [Tooltip("紅燈秒數")]
        public float redTime = 10f;
    }

    [Header("各紅綠燈控制")]
    public TrafficLightControl[] trafficLights;

    private void Awake()
    {
        // 如果 trafficLights 沒手動設定，嘗試自動抓取 Road_out 下的 SemaphoreSimulator
        if (trafficLights.Length == 0)
        {
            var semaphores = GetComponentsInChildren<SemaphoreSimulator>();
            trafficLights = new TrafficLightControl[semaphores.Length];
            for (int i = 0; i < semaphores.Length; i++)
            {
                trafficLights[i] = new TrafficLightControl
                {
                    semaphore = semaphores[i],
                    greenTime = semaphores[i].greenTime,
                    yellowTime = semaphores[i].yellowTime,
                    redTime = semaphores[i].redTime
                };
            }
        }
    }

    private void Start()
    {
        // 初始化每個紅綠燈的時間
        foreach (var t in trafficLights)
        {
            if (t.semaphore != null)
            {
                t.semaphore.greenTime = t.greenTime;
                t.semaphore.yellowTime = t.yellowTime;
                t.semaphore.redTime = t.redTime;

                t.semaphore.ResetSemaphore(); // 重置紅綠燈
            }
        }
    }

    // 可以單獨修改某個紅綠燈時間
    public void SetTrafficLightTime(int index, float green, float yellow, float red)
    {
        if (index < 0 || index >= trafficLights.Length) return;

        var t = trafficLights[index];
        if (t.semaphore == null) return;

        t.greenTime = green;
        t.yellowTime = yellow;
        t.redTime = red;

        t.semaphore.greenTime = green;
        t.semaphore.yellowTime = yellow;
        t.semaphore.redTime = red;

        t.semaphore.ResetSemaphore();
    }
}
