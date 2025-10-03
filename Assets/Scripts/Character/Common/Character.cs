using System;
using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 플레이어/몬스터 공통 캐릭터 베이스
    /// 단, 몬스터한테만 적용될 수 있는 것들은 따로 캐릭터 베이스를 확장시켜서빼야할 듯
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Character : DamageableBase
    {
        [Header("Abilities")]
        public Ability[] ownedAbilities;
        public ThrowSnowball throwAbility;                 // 눈 던지기 능력

        // 이 변수들은 모두 ThrowSnowball로 옮기기
        [Header("Runtime")]
        public bool useCurveBall = false;                    // 커브 선택 여부 
        
        // InputState 저장 (Character에 놓여야 할 듯)
        // 1. 매 프레임 변하는 Input 값을 캐싱해서, PlayerInput과 Ability들을 디커플링 시키기 위함
        // 2. Player/Monster 모두 이 값을 사용하기 위함
        [Header("InputState - Move")]
        [SerializeField] public InputState inputState_MoveUp;
        [SerializeField] public InputState inputState_MoveDown;
        [SerializeField] public InputState inputState_MoveLeft;
        [SerializeField] public InputState inputState_MoveRight;
        
        [Header("InputState - Attack")]
        [SerializeField] public InputState inputState_Attack;
        
        [Header("InputState - Reload")]
        [SerializeField] public InputState inputState_Reload;

        public CharacterController characterController;

        // 컴포넌트 초기화
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            ownedAbilities = GetComponents<Ability>();

            this.InitAllAbilities();
        }

        private void Update()
        {
            this.HandleInputAllAbilities();
            this.TickAllAbilities();
        }


#region AbilitiesManagement
        /// <summary>
        /// 소유한 모든 능력들 초기화
        /// </summary>
        private void InitAllAbilities()
        {
            for (int i = 0; i < ownedAbilities.Length; i++)
            {
                if (ownedAbilities[i] != null)
                {
                    ownedAbilities[i].Init();
                }
            }
        }

        /// <summary>
        /// 매 프레임 모든 입력값 처리
        /// </summary>
        private void HandleInputAllAbilities()
        {
            for (int i = 0; i < ownedAbilities.Length; i++)
            {
                if (ownedAbilities[i] != null)
                {
                    ownedAbilities[i].HandleInput();
                }
            }
        }
        
        /// <summary>
        /// 매 프레임 모든 능력 실행
        /// </summary>
        private void TickAllAbilities()
        {
            for (int i = 0; i < ownedAbilities.Length; i++)
            {
                if (ownedAbilities[i] != null)
                {
                    ownedAbilities[i].Tick();
                }
            }
        }
        
        // 지워야 함 (Character클래스는 어떤 Ability가 실행될지 몰라야 한다)
        // 능력을 실행하도록 요청
        public void ExecuteAbility(Ability ability)
        {
            if (ability != null)
            {
                ability.Execute();
            }
        }
#endregion
        
        // 직구 선택
        public void SelectStraight()
        {
            useCurveBall = false;
        }

        // 커브 선택
        public void SelectCurve()
        {
            useCurveBall = true;
        }


        
    }
}