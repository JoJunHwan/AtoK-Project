using UnityEngine;
using System.Collections;

// SnowFight 네임스페이스(Ability/ReloadSnowball 정의) 사용
using SnowFight;

/// <summary>
/// 눈 필드 클래스. Field를 상속.
/// - 플레이어가 눈 위에서 R키를 누르면 ReloadSnowball 능력을 통해 장전 시작/중지.
/// - 코루틴/주기는 ReloadSnowball이 담당.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SnowGround : Ground
{
    [Header("필드 동작 설정")]
    [Tooltip("플레이어 태그 이름")]
    [SerializeField] private string playerTag = "Player";

    // 현재 필드 위에 플레이어가 있는지
    private bool _isPlayerOnField = false;

    // 필드 위의 플레이어 참조
    private GameObject _player;

    // 플레이어의 ReloadSnowball 참조(있으면 캐싱)
    private ReloadSnowball _playerReload;

    /// <summary>
    /// 에디터에서 붙이자마자 트리거로 설정
    /// </summary>
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    protected override void SetGroundType()
    {
        base.groundType = GroundType.Snow;
    }

    /// <summary>
    /// 필드의 효과 실행.
    /// (필드 자체의 효과..? 뭔가 CharacterAbility랑 커플링 지으려니까 더 힘든 듯..?_
    /// 그냥 이속 느려지거나 그렇게..?
    /// </summary>
    public override void ExecuteGroundEffect(GameObject targetOnGround)
    {
        if (targetOnGround == null) return;
        

        // 대상에게 ReloadSnowball 능력이 있는지 확인(자식/부모까지 탐색)
        ReloadSnowball reload = targetOnGround.GetComponent<ReloadSnowball>();
        if  (reload == null) return;

        if (reload != null)
        {
            // 이 필드에서만 장전 가능하도록 허용
            //reload.SetReloadAllowed(true);

            // 장전 시작(입력 조건 등은 Update에서 제어하므로 여기서는 즉시 스타트해도 되고,
            // 필요하면 외부에서만 BeginReload를 호출하도록 정책 변경 가능)
            reload.BeginReload();
        }
    }

    
    
}
