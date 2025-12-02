using SnowFight;
using UnityEngine;

public class AIController_Boss : AIController
{
    public override void UpdateEntity()
    {
        
            
        character.UpdateByLCharacterEntityController();
    }
}
