using UnityEngine;

public enum InputState
{
    None,       // 눌러지지 않은 상태
    Pressed,    // GetKeyDown 상태   
    Held,       // GetKey 상태   
    Released    // GetKeyUp 상태
}
