using System;
using UnityEngine;
using UnityEngine.UI;

//View..?
// 나중에 MVC 패턴 적용하기
//UI에서 관심있는 대상을 관찰하는 느낌으로 가야함.
//그게 관심있는 대상이 정보를 나눠주는 방식이면,
public class UI_HpBar : UI_ElementBase
{
  
    public Health health;
    public Health bossHealth;
    [SerializeField] private Image barImage;
    [SerializeField] private Image bossBarImage;
    
    
    public override void InitByUiController()
    {
        health.OnDamagedEvent += UpdateHpBar;
        health.OnDeathEvent  += UpdateHpBar;
        
        if (bossHealth != null) 
        {
            bossHealth.OnDamagedEvent += UpdateBossHpBar; 
            bossHealth.OnDeathEvent += UpdateBossHpBar; 
        }
    }
    
    //merge: ImageUI 오브젝트 매개변수 추가 및 매개변수 기반 fillAmount
    public void ChangeSlideBarAmount(Image bar,float amount) //* HP 게이지 변경 
    {
        bar.fillAmount = amount;
    }

    public void UpdateHpBar(DamageData damageData)
    {
        this.ChangeSlideBarAmount(barImage, health.GetHealthRatio());
    }
    
    //merge: 보스 체력바 Update
    public void UpdateBossHpBar(DamageData damageData)
    {
        this.ChangeSlideBarAmount(bossBarImage, bossHealth.GetHealthRatio());
    }
}
