using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private bool cursorVisible = true;
    [SerializeField] private bool isCameraOrthographic = false;
    [SerializeField] private bool isCustomCursorVisible = true;
    [SerializeField] private Vector2 cursorPos;
    
    
    
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    
    public static CursorController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        spriteRenderer =  GetComponentInChildren<SpriteRenderer>();
        SetCursorVisible(cursorVisible);
    }

    // Update is called once per frame
    void Update()
    {
        // 진짜 커서는 안보이도록
        SetCursorVisible(cursorVisible);
        /*
        // 커스텀 커서 보이게/안보이게
        SetCustomCursorVisible(isCustomCursorVisible);
        
        // 커스텀 커서 위치설정
        Vector3 beforeMousePosition = Input.mousePosition;
        Vector3 convertedMousePosition = SelectCursorByCamera(beforeMousePosition);
        transform.position = convertedMousePosition;
        */
        //transform.position = Input.mousePosition;
    }

    public Vector3 SelectCursorByCamera(Vector3 screenPosition)
    {
        if (isCameraOrthographic == true) return GetMousePosition_Orthograph(screenPosition);
        else return GetMousePosition_Perspective(screenPosition, 0);
    }

    
    public Vector3 GetMousePosition_Perspective(Vector3 screenPosition, float z)
    {
        return GetWorldPositionOnPlane(screenPosition, z);
    }
    
    public Vector3 GetMousePosition_Orthograph(Vector3 screenPosition)
    {
        return Camera.main.ScreenToWorldPoint(screenPosition);
    }
    
    
    public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    public void SetCustomCursorVisible (bool visible)
    {
        this.spriteRenderer.enabled = visible;
    }

    private void SetCursorVisible(bool _visible)
    {
        Cursor.visible = _visible;
    }
}
