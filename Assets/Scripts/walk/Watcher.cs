using System.Collections.Generic;
using UnityEngine;

public class Watcher : MonoBehaviour
{

    // ���� ��������
    public Direction[] directions;

    
    public Material _correctMat;
    public Material _inCorrectMat;
    public SemaphorePeople semaphore;
    [SerializeField] private GameObject crosswalk;

    private void Awake()
    {
        semaphore.ChangeLightColor += ResetWatcher;
    }

    public void CheckWatcher()
    {
        if (crosswalk == null)
            Debug.Log("������� ���� ������ �� crosswalk");

        if (directions.Length <= 1)
            Debug.Log("������� ������ directions � Watcher, ������� ���� ���� � ������� ������ ���������� �����.");

        if(semaphore == null)
            Debug.Log("������� ���� ������ �� semaphore");

        foreach (Direction dir in directions)
        {
            if (dir.mainZone.playerStayInZone)
            {
                //���� ����� ����� � ����
                if (semaphore.PEOPLE_CAN == false)
                    return;

                foreach (ZoneColor zc in dir.playerLooked)
                {
                    if (zc.PlayerLookedToThis == false)
                        return;
                }
            }
        }

        crosswalk.GetComponent<MeshRenderer>().material = _correctMat;
    }

    public void ResetWatcher()
    {
        if(semaphore.PEOPLE_CAN == false)
        {
            //��� ���� �� ���� �������� ����������� ������� �������
            foreach (Direction dir in directions)
            {
                foreach (ZoneColor zc in dir.playerLooked)
                {
                    zc.PlayerLookedToThis = false;
                }
            }

            crosswalk.GetComponent<MeshRenderer>().material = _inCorrectMat;
        }
        
    }
}

[System.Serializable]
public class Direction
{
    public ZoneColor[] playerLooked;
    public MainZone mainZone;
}

