// Copyright (C) 2017-2020 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages the in-game user interface.
    /// </summary>
    public class GameUi : MonoBehaviour
    {
        public Text limitTitleText;
        public Text limitText;

        public Text scoreText;

        public ProgressBar progressBar;

        public GoalUi goalUi;

#pragma warning disable 649
        [SerializeField]
        private GameObject goalHeadline;

        [SerializeField]
        private GameObject scoreGoalHeadline;

        [SerializeField]
        private Text scoreGoalAmountText;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(goalHeadline);
            Assert.IsNotNull(scoreGoalHeadline);
            Assert.IsNotNull(scoreGoalAmountText);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            goalHeadline.SetActive(false);
            scoreGoalHeadline.SetActive(false);
        }

        /// <summary>
        /// Sets the goal in the goal UI.
        /// </summary>
        /// <param name="goals">The list of goal of the current LevelData.</param>
        /// <param name="itemGoalPrefab">The goal prefab.</param>
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
}
