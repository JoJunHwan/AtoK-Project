using SnowFight;
using UnityEngine;

public class ThrowSnowball_Enemy : ThrowSnowball
{
    private EnemyAI enemyAI;
    
    public override void Init()
    {
        base.Init();

        enemyAI = this.GetComponent<EnemyAI>();
    }
    
    protected override Vector3 GetLaunchDestination()
    {
        return enemyAI.player.transform.position;
    }
    
    // Enemy가 플레이어 위치 판단해서 던져야 함
    protected override Vector3 GetLaunchDirection()
    {
        return enemyAI.dir;
    }
}
