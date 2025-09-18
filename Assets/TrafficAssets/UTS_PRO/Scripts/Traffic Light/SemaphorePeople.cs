using UnityEngine;
using System;

public class SemaphorePeople : MonoBehaviour
{
    public event Action ChangeLightColor;
    [SerializeField] private bool carCan;
    private bool flicker;

    public bool CAR_CAN
    {
        get { return carCan; }
        set { carCan = value; }
    }

    public bool FLICKER
    {
        get { return flicker; }
        set { flicker = value; }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("Car"))
    //    {
    //        if (other.transform.GetComponent<CarAIController>())
    //        {
    //            CarAIController car = other.GetComponent<CarAIController>();
    //            car.INSIDE = true;
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Car"))
    //    {
    //        if (other.transform.GetComponent<CarAIController>())
    //        {
    //            CarAIController car = other.GetComponent<CarAIController>();
    //            car.INSIDE = false;
    //        }
    //    }
    //}
}
