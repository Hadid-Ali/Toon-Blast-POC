// Copyright (C) 2017-2020 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Scenes;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup that is shown when a game ends.
    /// </summary>
    public class EndGamePopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Text scoreText;

        [SerializeField]
        private GameObject goalGroup;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(levelText);
            Assert.IsNotNull(scoreText);
            Assert.IsNotNull(goalGroup);
        }

        /// <summary>
        /// Called when the replay button is pressed.
        /// </summary>
        public void OnReplayButtonPressed()
        {
            var gameScene = parentScene as GameScene;
            if (gameScene != null)
            {
                var numLives = PlayerPrefs.GetInt("num_lives");
                if (numLives > 0)
                {
                    gameScene.RestartGame();
                    Close();
                }
            }
        }

        /// <summary>
        /// Sets the LevelData text.
        /// </summary>
        /// <param name="level">The LevelData text.</param>
        public void SetLevel(int level)
        {
            levelText.text = "LevelData " + level;
        }

        /// <summary>
        /// Sets the score text.
        /// </summary>
        /// <param name="score">The score text.</param>
        public void SetScore(int score)
        {
            scoreText.text = score.ToString();
        }

        /// <summary>
        /// Sets the goal group.
        /// </summary>
        /// <param name="group">The goal group.</param>
        public void SetGoals(GameObject group)
        {
            foreach (var goal in group.GetComponentsInChildren<GoalUiElement>())
            {
                var goalObject = Instantiate(goal);
                goalObject.transform.SetParent(goalGroup.transform, false);
                goalObject.GetComponent<GoalUiElement>().SetCompletedTick(goal.isCompleted);
            }
        }
    }
}
