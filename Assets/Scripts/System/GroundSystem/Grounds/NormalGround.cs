using UnityEngine;

public class NormalGround : Ground
{
    protected override void SetGroundType()
    {
        base.groundType =  GroundType.Normal;
    }

    public override void ExecuteGroundEffect(GameObject target)
    {
        
    }
}
