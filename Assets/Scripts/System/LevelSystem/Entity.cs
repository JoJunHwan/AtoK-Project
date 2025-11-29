using UnityEngine;

public class Entity : MonoBehaviour
{
    private bool hasAwakeEntityRun;
    private bool hasStartEntityRun;

    public void AwakeByLevelController()
    {
        if (IsInactive()) return;
        RunAwakeOnce();
    }
    
    public void StartByLevelController()
    {
        if (IsInactive()) return;
        RunAwakeOnce();
        RunStartOnce();
    }

    public void UpdateByLevelController()
    {
        if (IsInactive()) return;
        RunAwakeOnce();
        RunStartOnce();
        UpdateEntity();
    }

    private bool IsInactive()
    {
        return gameObject.activeInHierarchy == false;
    }

    private void RunAwakeOnce()
    {
        if (hasAwakeEntityRun) return;
        hasAwakeEntityRun = true;
        AwakeEntity();
    }

    private void RunStartOnce()
    {
        if (hasStartEntityRun) return;
        hasStartEntityRun = true;
        StartEntity();
    }
    
    public virtual void AwakeEntity()
    {
    }
    
    public virtual void StartEntity()
    {
    }

    public virtual void UpdateEntity()
    {
    }
    
    public virtual void Register()
    {
    }

    public virtual void Unregister()
    {
    }
}