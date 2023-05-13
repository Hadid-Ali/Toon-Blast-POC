using UnityEngine;
using UnityEngine.UI;
using GameVanilla.Game.Common;

public class GoalUi : MonoBehaviour
{
    [SerializeField] public HorizontalLayoutGroup group;

    public void UpdateGoals(GameState state)
    {
        foreach (var element in group.GetComponentsInChildren<GoalUiElement>())
        {
            element.UpdateGoal(state);
        }
    }
}