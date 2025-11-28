using SnowFight;
using UnityEngine;

public class ThrowSnowball_Enemy : ThrowSnowball
{
    private AIController aiController;
    
    public override void Init()
    {
        base.Init();

        aiController = this.GetComponent<AIController>();
    }
    
    protected override Vector3 GetLaunchDestination()
    {
        return aiController.player.transform.position;
    }
    
    // Enemy가 플레이어 위치 판단해서 던져야 함
    protected override Vector3 GetLaunchDirection()
    {
        return aiController.dir;
    }
}
