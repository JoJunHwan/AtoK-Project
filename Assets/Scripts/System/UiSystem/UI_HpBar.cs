using System;
using UnityEngine;
using UnityEngine.UI;

//View..?
// 나중에 MVC 패턴 적용하기
//UI에서 관심있는 대상을 관찰하는 느낌으로 가야함.
//그게 관심있는 대상이 정보를 나눠주는 방식이면,
public class UI_HpBar : MonoBehaviour
{
    public Health health;
    [SerializeField] private Image barImage;

    private void Start()
    {
        health.OnDamagedEvent += UpdateHpBar;
        health.OnDeathEvent  += UpdateHpBar;
    }

    public void ChangeSlideBarAmount(float amount) //* HP 게이지 변경 
    {
        barImage.fillAmount = amount;
    }

    public void UpdateHpBar(DamageData damageData)
    {
        this.ChangeSlideBarAmount(health.GetHealthRatio());
    }
}
