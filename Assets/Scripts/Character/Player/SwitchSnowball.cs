using System.Collections.Generic;
using UnityEngine;
using SnowFight;

public class SwitchSnowball : Ability
{
    [Header("Snowball Options")]
    public List<Snowball> availableSnowballs = new List<Snowball>();
    [SerializeField] private int currentSnowballIndex = 0;

    private ThrowSnowball throwSnowball;

    public override void Init()
    {
        base.Init();
        
        throwSnowball = ownerCharacter.GetTargetAbility<ThrowSnowball>();
        UpdateEquippedSnowball();
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

    public override void Execute()
    {
        //throw new System.NotImplementedException();
    }

    private void SwitchLeft()
    {
        currentSnowballIndex--;
        if (currentSnowballIndex < 0)
        {
            currentSnowballIndex = availableSnowballs.Count - 1;
        }
        UpdateEquippedSnowball();
    }

    private void SwitchRight()
    {
        currentSnowballIndex++;
        if (currentSnowballIndex >= availableSnowballs.Count)
        {
            currentSnowballIndex = 0;
        }
        UpdateEquippedSnowball();
    }

    private void UpdateEquippedSnowball()
    {
        Debug.Assert(availableSnowballs.Count != 0, "선택 가능한 눈덩이가 없습니다");

        throwSnowball.snowballPrefab = availableSnowballs[currentSnowballIndex];
        Debug.Log($"{gameObject.name} : {availableSnowballs[currentSnowballIndex].name}로 변경됨");
    }
}