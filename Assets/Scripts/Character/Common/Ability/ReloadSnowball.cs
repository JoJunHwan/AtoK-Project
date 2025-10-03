using System.Collections;
using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 눈덩이 장전(재보급) 능력.
    /// - 입력 처리 없음(플레이어/몬스터 공용).
    /// - 외부 스크립트(예: SnowField)가 BeginReload/EndReload를 호출해 제어.
    /// - N초 간격으로 ammoPerTick 만큼 충전.
    /// </summary>
    [DisallowMultipleComponent]
    public class ReloadSnowball : Ability
    {
        [Header("눈덩이 잔량")]
        public int curSnowStock = 100;                          // 던질 수 있는 눈덩이 재고
        public int maxSnowStock = 100;                       // 눈덩이 최대 재고
        
        [Header("리필 설정")]
        [Tooltip("몇 초마다 충전할지")]
        [SerializeField] private float refillInterval = 1.0f;

        [Tooltip("틱마다 충전되는 탄약 개수")]
        [SerializeField] private int amountRefilledPerInterval = 10;

        [Tooltip("최대치에 도달하면 자동으로 정지할지")]
        [SerializeField] private bool stopAtFull = true;
        
        private Coroutine _routine;       // 리필 코루틴
        private bool _isReloading = false;
        
        [SerializeField] private CheckGround checkGround;

        #region Unity
        public override void Init()
        {
            base.Init();
            
            checkGround = base.ownerCharacter.gameObject.GetComponent<CheckGround>();
        }

        public override void HandleInput()
        {
            if (base.ownerCharacter.inputState_Reload == InputState.Held)
            {
                this.TryExecute();
            }

            if (base.ownerCharacter.inputState_Reload == InputState.Released)
            {
                this.EndReload();
            }
        }

        private void OnDisable()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            _isReloading = false;
        }
        #endregion

       
        public override void TryExecute()
        {
            if (this.CanExecute() == true)
            {
                Execute();
            }
        }

        public override bool CanExecute()
        {
            if (checkGround.GetCurrentGround() == GroundType.Snow)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Ability 진입점. 기본은 연속 리필 시작.
        /// </summary>
        public override void Execute()
        {
            BeginReload();
            Debug.Log("Execute");
        }

        /// <summary>
        /// 연속 리필 시작
        /// </summary>
        public void BeginReload()
        {
            if (_routine == null && _isReloading == false)
            {
                Debug.Log("BeginReload");
                _isReloading = true;
                _routine = StartCoroutine(ReloadLoop());
            }
        }

        /// <summary>
        /// 연속 리필 중지
        /// </summary>
        public void EndReload()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            _isReloading = false;
            
            Debug.Log("EndReload");
        }
        

        #region Core
        /// <summary>
        /// N초 간격으로 ammoPerTick만큼 충전하는 루프
        /// </summary>
        private IEnumerator ReloadLoop()
        {
            if (refillInterval <= 0f) refillInterval = 0.1f;

            while (_isReloading)
            {
                this.AddSnowStock(this.amountRefilledPerInterval);
                
                yield return new WaitForSeconds(refillInterval);
            }

            _isReloading = false;
            _routine = null;
        }
        #endregion
        
#region ManageSnowStock
        public void AddSnowStock(int amount)
        {
            curSnowStock += amount;
            if (curSnowStock > maxSnowStock)
            {
                curSnowStock = maxSnowStock;
            }
        }

        public void ConsumeSnowStock(int amount)
        {
            curSnowStock -= amount;
            if (curSnowStock < 0)
            {
                curSnowStock = 0;
            }
        }

        public int GetCurrentSnowStock()
        {
            return this.curSnowStock;
        }
#endregion
    }
}
