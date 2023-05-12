using System;
using GameVanilla.Game.Common;

[Serializable]
    public class Goal
    {
        public int score;

        public bool IsComplete(GameState state)
        {
            return state.score >= score;
        }

    }