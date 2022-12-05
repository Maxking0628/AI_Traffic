using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainZone : MonoBehaviour
{
    public bool playerStayInZone { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("����� � ������� ����");
            playerStayInZone = true;
            //���������� ������� ���� ��������
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStayInZone = false;
        }
    }
}
