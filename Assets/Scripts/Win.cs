using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    private DI di;

    public GameObject arrow;

    bool tablNotSubscribe;
    void Start()
    {
        di = DI.instance;
        tablNotSubscribe = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(arrow != null)
                arrow.SetActive(false);
            //������� ����� �� ��������
            di.tooltip.ChangeTooltipText("���������!");
            di.tooltip.UpdateCloseText();
            //�������� ��������
            di.tooltip.ShowTip();

            //����������� �������� � �������
            if (tablNotSubscribe)
            {
                di.holdThreeSeconds.ReduceTime += di.tooltip.ReduceTime;
                di.holdThreeSeconds.ResetTime += di.tooltip.ResetTime;
                tablNotSubscribe = false;
            }

            //��������� ��������
            di.rightTeleportController.SetActive(false);
            di.leftTeleportController.SetActive(false);
        }
    }
}
