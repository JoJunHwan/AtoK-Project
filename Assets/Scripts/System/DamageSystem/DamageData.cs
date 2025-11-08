using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 나중에 SO 데이터로 바꿔야 할듯..?
/// 아니면 Attack데이터가 SO고, 그 안에 DamageData 구조체를 포함해야 할 듯..?
/// </summary>
[Serializable]
public struct DamageData
{
    public float damageAmount;
    public float knockbackPower;
    public GameObject attacker;  //데미지를 소환한 공격자 (Player/Enemy 등등)
    public GameObject hitSource; //데미지 인스턴스 그 자체 (파이어볼 등등)
    //public Vector2 hitPosition;
    
    //public AttackType attckType;
    //public ElementType elementType;
    //public HitReactionType hitReactionType;
}