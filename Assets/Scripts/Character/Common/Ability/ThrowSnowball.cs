using UnityEngine;

namespace SnowFight
{
    public class ThrowSnowball : Ability
    {
        [Header("Reload Exists?")]
        [SerializeField] protected ReloadSnowball reloadSnowball;
        [SerializeField] protected bool hasReloadSnowball;
        
        [Header("Create Snowball")]
        [SerializeField] private int cost;
        [SerializeField] protected Snowball curCreatedSnowball;
        
        [Header("Throw Snowball")]
        public Transform launchPivot;
        [SerializeField] protected Vector3 launchDestination;
        [SerializeField] protected Vector3 launchDestinationOffset;
        private Vector3 launchDirection;
        
        [Header("Snowball Data")]
        public Snowball snowballPrefab;
        [SerializeField] private LayerMask collisionLayer;
        public float initialSpeed = 12f;
        public float curveSideForce = 8f;
        public float lifeTime = 6f;
        
        [Header("Sound")]
        [SerializeField] private AudioClip sfx_launchSnowball;

        public override void Init()
        {
            base.Init();
            HasReloadSnowball();
        }

        public override void HandleInput()
        {
            base.HandleInput();
            
            if (base.ownerCharacter.inputState_Attack == InputState.Pressed)
            {
                this.Execute();
            }
        }

        public override void Execute()
        {
            Debug.Assert(snowballPrefab != null, "snowballPrefab이 비었음");
            if (IsSnowStockEnough() == false) return;
            SpendSnowStock();
            
            curCreatedSnowball = CreateSnowball();
            LaunchSnowball();
        }
        
        private void HasReloadSnowball()
        {
            reloadSnowball = base.ownerCharacter.GetComponent<ReloadSnowball>();
            if (reloadSnowball == null) hasReloadSnowball = false;
            else hasReloadSnowball = true;
        }

        protected Snowball CreateSnowball()
        {
            Debug.Log("CreateSnowball");
            Vector3 spawnPos = GetSpawnPosition(base.ownerCharacter);
            Quaternion spawnRot = Quaternion.LookRotation(launchDirection, Vector3.up);
            Snowball instance = Instantiate(snowballPrefab, spawnPos, spawnRot);
            instance.Init(this.collisionLayer);
            return instance;
        }

        protected void LaunchSnowball()
        {
            launchDestination = GetLaunchDestination();
            launchDirection = GetLaunchDirection();
            
            curCreatedSnowball.ActivateSnowball(true);
            curCreatedSnowball.LaunchCurvedToDestination(launchDestination, initialSpeed, lifeTime);

            this.SFX_LaunchSnowball();
        }
        
        protected virtual Vector3 GetLaunchDestination()
        {
            Debug.LogError($"{this.gameObject}의 {this} 클래스 오버라이드 필요");
            return Vector3.zero;
        }

        protected virtual Vector3 GetLaunchDirection()
        {
            Debug.LogError($"{this.gameObject}의 {this} 클래스 오버라이드 필요");
            return Vector3.zero;
        }

        protected Vector3 GetSpawnPosition(Character owner)
        {
            if (launchPivot != null)
            {
                return launchPivot.position;
            }

            Vector3 pos = owner.transform.position + Vector3.up * 1.2f;
            return pos;
        }
        
        // ⬇⬇⬇ 여기만 private → protected 로 바꿈
        protected void SpendSnowStock()
        {
            if (IsSnowStockEnough() == false) return;
            if (reloadSnowball != null)
            {
                reloadSnowball.ConsumeSnowStock(cost);
            }
        }

        protected bool IsSnowStockEnough()
        {
            if (hasReloadSnowball == false) return true;
            if (reloadSnowball.GetCurrentSnowStock() <= 0)
            {
                return false;
            }
            else return true;
        }

        protected void SFX_LaunchSnowball()
        {
            SoundManager.Instance.PlaySFX(sfx_launchSnowball, 1f);
        }
    }
}
