using SnowFight;
using UnityEngine;

public class ThrowSnowball_Player : ThrowSnowball
{
    private Camera mainCamera;
    
    [Header("Gizmos For Debugging")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private float markerPrefabHeight = 0.02f;
    private Transform markerInstance;

    [Header("Charge Throw Settings")]
    [SerializeField] private float chargeInterval = 1.0f;
    [SerializeField] private int snowCostPerCharge = 1;
    
    // 메서드 추출
    [SerializeField] private float scalePerCharge = 0.15f;
    [SerializeField] private float maxScale = 3.0f;
    
    
    public enum AttackPhase
    {
        Idle,
        Pressed,
        Charging,
        Launch
    }
    
    [Header("Debug / State")]
    [SerializeField] private AttackPhase attackPhase = AttackPhase.Idle;

    private bool isCharging;
    private float chargeTimer;

    // 초기화 (카메라 참조)
    public override void Init()
    {
        base.Init();
        mainCamera = Camera.main;
    }

    // 매 프레임 갱신 (마커 및 충전 처리)
    public override void Tick()
    {
        base.Tick();
        TryUpdateMarker();
        Co_TryChargeSnowball();
    }
    
    // 입력 처리 (상태 기반)
    public override void HandleInput()
    {
        //처음 눈덩이 생성, 이후 충전루틴 시작
        if (ownerCharacter.inputState_Attack == InputState.Pressed)
        {
            if (attackPhase == AttackPhase.Idle) attackPhase = AttackPhase.Pressed;
            Debug.Log("Phase 1");
        }
        
        else if (ownerCharacter.inputState_Attack == InputState.Held)
        {
            if (attackPhase == AttackPhase.Pressed) attackPhase = AttackPhase.Charging;
            Debug.Log("Phase 2");
        }

        else if (ownerCharacter.inputState_Attack == InputState.Released)
        {
            if (attackPhase == AttackPhase.Charging) attackPhase = AttackPhase.Launch; //이때 발사
            Debug.Log("Phase 3");
        }

        else if (ownerCharacter.inputState_Attack == InputState.None)
        {
            // 발사 처리 이후 Idle 상태로 바뀜
            if (attackPhase == AttackPhase.Launch) attackPhase = AttackPhase.Idle;
            Debug.Log("Phase 4");
        }
        
        HandleAttackPhase();
    }

    // AttackPhase에 따라 실제 로직 실행
    private void HandleAttackPhase()
    {
        if (attackPhase == AttackPhase.Pressed)
        {
            Execute_AttackPressed();
            return;
        }

        else if (attackPhase == AttackPhase.Charging)
        {
            Execute_AttackCharging();
            return;
        }

        else if (attackPhase == AttackPhase.Launch)
        {
            Execute_AttackLaunch();
            attackPhase = AttackPhase.Idle;
        }
    }

    // 공격키 눌렀을 때
    private void Execute_AttackPressed()
    {
        if (base.IsCoolTimeReady() == false) return;
        
        if (IsSnowStockEnough() == false) return;
        SpendSnowStock();
        
        Debug.Log("CreateSnowball");
        curCreatedSnowball = base.CreateSnowball();
        
        //스노우 볼이 플레이어의 자식으로 들어오도록
        curCreatedSnowball.transform.SetParent(this.transform);

        StartCharge();
    }

    // 공격키 누르고 있는 동안
    private void Execute_AttackCharging()
    {
        // 충전 중, 만약 차징이 끊기면 충전 취소
        if (isCharging == false) attackPhase = AttackPhase.Idle;
    }

    // 공격키 뗐을 때
    private void Execute_AttackLaunch()
    {
        // 자식 해제
        curCreatedSnowball.transform.SetParent(null);
        
        base.LaunchSnowball();
        base.UpdateLastThrowTime();
        
        isCharging = false;
        //curCreatedSnowball = null;
    }
    
    // 충전 시작 처리 (초기 소비 및 타이머 초기화)
    private void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
    }

    // 충전 주기마다 눈덩이 크기 증가 시도
    private void Co_TryChargeSnowball()
    {
        if (isCharging == false) return;

        // chargeInterval마다, 눈덩이 키움
        chargeTimer += Time.deltaTime;
        if (chargeTimer < chargeInterval) return;
        chargeTimer = 0f;
        
        TryGrowSnowball();
    }

    // 눈덩이 확장 조건 검사 및 실행
    // 눈덩이 확장시, 확장 효과는 각 Snowball이 오버라이드 해서 다르게 되도록 (크기가 커질수도, 파워/속도가 커질수도, 특수효과가 생길 수도 있음)
    private void TryGrowSnowball()
    {
        if (CanConsumeChargeSnow() == false) return;
        
        reloadSnowball.ConsumeSnowStock(snowCostPerCharge);
        
        curCreatedSnowball.ExecuteCharging();
    }

    // 눈덩이 확장 가능한 눈 재고 확인
    private bool CanConsumeChargeSnow()
    {
        if (reloadSnowball.GetCurrentSnowStock() < snowCostPerCharge) return false;
        return true;
    }

#region MouseAim
    // 마우스 커서 기준 발사 목적지 계산
    protected override Vector3 GetLaunchDestination()
    {
        Vector3 launchPoint = Vector3.zero;
        if (mainCamera == null) Debug.LogError("mainCamera가 null임");

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            launchPoint = hit.point + launchDestinationOffset;
            UpdateMarker(hit);
            DrawDebug(hit);
        }
        return launchPoint;
    }

    // 발사 방향 계산 (마우스 위치 기반)
    protected override Vector3 GetLaunchDirection()
    {
        return GetMouseAimDirection();
    }
    
    // 카메라 → 마우스 방향 벡터 계산
    protected Vector3 GetMouseAimDirection()
    {
        Vector3 direction = base.launchDestination - transform.position;
        direction = direction.normalized;
        return direction;
    }
#endregion
    

#region MouseCursorMarker
    // 마커 위치 갱신 시도
    private void TryUpdateMarker()
    {
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            UpdateMarker(hit);
        }
    }
    
    // 마우스 위치에 마커 표시
    private void UpdateMarker(RaycastHit hit)
    {
        if (markerPrefab != null && markerInstance == null)
        {
            markerInstance = Instantiate(markerPrefab).transform;
        }

        if (markerInstance != null)
        {
            markerInstance.position = hit.point + hit.normal * markerPrefabHeight;
            markerInstance.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }

    // 마커 디버그 라인 표시
    private void DrawDebug(RaycastHit hit)
    {
        Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.yellow);
        Debug.DrawLine(hit.point - Vector3.right * 0.25f, hit.point + Vector3.right * 0.25f, Color.green);
        Debug.DrawLine(hit.point - Vector3.forward * 0.25f, hit.point + Vector3.forward * 0.25f, Color.green);
    }
    

#endregion
    
}
