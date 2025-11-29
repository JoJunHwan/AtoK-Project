using UnityEngine;
using UnityEngine.UI;

public class UI_EnemyAliveCounter : UI_ElementBase
{
    [SerializeField] private Text enemyAliveCount;
    public EnemyGroupController enemyGroupController;

    public override void InitByUiController()
    {
        enemyGroupController.OnAliveEnemyCountChanged += UpdateEnemyAliveCount;
    }
    
    public void UpdateEnemyAliveCount(int count)
    {
        enemyAliveCount.text = count.ToString();
    }
}
