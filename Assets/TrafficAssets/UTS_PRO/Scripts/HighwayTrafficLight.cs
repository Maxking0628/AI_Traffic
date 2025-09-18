using UnityEngine;

public class HighwayTrafficLight : MonoBehaviour
{
    [SerializeField] private float greenTime = 10f;   // ºñ¿O«ùÄò¬í¼Æ
    [SerializeField] private float yellowTime = 3f;   // ¶À¿O¬í¼Æ
    [SerializeField] private float redTime = 10f;     // ¬õ¿O¬í¼Æ

    private float timer;
    private int stage; // 0=ºñ¿O, 1=¶À¿O, 2=¬õ¿O

    public int STAGE => stage;

    private void Start()
    {
        stage = 0; // ¤@¶}©l¬Oºñ¿O
        timer = greenTime;
        Debug.Log("ºñ¿O¶}©l");
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            NextStage();
        }
    }

    private void NextStage()
    {
        switch (stage)
        {
            case 0: // ºñ¿O ¡÷ ¶À¿O
                stage = 1;
                timer = yellowTime;
                Debug.Log("¶À¿O°{Ã{¶}©l");
                break;

            case 1: // ¶À¿O ¡÷ ¬õ¿O
                stage = 2;
                timer = redTime;
                Debug.Log("¬õ¿O¶}©l");
                break;

            case 2: // ¬õ¿O ¡÷ ºñ¿O
                stage = 0;
                timer = greenTime;
                Debug.Log("ºñ¿O¶}©l");
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CarAIController car = other.GetComponent<CarAIController>();
            if (car != null)
            {
                // stage 2 = ¬õ¿O ¡÷ °±
                // stage 0/1 = ºñ¿O©Î¶À¿O ¡÷ ¥i¥H³q¦æ
                //car.STOP = (stage == 2);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CarAIController car = other.GetComponent<CarAIController>();
            if (car != null)
            {
               // car.STOP = false; // Â÷¶}«á«ì´_¦æ¾p
            }
        }
    }
}
