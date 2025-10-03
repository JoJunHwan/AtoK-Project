using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 눈덩이 투척 능력 (카메라 기준 마우스 방향으로 발사)
    /// (정말 눈덩이 생성/투척 그 자체에만 관심 가짐)
    /// (나중에 얘는 1) 눈덩이 종류, 2) 눈덩이 날아갈 방향만 정하도록)
    /// (또한 구체적인 방향은 외부에서 전달받은 값을 사용하도록)
    /// </summary>
    public class ThrowSnowball : Ability
    {
        [Header("Reload 클래스 존재 여부")]
        [SerializeField] private ReloadSnowball reloadSnowball;
        [SerializeField] private bool hasReloadSnowball;
        
        [Header("생성")]
        public Snowball snowballPrefab;             // 생성할 눈 프리팹
        [SerializeField] private LayerMask collisionLayer;
        public Transform launchPivot;               // 발사 위치 기준 트랜스폼
        public Vector3 launchDestination;
        public Vector3 launchDestinationOffset;     // 발사되어서 도착지점의 오프셋(지면으로부터 얼마나 떨어진 위치로 갈지..?)
        [SerializeField] private int cost;          //생성 비용
        private Vector3 launchDirection;
        [SerializeField] private Snowball curCreatedSnowball;
        
        [Header("투척")]
        public float initialSpeed = 12f;       // 초기 속도(모든 방향 동일)
        public float curveSideForce = 8f;      // 커브일 때 측면 가속도
        public float lifeTime = 6f;            // 눈덩이 생존 시간
        

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

        // 눈덩이를 생성하고 던짐
        public override void Execute()
        {
            Debug.Assert(snowballPrefab !=null, "snowballPrefab이 비었음");
            
            // 눈 잔량 확인
            if (IsSnowStockEnough() == false) return;
            
            // 눈 소비
            this.SpendSnowStock();
            
            // 눈덩이 생성
            launchDirection = GetLaunchDirection();                                       
            curCreatedSnowball = this.CreateSnowball();
            
            // 눈덩이 발사
            this.LaunchSnowball();
        }
        
        private void HasReloadSnowball()
        {
            reloadSnowball = base.ownerCharacter.GetComponent<ReloadSnowball>();
            
            if (reloadSnowball == null) hasReloadSnowball = false;
            else hasReloadSnowball = true;
        }

        protected Snowball CreateSnowball()
        {
            // 스폰 위치 계산
            Vector3 spawnPos = GetSpawnPosition(base.ownerCharacter);                   
            Quaternion spawnRot = Quaternion.LookRotation(launchDirection, Vector3.up);
            
            Snowball instance = Instantiate(snowballPrefab, spawnPos, spawnRot);
            instance.Init(this.collisionLayer);
            return instance;
        }

        protected void LaunchSnowball()
        {
            //curCreatedSnowball.Launch(launchDirection * initialSpeed, base.ownerCharacter.useCurveBall, curveSideForce, lifeTime); // 동일 속도로 발사
            curCreatedSnowball.LaunchToDestination(launchDestination, initialSpeed, lifeTime);
        }

        protected virtual Vector3 GetLaunchDirection()
        {
            return Vector3.zero;
        }

        // 스폰 위치를 반환함
        protected Vector3 GetSpawnPosition(Character owner)
        {
            if (launchPivot != null)
            {
                return launchPivot.position;
            }

            Vector3 pos = owner.transform.position + Vector3.up * 1.2f;
            return pos;
        }
        
        /// <summary>
        /// 눈덩이 소비함
        /// </summary>
        private void SpendSnowStock()
        {
            if (IsSnowStockEnough() == false) return;
            
            if (reloadSnowball != null)
            {
                reloadSnowball.ConsumeSnowStock(cost);
            }
        }
        
        /// <summary>
        /// 1. ReloadSnowball이 없으면, 항상 던질 수 있음
        /// 2. ReloadSnowball이 있으면, 재고량 파악해서 던짐
        /// </summary>
        /// <returns></returns>
        private bool IsSnowStockEnough()
        {
            if (hasReloadSnowball == false) return true;
              
            if (reloadSnowball.GetCurrentSnowStock() <= 0)
            {
                return false;
            }
            else return true;
        }
    }
}
