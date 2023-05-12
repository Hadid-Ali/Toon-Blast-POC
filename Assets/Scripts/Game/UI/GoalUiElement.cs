// Copyright (C) 2017-2020 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages a single goal element within the in-game user interface for goal.
    /// </summary>
    public class GoalUiElement : MonoBehaviour
    {
        public Image image;
        public Text amountText;
        public Image tickImage;
        public Image crossImage;
        public ParticleSystem shineParticles;
        public ParticleSystem starParticles;

        public bool isCompleted { get; private set; }

        private Goal currentGoal;
        private int targetAmount;
        private int currentAmount;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            tickImage.gameObject.SetActive(false);
            crossImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fills this element with the information of the specified goal.
        /// </summary>
        /// <param name="goal">The associated goal.</param>
        public virtual void Fill(Goal goal)
        {
            currentGoal = goal;
        }

        /// <summary>
        /// Updates this element based on the current state of the game.
        /// </summary>
        /// <param name="state">The current game state.</param>
        public virtual void UpdateGoal(GameState state)
        {
           
        }

        /// <summary>
        /// Sets the goal tick as completed/not completed.
        /// </summary>
        /// <param name="completed">True if the completion tick should be shown; false otherwise.</param>
        public void SetCompletedTick(bool completed)
        {
            isCompleted = completed;
            amountText.gameObject.SetActive(false);
            if (completed)
            {
                tickImage.gameObject.SetActive(true);
                image.GetComponent<Animator>().SetTrigger("GoalAchieved");
                tickImage.GetComponent<Animator>().SetTrigger("GoalAchieved");
                shineParticles.Play();
                starParticles.Play();
            }
            else
            {
                crossImage.gameObject.SetActive(true);
            }
        }
    }
}
