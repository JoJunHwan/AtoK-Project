using UnityEngine;
using UnityEngine.UI;

namespace SnowFight
{
    public class UI_SnowAmmo : MonoBehaviour
    {
        [SerializeField] private ReloadSnowball reloadSnowball;
        [SerializeField] private Image barImage;
        [SerializeField] private Text amountText;
        
        private void Start()
        {
            reloadSnowball.SnowStockChanged += UpdateSnowAmmoUi;
        }
        
        public void ChangeSlideBarAmount(float amount) //* HP 게이지 변경 
        {
            barImage.fillAmount = amount;
        }

        private void UpdateSnowAmmoUi(int current, int max)
        {
            barImage.fillAmount = GetSnowAmmoRatio(current, max);

            amountText.text = current.ToString();
        }
        
        public float GetSnowAmmoRatio(int current, int max)
        {
            float currentF = current;
            float maxF = max;
            
            float ratio = currentF / maxF;

            if (ratio < 0f)
            {
                ratio = 0f;
            }
            else if (ratio > 1f)
            {
                ratio = 1f;
            }

            return ratio;
        }
    }
}