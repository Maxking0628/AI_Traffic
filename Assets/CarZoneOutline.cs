using System.Collections.Generic;
using UnityEngine;
using TMPro;   // �ޤJ TextMeshPro �R�W�Ŷ�

public class CarZoneOutline : MonoBehaviour
{
    public TextMeshPro carCountText;  // 3D ��r
    private HashSet<GameObject> carsInZone = new HashSet<GameObject>();
    private LineRenderer lineRenderer;
    private BoxCollider box;


    void Awake()
    {
        box = GetComponent<BoxCollider>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 8; // �ߤ��骺 8 �Ө�
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;

        DrawOutline();
    }

    void DrawOutline()
    {
        Vector3 center = box.center;
        Vector3 size = box.size / 2f;

        Vector3[] corners = new Vector3[8];

        // �p��ߤ��� 8 �Ө�
        corners[0] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, -size.z));
        corners[1] = transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z));
        corners[2] = transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z));
        corners[3] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z));
        corners[4] = transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z));
        corners[5] = transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z));
        corners[6] = transform.TransformPoint(center + new Vector3(size.x, size.y, size.z));
        corners[7] = transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z));

        // �s������ (���� + �W��)
        lineRenderer.positionCount = 16;
        lineRenderer.SetPositions(new Vector3[] {
            corners[0], corners[1], corners[2], corners[3], corners[0],
            corners[4], corners[5], corners[6], corners[7], corners[4],
            corners[5], corners[1], corners[2], corners[6], corners[7], corners[3]
        });
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            carsInZone.Add(other.gameObject);
            UpdateCount();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            carsInZone.Remove(other.gameObject);
            UpdateCount();
        }
    }

    void UpdateCount()
    {
        int carCount = carsInZone.Count;

        // ��s��r
        if (carCountText != null)
            carCountText.text = carCount.ToString();

        // ����C�ⱱ��
        if (lineRenderer != null)
        {
            if (carCount > 150)
                lineRenderer.material.color = new Color(0.5f, 0f, 0.5f);
            else if (carCount > 100)
                lineRenderer.material.color = Color.red;
            else if (carCount > 50)
                lineRenderer.material.color = Color.yellow;
            else
                lineRenderer.material.color = Color.green;
        }
    }
    void LateUpdate()
    {
        // ���Ʀr�û����V��v�� (�����Ҥ@��)
        if (carCountText != null && Camera.main != null)
        {
            carCountText.transform.rotation = Quaternion.LookRotation(
                carCountText.transform.position - Camera.main.transform.position
            );
        }
    }
}
