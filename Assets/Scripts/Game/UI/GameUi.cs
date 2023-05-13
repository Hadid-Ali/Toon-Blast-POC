using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using GameVanilla.Game.Common;

public class GameUi : MonoBehaviour
{
    public Text limitTitleText;
    public Text limitText;

    public Text scoreText;

    public UnityEngine.UIElements.ProgressBar progressBar;

    public GoalUi goalUi;

    [SerializeField] private GameObject goalHeadline;

    [SerializeField] private GameObject scoreGoalHeadline;

    [SerializeField] private Text scoreGoalAmountText;

    private void Start()
    {
        goalHeadline.SetActive(false);
        scoreGoalHeadline.SetActive(false);
    }

    public void SetGoals(Goal goal, GameObject itemGoalPrefab)
    {
        var childrenToRemove = goalUi.group.GetComponentsInChildren<GoalUiElement>().ToList();
        foreach (var child in childrenToRemove)
        {
            Destroy(child.gameObject);
        }

        goalHeadline.SetActive(false);
        scoreGoalHeadline.SetActive(true);
        scoreGoalAmountText.text = goal.score.ToString();
    }
}