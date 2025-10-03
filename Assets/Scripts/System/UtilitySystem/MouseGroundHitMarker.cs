using UnityEngine;

[DisallowMultipleComponent]
public class MouseGroundHitMarker : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject markerPrefab;

    private Transform markerInstance;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        TryUpdateMarker();
    }

    // === 분리된 기능 ===

    private void TryUpdateMarker()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            UpdateMarker(hit);
            DrawDebug(hit);
        }
    }

    private void UpdateMarker(RaycastHit hit)
    {
        if (markerPrefab != null && markerInstance == null)
        {
            markerInstance = Instantiate(markerPrefab).transform;
        }

        if (markerInstance != null)
        {
            markerInstance.position = hit.point + hit.normal * 0.02f;
            markerInstance.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }

    private void DrawDebug(RaycastHit hit)
    {
        Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.yellow);
        Debug.DrawLine(hit.point - Vector3.right * 0.25f, hit.point + Vector3.right * 0.25f, Color.green);
        Debug.DrawLine(hit.point - Vector3.forward * 0.25f, hit.point + Vector3.forward * 0.25f, Color.green);
    }
}