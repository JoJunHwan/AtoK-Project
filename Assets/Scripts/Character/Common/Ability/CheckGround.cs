using UnityEngine;

/// <summary>
/// 플레이어 발밑에 붙여서 현재 어떤 지면과 닿아있는지 확인.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckGround : MonoBehaviour
{
    [Tooltip("현재 밟고 있는 지면의 타입")]
    public GroundType currentGround = GroundType.Normal;

    private void Reset()
    {
        // 발밑 판정은 보통 Trigger Collider로 설정
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var ground = other.GetComponent<Ground>();
        if (ground != null)
        {
            currentGround = ground.GetGroundType();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var ground = other.GetComponent<Ground>();
        if (ground != null)
        {
            currentGround = ground.GetGroundType();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var ground = other.GetComponent<Ground>();
        if (ground != null)
        {
            // 떠나면 Normal로 초기화
            currentGround = GroundType.Normal;
        }
    }

    /// <summary>
    /// 현재 발밑이 눈인지 여부 확인
    /// </summary>
    public GroundType GetCurrentGround()
    {
        return this.currentGround;
    }
}