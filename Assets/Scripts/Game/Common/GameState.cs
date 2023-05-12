using System;
using System.Collections.Generic;

public class GameState
{
    public int score;
    public Dictionary<BlockType, int> collectedBlocks = new Dictionary<BlockType, int>();
    public Dictionary<BlockerType, int> collectedBlockers = new Dictionary<BlockerType, int>();

    /// <summary>
    /// Resets the game state to its initial state.
    /// </summary>
    public void Reset()
    {
        score = 0;
        collectedBlocks.Clear();
        collectedBlockers.Clear();
        foreach (var value in Enum.GetValues(typeof(BlockType)))
        {
            collectedBlocks.Add((BlockType)value, 0);
        }

        foreach (var value in Enum.GetValues(typeof(BlockerType)))
        {
            collectedBlockers.Add((BlockerType)value, 0);
        }
    }
}