using UnityEngine;
using System.Collections;

public class ThrowArm : MonoBehaviour
{
    private bool isPressed = false;

    [Header("회전 설정")]
    public float returnDelay = 0.5f;
    public Vector3 pressedRotation = new Vector3(90, 0, 0); // 눕힐 각도 (로컬 기준)

    private Quaternion verticalRot;   // 기본 (ㅣ 상태)
    private Quaternion horizontalRot; // 눕힘 (ㅡ 상태)

    void Start()
    {
        // 로컬 회전값 저장
        verticalRot = transform.localRotation;
        horizontalRot = Quaternion.Euler(pressedRotation);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isPressed)
            {
                isPressed = true;
                transform.localRotation = horizontalRot;
            }
        }
        else
        {
            if (isPressed)
            {
                isPressed = false;
                StartCoroutine(ReturnAfterDelay());
            }
        }
    }

    IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        transform.localRotation = verticalRot;
    }
}