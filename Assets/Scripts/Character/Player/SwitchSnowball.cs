using System;
using System.Collections.Generic;
using UnityEngine;
using SnowFight;

public class SwitchSnowball : Ability
{
    [Header("Snowball Options")]
    public List<Snowball> availableSnowballs = new List<Snowball>();
    [SerializeField] private int currentSnowballIndex = 0;

    public event Action<int, Snowball> OnSnowballChanged;

    private ThrowSnowball throwSnowball;

    public override void Init()
    {
        base.Init();
        throwSnowball = ownerCharacter.GetTargetAbility<ThrowSnowball>();
        UpdateEquippedSnowball();
        InvokeOnSnowballChanged();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if (ownerCharacter.inputState_SwitchLeft == InputState.Pressed)
        {
            SwitchLeft();
        }
        if (ownerCharacter.inputState_SwitchRight == InputState.Pressed)
        {
            SwitchRight();
        }
    }

    public override void Execute() { }

    private void SwitchLeft()
    {
        currentSnowballIndex--;
        if (currentSnowballIndex < 0)
        {
            currentSnowballIndex = availableSnowballs.Count - 1;
        }
        UpdateEquippedSnowball();
        InvokeOnSnowballChanged();
    }

    private void SwitchRight()
    {
        currentSnowballIndex++;
        if (currentSnowballIndex >= availableSnowballs.Count)
        {
            currentSnowballIndex = 0;
        }
        UpdateEquippedSnowball();
        InvokeOnSnowballChanged();
    }

    public int CurrentIndex
    {
        get { return currentSnowballIndex; }
    }

    private void UpdateEquippedSnowball()
    {
        Debug.Assert(availableSnowballs.Count != 0, "선택 가능한 눈덩이가 없습니다");
        throwSnowball.snowballPrefab = availableSnowballs[currentSnowballIndex];
        Debug.Log($"{gameObject.name} : {availableSnowballs[currentSnowballIndex].name}로 변경됨");
    }

    private void InvokeOnSnowballChanged()
    {
        if (availableSnowballs == null) return;
        if (availableSnowballs.Count == 0) return;

        Snowball current = availableSnowballs[currentSnowballIndex];
        if (OnSnowballChanged != null) OnSnowballChanged.Invoke(currentSnowballIndex, current);
    }
}
