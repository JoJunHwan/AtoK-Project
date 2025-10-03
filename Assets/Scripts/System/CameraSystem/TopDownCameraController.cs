using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public class TopDownCameraController : MonoBehaviour
{
    [Header("Cinemachine 3")]
    [SerializeField] private CinemachineCamera vcam;

    [Header("Follow Target")]
    [SerializeField] private Transform followTarget;                 // 따라갈 타깃
    [SerializeField] private bool followTargetEnabled = true;        // 타깃 추적 여부
    [SerializeField, Min(0f)] private float followLerp = 10f;        // 부드럽게 따라오는 속도

    // 내부 상태
    private Transform followPivot;           // vcam.Follow가 가리킬 피벗
    private Quaternion lockedRotation;       // 고정할 카메라 회전
    private float fixedHeight = 0f;          // 피벗 Y 고정값

    private void Awake()
    {
        if (vcam == null)
        {
            vcam = GetComponentInChildren<CinemachineCamera>();
        }

        EnsureFollowPivot();
        LockCameraRotationNow();
        DisableRotationControls_CM3();
    }

    private void Start()
    {
        Vector3 p;
        if (followTarget != null)
        {
            p = followTarget.position;
        }
        else
        {
            p = followPivot.position;
        }
        p.y = fixedHeight;
        followPivot.position = p;
    }

    private void LateUpdate()
    {
        // 1) 회전 고정
        ApplyLockedRotation();

        if (followTargetEnabled && followTarget != null)
        {
            Vector3 want = followTarget.position;
            want.y = fixedHeight;

            Vector3 targetPos;
            if (followLerp <= 0f)
            {
                targetPos = want;
            }
            else
            {
                float t = followLerp * Time.deltaTime;
                targetPos = Vector3.Lerp(followPivot.position, want, t);
            }

            targetPos.y = fixedHeight;
            followPivot.position = targetPos;
        }
    }

    private void EnsureFollowPivot()
    {
        if (vcam == null)
        {
            Debug.LogError("[TopDownCameraController] CinemachineCamera가 필요합니다.");
            return;
        }

        // 기존 Follow 있으면 사용, 없으면 생성
        if (vcam.Follow != null)
        {
            followPivot = vcam.Follow;
        }
        else
        {
            GameObject pivot = new GameObject("CameraFollowPivot");
            followPivot = pivot.transform;
            followPivot.position = Vector3.zero;
            vcam.Follow = followPivot;
        }

        fixedHeight = followPivot.position.y;
    }

    private void LockCameraRotationNow()
    {
        if (vcam == null)
        {
            return;
        }
        lockedRotation = vcam.transform.rotation;
    }

    private void ApplyLockedRotation()
    {
        if (vcam == null)
        {
            return;
        }
        vcam.transform.rotation = lockedRotation;

        if (vcam.Lens.Dutch != 0f)
        {
            vcam.Lens.Dutch = 0f;
        }
    }

    private void DisableRotationControls_CM3()
    {
        if (vcam == null)
        {
            return;
        }

        var pov = vcam.GetComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.enabled = false;
        }

        var rotComposer = vcam.GetComponent<CinemachineRotationComposer>();
        if (rotComposer != null)
        {
            rotComposer.enabled = false;
        }

        var groupComposer = vcam.GetComponent<CinemachineGroupComposer>();
        if (groupComposer != null)
        {
            groupComposer.enabled = false;
        }
    }

    // 외부에서 런타임 중 회전 잠금 기준을 다시 잡고 싶을 때
    public void ReLockRotationToCurrent()
    {
        LockCameraRotationNow();
        ApplyLockedRotation();
    }

    // 외부에서 피벗 높이를 바꾸고 싶을 때
    public void SetFixedHeight(float height)
    {
        fixedHeight = height;
        if (followPivot != null)
        {
            Vector3 p = followPivot.position;
            p.y = fixedHeight;
            followPivot.position = p;
        }
    }
}
