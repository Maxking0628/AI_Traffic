using System.Collections.Generic;
using UnityEngine;
using TMPro;   // 引入 TextMeshPro 命名空間

public class CarZoneOutline : MonoBehaviour
{
    public TextMeshPro carCountText;  // 3D 文字
    private HashSet<GameObject> carsInZone = new HashSet<GameObject>();
    private LineRenderer lineRenderer;
    private BoxCollider box;


    void Awake()
    {
        box = GetComponent<BoxCollider>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 8; // 立方體的 8 個角
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;

        DrawOutline();
    }

    void DrawOutline()
    {
        Vector3 center = box.center;
        Vector3 size = box.size / 2f;

        Vector3[] corners = new Vector3[8];

        // 計算立方體 8 個角
        corners[0] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, -size.z));
        corners[1] = transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z));
        corners[2] = transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z));
        corners[3] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z));
        corners[4] = transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z));
        corners[5] = transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z));
        corners[6] = transform.TransformPoint(center + new Vector3(size.x, size.y, size.z));
        corners[7] = transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z));

        // 連接順序 (底部 + 上部)
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

        // 更新文字
        if (carCountText != null)
            carCountText.text = carCount.ToString();

        // 邊框顏色控制
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
        // 讓數字永遠面向攝影機 (像標籤一樣)
        if (carCountText != null && Camera.main != null)
        {
            carCountText.transform.rotation = Quaternion.LookRotation(
                carCountText.transform.position - Camera.main.transform.position
            );
        }
    }
}
