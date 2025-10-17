using SnowFight;
using UnityEngine;

public class Move_Enemy : Move
{
    private EnemyAI enemyAI;
    
    public override void Init()
    {
        base.Init();

        enemyAI = this.GetComponent<EnemyAI>();
    }
    
    public override void HandleInput()
    {
        //this.HandleInput_AI();
    }
    public void HandleInput_AI(float _curMoveX, float _curMoveZ)
    {
        base.SetCurrentMove(_curMoveX, _curMoveZ);
    }
}
