using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "GameData/LevelData/Create")]
public class LevelData : ScriptableObject
{
    public int id;

    public int width;
    public int height;
    public List<LevelTile> tiles = new();

    public LimitType limitType;
    public int limit;
    public int penalty;

    public Goal goal;
    public List<ColorBlockType> availableColors = new List<ColorBlockType>();

    public int score1;
    public int score2;
    public int score3;

    public bool awardBoostersWithRemainingMoves;
    public BoosterType awardedBoosterType;

    public int collectableChance;

    public Dictionary<BoosterType, bool> availableBoosters = new Dictionary<BoosterType, bool>();
}
