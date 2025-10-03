using SnowFight;
using UnityEngine;

public class ThrowSnowball_Enemy : ThrowSnowball
{
    // AI용: 외부에서 지정한 방향으로 발사 시도
    public bool TryFireInDirection(Vector3 direction)
    {
        Vector3 dir = direction;
        base.Execute();
        
        /*
        Character owner = GetOwnerCharacter();
        
        if (owner == null)
        {
            return false;
        }
        if (owner.snowStock <= 0)
        {
            return false;
        }
        if (snowballPrefab == null)
        {
            return false;
        }

        
        if (dir.sqrMagnitude <= 0.000001f)
        {
            dir = transform.forward;
        }
        dir = dir.normalized;

        Vector3 spawnPos = GetSpawnPosition(owner);
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        Snowball ball = Object.Instantiate(snowballPrefab, spawnPos, spawnRot);
        ball.Launch(dir * initialSpeed, owner.useCurveBall, curveSideForce, lifeTime);

        owner.snowStock -= 1;
        */
        return true;
    }
}
