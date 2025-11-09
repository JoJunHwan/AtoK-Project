using UnityEngine;
using SnowFight;

public class ThrowSnowball_Boss : ThrowSnowball
{
    private BossAI bossAI;

    void Start()
    {
        // Init()에서 처리할 로직을 Start()로 이동
        bossAI = this.GetComponentInParent<BossAI>();
        Debug.Assert(bossAI != null, "ERROR: BossAI 컴포넌트를 부모에서 찾을 수 없습니다.");
    }

    protected override Vector3 GetLaunchDestination()
    {
        // bossAI가 null이면 오류 발생!
        if (bossAI == null)
        {
            Debug.LogError("BossAI 참조가 NULL입니다. Start() 확인 필요");
            return transform.position;
        }

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