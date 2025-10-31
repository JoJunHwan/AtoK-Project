using SnowFight;
using UnityEngine;

public class ThrowSnowball_Player : ThrowSnowball
{
    private Camera mainCamera;
    
    [Header("Gizmos For Debugging")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject markerPrefab;
    private Transform markerInstance;

    [Header("Charge Throw Settings")]
    [SerializeField] private bool useCharge = true;
    [SerializeField] private float chargeInterval = 1.0f;
    [SerializeField] private float scalePerCharge = 0.15f;
    [SerializeField] private int snowCostPerCharge = 1;
    [SerializeField] private float maxScale = 3.0f;

    public enum AttackPhase
    {
        Idle,
        Pressed,
        Charging,
        Released
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
        TryChargeSnowball();
    }
    
    // 입력 처리 (상태 기반)
    public override void HandleInput()
    {
        base.HandleInput();
        UpdateAttackPhaseFromInput();
        HandleAttackPhase();
    }

    // 입력값을 AttackPhase로 변환
    private void UpdateAttackPhaseFromInput()
    {
        if (ownerCharacter.inputState_Attack == InputState.Pressed)
        {
            attackPhase = AttackPhase.Pressed;
            return;
        }

        if (ownerCharacter.inputState_Attack == InputState.Held)
        {
            if (attackPhase == AttackPhase.Pressed) attackPhase = AttackPhase.Charging;
            return;
        }

        if (ownerCharacter.inputState_Attack == InputState.Released)
        {
            attackPhase = AttackPhase.Released;
            return;
        }

        if (ownerCharacter.inputState_Attack == InputState.None)
        {
            if (attackPhase != AttackPhase.Charging) attackPhase = AttackPhase.Idle;
        }
    }

    // AttackPhase에 따라 실제 로직 실행
    private void HandleAttackPhase()
    {
        if (attackPhase == AttackPhase.Pressed)
        {
            OnAttackPressed();
            return;
        }

        if (attackPhase == AttackPhase.Charging)
        {
            OnAttackCharging();
            return;
        }

        if (attackPhase == AttackPhase.Released)
        {
            OnAttackReleased();
            attackPhase = AttackPhase.Idle;
        }
    }

    // 공격키 눌렀을 때
    private void OnAttackPressed()
    {
        if (useCharge == false)
        {
            Execute();
            return;
        }

        TryCreateSnowballForCharge();
    }

    // 공격키 누르고 있는 동안
    private void OnAttackCharging()
    {
        if (useCharge == false) return;
        if (isCharging == false) attackPhase = AttackPhase.Idle;
    }

    // 공격키 뗐을 때
    private void OnAttackReleased()
    {
        if (useCharge == false) return;
        if (isCharging == false) return;
        LaunchChargedSnowball();
    }

    // 눈덩이 생성 시도 (충전 시작용)
    private void TryCreateSnowballForCharge()
    {
        if (IsSnowCreatable() == false) return;
        launchDestination = GetLaunchDestination();
        Vector3 dir = GetLaunchDirection();
        curCreatedSnowball = CreateSnowballWithDirection(dir);
        StartCharge();
    }

    // 눈덩이 생성 가능 여부 확인
    private bool IsSnowCreatable()
    {
        if (snowballPrefab == null) return false;
        if (hasReloadSnowball == false) return true;
        if (reloadSnowball.GetCurrentSnowStock() <= 0) return false;
        return true;
    }

    // 지정된 방향으로 눈덩이 생성
    private Snowball CreateSnowballWithDirection(Vector3 direction)
    {
        Vector3 spawnPos = GetSpawnPosition(base.ownerCharacter);
        Quaternion spawnRot = Quaternion.LookRotation(direction, Vector3.up);
        Snowball instance = Instantiate(snowballPrefab, spawnPos, spawnRot);
        instance.Init(this.groundMask);        // 필요하면 충돌 레이어 교체
        return instance;
    }

    // 충전 시작 처리 (초기 소비 및 타이머 초기화)
    private void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
        SpendSnowStock();
    }

    // 충전 주기마다 눈덩이 크기 증가 시도
    private void TryChargeSnowball()
    {
        if (useCharge == false) return;
        if (isCharging == false) return;
        if (curCreatedSnowball == null)
        {
            isCharging = false;
            return;
        }

        chargeTimer += Time.deltaTime;
        if (chargeTimer < chargeInterval) return;
        chargeTimer = 0f;
        TryGrowSnowball();
    }

    // 눈덩이 확장 조건 검사 및 실행
    private void TryGrowSnowball()
    {
        if (CanConsumeChargeSnow() == false) return;
        ConsumeChargeSnow();
        GrowSnowballScale();
    }

    // 눈덩이 확장 가능한 눈 재고 확인
    private bool CanConsumeChargeSnow()
    {
        if (hasReloadSnowball == false) return true;
        if (reloadSnowball.GetCurrentSnowStock() < snowCostPerCharge) return false;
        return true;
    }

    // 충전 중 눈덩이 소모 처리
    private void ConsumeChargeSnow()
    {
        if (hasReloadSnowball == false) return;
        reloadSnowball.ConsumeSnowStock(snowCostPerCharge);
    }

    // 눈덩이 크기 증가 처리
    private void GrowSnowballScale()
    {
        if (curCreatedSnowball == null) return;
        Transform t = curCreatedSnowball.transform;
        Vector3 next = t.localScale + Vector3.one * scalePerCharge;
        if (next.x > maxScale) next = Vector3.one * maxScale;
        t.localScale = next;
    }

    // 충전 완료 후 눈덩이 발사
    private void LaunchChargedSnowball()
    {
        if (curCreatedSnowball == null)
        {
            isCharging = false;
            return;
        }

        curCreatedSnowball.LaunchCurvedToDestination(launchDestination, initialSpeed, lifeTime);
        isCharging = false;
        curCreatedSnowball = null;
    }
    
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
            markerInstance.position = hit.point + hit.normal * 0.02f;
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
}
