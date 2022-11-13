using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// ��������� ������� ���������������� ���� �� ������ ����� �������� �������
/// ������� � ����������� ����� ����� ������� �������������
/// </summary>

public class Quest : MonoBehaviour
{
    public int CurrentNumberStep { get; private set; }
    public Step[] steps;


    [Tooltip("������� ������ ��� �������� � ���������� ����")]
    [SerializeField] InputActionReference controllerNextStep;

    //______________________system variables
    private AudioSource auDictor;
    IEnumerator waitSpeachDictor;
    Action playstep;
    public int testStep;
    //_______________________

    private void Start()
    {
        CurrentNumberStep = 0;
    }

    protected virtual void OnEnable()
    {
        controllerNextStep.action.performed += ClickNextStepButton;
    }

    protected virtual void OnDisable()
    {
        controllerNextStep.action.performed -= ClickNextStepButton;
    }

    void ClickNextStepButton(InputAction.CallbackContext obj)
    {
        if (steps[CurrentNumberStep].clickToNextStep)
            NextStep();
    }

    void NextStep()
    {
        //���������� ����
        steps[CurrentNumberStep].eventAfterStep?.Invoke();
        CurrentNumberStep++;
        steps[CurrentNumberStep].eventBeforeStartStep?.Invoke();
        //���� ��������� �� ��� ����������
            //�������� ���������
        //���� ��������� �����������
            //�������� ���������
        //�������� ����
        //���� ��������� � ���������� ���� ����� ������� �� ��������� �������� ������� ��� ��� ����������
        //�� ���������� ����� ��������� NextStep
    }

}

/// <summary>
/// ������ �� ����� ����
/// </summary>
[System.Serializable]
public class Step
{
    public string name;

    [Space]
    public UnityEvent eventBeforeStartStep = new UnityEvent();


    [Tooltip("������� ��������")]
    public AudioClip dictor;

    [Space]
    [Tooltip("���������� ����� ����� ���� �������?")]
    public bool nextAfterDictor = false;

    [Space]
    [Tooltip("������� �� ��������� ������������� �� ���� ����?")]
    public bool forcedShowToolTip = false;
    [Tooltip("��� � ����������")]
    public bool likePrevious = true;
    [Tooltip("��������� ���������")]
    public string tipText;

    [Tooltip("������� � ���������� ���� ����� �� ����� �� ������")]
    public bool clickToNextStep;


    [Space]
    public UnityEvent eventAfterStep = new UnityEvent();
}
