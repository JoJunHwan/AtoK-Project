using UnityEngine;

public class SpawnController : MonoBehaviour
{
    //스폰 지점
    [SerializeField] private Transform spawnPoint;
    //플레이어 받음 (레벨컨트롤러에 의해서 받음)
    private GameObject playerGameObject;
    
    // 플레이어를 스폰시키는 함수 (LevelController에 의해서 실행)
    public void InitByLevelController(GameObject _playerGameObject)
    {
        this.playerGameObject = _playerGameObject;
        this.spawnPoint = GameObject.FindWithTag("PlayerSpawnPoint").transform;
        
        Debug.Assert(spawnPoint!=null, "spawnPoint 비어있음");
    }

    public void SpawnPlayer()
    {
        this.playerGameObject.transform.position = spawnPoint.position;
    }
}
