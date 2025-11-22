using UnityEngine;
using UnityEngine.UI;

public class UI_EnemyAliveCounter : UI_ElementBase
{
    [SerializeField] private Text enemyAliveCount;
    public EnemyDeathCounter enemyDeathCounter;

    public override void InitByUiController()
    {
        enemyDeathCounter.OnAliveEnemyCountChanged += UpdateEnemyAliveCount;
    }
    
    public void UpdateEnemyAliveCount(int count)
    {
        enemyAliveCount.text = count.ToString();
    }
}
