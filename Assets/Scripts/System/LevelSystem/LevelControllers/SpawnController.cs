using UnityEngine;

public class SpawnController : MonoBehaviour
{
    //스폰 지점
    [SerializeField] private Transform spawnPoint;
    //플레이어 받음 (레벨컨트롤러에 의해서 받음)
    private CharacterController playerController;
    
    // 플레이어를 스폰시키는 함수 (LevelController에 의해서 실행)
    public void InitByLevelController(CharacterController _playerController)
    {
        this.playerController = _playerController;
        this.spawnPoint = GameObject.FindWithTag("PlayerSpawnPoint").transform;
        
        Debug.Assert(spawnPoint!=null, "spawnPoint 비어있음");
    }

    public void SpawnPlayer()
    {
        this.playerController.transform.position = spawnPoint.position;
    }
}
