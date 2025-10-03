using System;
using UnityEngine;

/// <summary>
/// 지면에 붙는 클래스.
/// 자신이 어떤 지면인지 알려줌.
/// </summary>
public abstract class Ground : MonoBehaviour
{
    [Tooltip("이 지면의 타입")]
    [SerializeField] protected GroundType groundType;

    private void Start()
    {
        SetGroundType();
    }
    
    // 반드시 자식에서, 자신의 GroundType을 설정하도록 강제
    protected abstract void SetGroundType();
    
    public GroundType GetGroundType()
    {
        return groundType;
    }

    /// <summary>
    /// 필드의 효과를 실행.
    /// 자식 클래스에서 구체적인 효과를 구현.
    /// (외부에서는 이것만 실행하면 됨..?)
    /// </summary>
    public abstract void ExecuteGroundEffect(GameObject targetOnGround);
}