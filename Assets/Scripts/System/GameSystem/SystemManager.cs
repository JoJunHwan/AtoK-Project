using UnityEngine;

public abstract class SystemManager : MonoBehaviour
{
    // Init By GameManager in Order
    public abstract void Init();

    //protected abstract void SetupSingleton();
}
