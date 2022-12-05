using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : MonoBehaviour
{
    protected DI di;
    [SerializeField] bool defaultText;

    bool tablNotSubscribe;
    void Start()
    {
        di = DI.instance;
        tablNotSubscribe = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //������� ����� �� ��������
            if(defaultText)
                di.tooltip.ChangeTooltipText("������ �������� �� ������, ������� ������� ��������� ��������.");
            di.tooltip.UpdateCloseText();
            //�������� ��������
            di.tooltip.ShowTip();

            //����������� �������� � �������
            if(tablNotSubscribe)
            {
                di.holdThreeSeconds.ReduceTime += di.tooltip.ReduceTime;
                di.holdThreeSeconds.ResetTime += di.tooltip.ResetTime;
                tablNotSubscribe = false;
            }
            

            //�������� ����
            di.dieSound.Play();

            //��������� ��������
            di.rightTeleportController.SetActive(false);
            di.leftTeleportController.SetActive(false);
        }
    }
}
