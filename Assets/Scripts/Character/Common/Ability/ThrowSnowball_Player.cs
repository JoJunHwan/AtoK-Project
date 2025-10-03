using SnowFight;
using UnityEngine;

public class ThrowSnowball_Player : ThrowSnowball
{
    private Camera mainCamera;
    
    [Header("Gizmos For Debugging")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject markerPrefab;
    
    private Transform markerInstance;
    
    public override void Init()
    {
        base.Init();
            
        mainCamera = Camera.main;
    }

    public override void Tick()
    {
        base.Tick();
        TryUpdateMarker();
    }

    protected override Vector3 GetLaunchDirection()
    {
        return GetMouseAimDestination();
    }
    
    // 카메라에서 마우스 커서를 향하는 3D 방향을 구함
    protected Vector3 GetMouseAimDestination()
    {
        launchDestination = Vector3.zero;

        if (mainCamera == null)
        {
            return transform.forward;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            launchDestination = hit.point + launchDestinationOffset;
            UpdateMarker(hit);
            DrawDebug(hit);
        }
            
        Vector3 direction =  launchDestination - transform.position;
        direction = direction.normalized;
        return direction;
    }
    
    // 디버그용 마커
    private void TryUpdateMarker()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            UpdateMarker(hit);
            //DrawDebug(hit);
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
