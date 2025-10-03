using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 플레이어 입력을 캐릭터에 전달
    /// </summary>
    [RequireComponent(typeof(Character))]
    public class PlayerInput : MonoBehaviour
    {
        [Header("Move Keys")]
        public KeyCode keyUp = KeyCode.W;                  // 위로 이동 키
        public KeyCode keyLeft = KeyCode.A;                // 왼쪽 이동 키
        public KeyCode keyDown = KeyCode.S;                // 아래 이동 키
        public KeyCode keyRight = KeyCode.D;               // 오른쪽 이동 키

        [Header("Actions")]
        public KeyCode keyThrow = KeyCode.Mouse0;          // 눈 던지기
        public KeyCode keyCharge = KeyCode.R;              // 눈 충전
        public KeyCode keyStraight = KeyCode.Q;            // 직구 선택
        public KeyCode keyCurve = KeyCode.E;               // 커브 선택
        public KeyCode keyInteract = KeyCode.Mouse1;       // 상호작용

        [Header("Charge Settings")]
        public int chargePerPress = 1;                     // R 한 번당 충전량

        private Character character;

        // 컴포넌트 초기화
        private void Awake()
        {
            character = GetComponent<Character>();
        }

        // 입력을 읽고 캐릭터에 반영
        private void Update()
        {
            ReadMove();
            ReadActions();
        }

        // 이동 입력을 읽음 (Move 클래스로 이동)
        private void ReadMove()
        {
            UpdateKeyState(ref character.inputState_MoveDown, keyDown);
            UpdateKeyState(ref character.inputState_MoveUp, keyUp);
            UpdateKeyState(ref character.inputState_MoveLeft, keyLeft);
            UpdateKeyState(ref character.inputState_MoveRight, keyRight);
        }
        
        /// <summary>
        /// 고쳐야 함. 지금은 Input에서 각 Ability의 함수를 실행하는데.
        /// 이게 아니라, 각 Abilty에서 Input의 캐싱된 값을 읽도록 해야함
        /// </summary>
        private void ReadActions()
        {
            UpdateKeyState(ref character.inputState_Attack, keyThrow); 
            UpdateKeyState(ref character.inputState_Reload, keyCharge);

            if (Input.GetKeyDown(keyStraight))
            {
                character.SelectStraight();
            }

            if (Input.GetKeyDown(keyCurve))
            {
                character.SelectCurve();
            }

            if (Input.GetKeyDown(keyInteract))
            {
                TryInteract();
            }
        }

        // 간단한 상호작용 레이캐스트
        private void TryInteract()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1.2f, transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 2.0f))
            {
                // 여기에 상호작용 가능한 오브젝트 처리 코드를 넣음
            }
        }
        
        private void UpdateKeyState(ref InputState state, KeyCode key)
        {
            if (Input.GetKeyDown(key))
            {
                state = InputState.Pressed;
                return;
            }

            if (Input.GetKey(key))
            {
                state = InputState.Held;
                return;
            }

            if (Input.GetKeyUp(key))
            {
                state = InputState.Released;
                return;
            }

            state = InputState.None;
        }
    }
}
