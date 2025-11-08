using UnityEngine;
using SnowFight;

public class ThrowSnowball_Boss : ThrowSnowball
{
    private BossAI bossAI;

    // ⚠️ Init() 대신 Start()를 사용하여 참조 시점 지연
    void Start()
    {
        // Init()에서 처리할 로직을 Start()로 이동
        bossAI = this.GetComponentInParent<BossAI>();
        Debug.Assert(bossAI != null, "ERROR: BossAI 컴포넌트를 부모에서 찾을 수 없습니다.");

        // base.Init()은 Character.cs에서 호출되므로, 여기서는 Init() 로직만 가져옵니다.
        // 만약 Init()이 Character에 의해 호출되어야 한다면, Init()을 비워두고 Start()에만 참조를 넣습니다.
    }

    // (기존 Init() 함수는 주석 처리하거나 비워둡니다)
    // public override void Init() { /* base.Init(); */ } 


    // 기존 GetLaunchDestination 함수 유지 (안전 장치 포함 권장)
    protected override Vector3 GetLaunchDestination()
    {
        // bossAI가 null이면 오류 발생!
        if (bossAI == null)
        {
            Debug.LogError("BossAI 참조가 NULL입니다. Start() 확인 필요");
            return transform.position;
        }

        // player는 할당되어 있으므로, bossAI가 null이 아닌지 확인하는 것이 핵심입니다.
        return bossAI.player.position + launchDestinationOffset;
    }

    protected override Vector3 GetLaunchDirection()
    {
        // 목표 지점과 발사 위치를 기준으로 방향을 자체 계산
        Vector3 spawnPos = GetSpawnPosition(this.ownerCharacter);
        Vector3 dir = GetLaunchDestination() - spawnPos;
        return dir.normalized;
    }

    public void ThrowFromBoss()
    {
        this.Execute();
    }
}