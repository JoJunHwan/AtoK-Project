using UnityEngine;

namespace SnowFight
{
    /// <summary>
    /// 이동 능력
    /// </summary>
    public class Move : Ability
    {
        [Header("Current Input")]
        [SerializeField] private float curMoveX = 0f;
        [SerializeField] private float curMoveZ = 0f;
        
        [Header ("Physics")]
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Movement")]
        public float moveSpeed = 5f;                       // 이동 속도
        public Vector3 moveDirection;                      // 외부 입력 또는 AI가 채우는 이동 방향

        public override void HandleInput()
        {
            curMoveX = 0.0f;
            curMoveZ = 0.0f;
            
            // 플레이어 이동 처리 (모두 함수로 빼두기)
            if (base.ownerCharacter.inputState_MoveLeft == InputState.Held)
            {
                curMoveX -= 1f;
            }
            if (base.ownerCharacter.inputState_MoveRight == InputState.Held)
            {
                curMoveX += 1f;
            }
            if (base.ownerCharacter.inputState_MoveUp == InputState.Held)
            {
                curMoveZ += 1f;
            }
            if (base.ownerCharacter.inputState_MoveDown == InputState.Held)
            {
                curMoveZ -= 1f;
            }
        }

        public override void Tick()
        {
            UpdateMovement(curMoveX, curMoveZ);
        }

        // 해당프레임에 들어온 입력을 적용해서, 이동을 실행
        // 단, 중력도 같이 적용함
        public override void Execute()
        {
            if (ownerCharacter == null)
            {
                return;
            }

            // 수평 성분만 정규화해서 속도 방향으로 사용 (수직은 중력 전용으로 분리)
            Vector3 horizontal = new Vector3(this.moveDirection.x, 0f, this.moveDirection.z);
            if (horizontal.sqrMagnitude > 1f)
            {
                horizontal = horizontal.normalized;
            }

            this.MoveCharacter(horizontal);
        }
        
        /// <summary>
        /// Move클래스에서 이동을 실제로 수행
        /// </summary>
        public void MoveCharacter(Vector3 worldDir)
        {
            // 중력 먼저 갱신(수직 속도 축적)
            this.ApplyGravity();

            // 수평 속도 = 방향 * moveSpeed, 수직 속도 = moveDirection.y (중력 누적)
            Vector3 velocity = worldDir * moveSpeed;
            velocity.y = moveDirection.y;

            ownerCharacter.characterController.Move(velocity * Time.deltaTime);
        }
        
        // 움직임 갱신
        private void UpdateMovement(float x, float z)
        {
            Vector3 world = GetWorldMoveDirection(x, z);
            ApplyMoveAbility(world);
            RotateTowards(world);
        }

        // 카메라 기준으로 (x, z) 이동 값을 반환한다
        private Vector3 GetWorldMoveDirection(float x, float z)
        {
            Vector3 forward = GetCameraForward();
            Vector3 right = GetCameraRight();
            return forward * z + right * x;
        }
        
        private Vector3 GetCameraForward()
        {
            if (Camera.main == null) return Vector3.forward;
            
            Vector3 f = Camera.main.transform.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0f)
            {
                return f.normalized;
            }
            return Vector3.forward;
        }

        private Vector3 GetCameraRight()
        {
            if (Camera.main == null) return Vector3.right;
            Vector3 r = Camera.main.transform.right;
            r.y = 0f;
            if (r.sqrMagnitude > 0f)
            {
                return r.normalized;
            }
            return Vector3.right;
        }
        
        private void ApplyMoveAbility(Vector3 world)
        {
            // 입력으로 계산한 수평 이동을 moveDirection에 반영
            // (y값은 건드리지 않음. 경사 올라갈때 달라지기 때문)
            moveDirection.x = world.x;
            moveDirection.z = world.z;
            
            // 이 프레임에 즉시 실행(Ability 시스템 호출 타이밍과 무관하게 보장)
            this.Execute();
        }
        
        private void RotateTowards(Vector3 world)
        {
            if (world.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(world, Vector3.up),
                    20f * Time.deltaTime
                );
            }
        }
        
#region Physics
        private void ApplyGravity()
        {
            // 땅에 닿아있으면 수직 속도 리셋
            if (ownerCharacter.characterController.isGrounded)
            {
                if (moveDirection.y < 0f)
                {
                    moveDirection.y = 0f;
                }
            }
            else
            {
                // 공중일 때만 중력 누적
                moveDirection.y += gravity * Time.deltaTime;
            }
        }
#endregion
    }
}
