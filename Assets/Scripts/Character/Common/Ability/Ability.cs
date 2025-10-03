using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 모든 능력의 최상위 부모 클래스
    /// (하위 능력이 구현할 요소는 모두 가지고 있되, 하위 능력에서 알아서 조합해서 쓰면 됨)
    /// (시스템 기획 느낌으로, 큰 틀만 제공할 뿐)
    /// (각 능력들의 전체 틀을 제공하고, 그 틀에 따라 생각하기 위함)
    /// </summary>
    public abstract class Ability : MonoBehaviour
    {
        protected Character ownerCharacter; //능력의 주인
            
        // 0. 초기화
        public virtual void Init()
        {
            this.SetOwner();
        }
        
        // 1. 외부의 입력값에 따라, 트리거 됨 (플레이어 입력, AI입력 모두 무관)
        public virtual void HandleInput() {}
        
        // 2. 매 프레임 실행할 것
        public virtual void Tick() {}
        //public virtual void EarlyTick() {}
        //public virtual void LateTick() {}
        
        // 3-1. 실행을 시도
        public virtual void TryExecute() {}
        
        // 3-2. 현재 프레임에 실행가능한지
        public virtual bool CanExecute() { return true; }
        
        // 3-3. 능력 "실제로 실행"
        // (실행 빈도/순서는 해당 능력 내부에서 결정)
        public abstract void Execute();
        
        // 이 Ability가 귀속된 캐릭터를 반환
        public void SetOwner()
        {
            this.ownerCharacter = GetComponent<Character>();
        }

        // 이 Ability가 귀속된 캐릭터를 반환
        public Character GetOwnerCharacter()
        {
            return this.ownerCharacter;
        }
    }
}