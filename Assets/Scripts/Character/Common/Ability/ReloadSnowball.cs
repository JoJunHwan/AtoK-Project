using System;
using System.Collections;
using UnityEngine;

namespace SnowFight
{
    [DisallowMultipleComponent]
    public class ReloadSnowball : Ability
    {
        public event Action<int, int> SnowStockChanged; // (cur, max)

        [Header("눈덩이 잔량")]
        public int curSnowStock = 100;
        public int maxSnowStock = 100;

        [Header("리필 설정")]
        [SerializeField] private float refillInterval = 1.0f;
        [SerializeField] private int amountRefilledPerInterval = 10;
        [SerializeField] private bool stopAtFull = true;

        private Coroutine _routine;
        private bool _isReloading = false;
        [SerializeField] private CheckGround checkGround;

        public override void Init()
        {
            base.Init();
            checkGround = base.ownerCharacter.gameObject.GetComponent<CheckGround>();
            RaiseSnowStockChanged();
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

        public override void Execute()
        {
            BeginReload();
            Debug.Log("Execute");
        }

        public void BeginReload()
        {
            if (_routine == null && _isReloading == false)
            {
                Debug.Log("BeginReload");
                _isReloading = true;
                _routine = StartCoroutine(ReloadLoop());
            }
        }

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

        public void AddSnowStock(int amount)
        {
            curSnowStock += amount;
            if (curSnowStock > maxSnowStock)
            {
                curSnowStock = maxSnowStock;
            }
            RaiseSnowStockChanged();
        }

        public void ConsumeSnowStock(int amount)
        {
            curSnowStock -= amount;
            if (curSnowStock < 0)
            {
                curSnowStock = 0;
            }
            RaiseSnowStockChanged();
        }

        public int GetCurrentSnowStock()
        {
            return this.curSnowStock;
        }

        private void RaiseSnowStockChanged()
        {
            if (SnowStockChanged != null)
            {
                SnowStockChanged.Invoke(curSnowStock, maxSnowStock);
            }
        }
    }
}
