using UnityEngine;

public class SemaphoreSimulator : MonoBehaviour
{
    private float greenTimer;
    private float yellowTimer;
    private float redTimer;
    private bool yellowOn;
    private bool timeBreak;

    [SerializeField]
    [Tooltip("前方方向的紅綠燈")]
    private TLGraphicsControl[] FWDlights;

    [SerializeField]
    [Tooltip("綠燈時間")]
    public float greenTime;

    [SerializeField]
    [Tooltip("黃燈時間")]
    public float yellowTime;

    [SerializeField]
    [Tooltip("紅燈時間")]
    public float redTime;

    // ⚠ stage 要保留（其他腳本會用到）
    private int stage;

    public bool YELLOW_ON
    {
        get { return yellowOn; }
        set
        {
            yellowOn = value;
            YellowTime();
        }
    }

    public int STAGE
    {
        get { return stage; }
        set { stage = value; }
    }

    private void Awake()
    {
        greenTimer = greenTime;
        yellowTimer = yellowTime;
        redTimer = redTime;
    }

    private void Start()
    {
        SetFlow();
    }

    private void Update()
    {
        if (yellowOn)
        {
            yellowTimer -= Time.deltaTime;

            if (yellowTimer <= 0)
            {
                yellowOn = false;
                yellowTimer = yellowTime;

                if (timeBreak)
                {
                    stage = stage == 0 ? 1 : 0;
                    timeBreak = false;
                    SetFlow();
                    greenTimer = greenTime;
                }
                else
                {
                    for (int i = 0; i < FWDlights.Length; i++)
                    {
                        FWDlights[i].DisableYellow();
                        FWDlights[i].EnableRed();
                    }

                    if (stage == 0)
                    {
                        timeBreak = true;
                    }
                }
            }
        }
        else
        {
            if (timeBreak)
            {
                TimeBreak();
            }
        }

        if (greenTimer > 0)
        {
            greenTimer -= Time.deltaTime;

            if (greenTimer <= 0)
            {
                StartFlickerGreen();
            }
        }
    }

    private void StartFlickerGreen()
    {
        for (int i = 0; i < FWDlights.Length; i++)
        {
            FWDlights[i].FlickerGreen(4.0f, 0.5f);
        }
    }

    private void TimeBreak()
    {
        redTimer -= Time.deltaTime;

        if (redTimer <= 0)
        {
            for (int i = 0; i < FWDlights.Length; i++)
            {
                FWDlights[i].EnableYellow();
            }

            redTimer = redTime;
            yellowOn = true;
        }
    }

    private void AllowFwd()
    {
        for (int i = 0; i < FWDlights.Length; i++)
        {
            FWDlights[i].EnableGreen(true);
            FWDlights[i].DisableRed();
            FWDlights[i].DisableYellow();
        }
    }

    private void SetFlow()
    {
        AllowFwd();
    }

    public void ResetSemaphore()
    {
        timeBreak = true;
    }

    private void YellowTime()
    {
        // 行人相關已移除
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Car"))
        {
            if (other.transform.GetComponentInParent<CarAIController>())
            {
                CarAIController car = other.GetComponentInParent<CarAIController>();
                car.INSIDE = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Car"))
        {
            if (other.transform.GetComponentInParent<CarAIController>())
            {
                CarAIController car = other.GetComponentInParent<CarAIController>();
                car.INSIDE = false;
            }
        }
    }

    public bool IsRed
    {
        get { return !yellowOn && greenTimer <= 0; } // 這只是範例，你可以依邏輯調整
    }

}
