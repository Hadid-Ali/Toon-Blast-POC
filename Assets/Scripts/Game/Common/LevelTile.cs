
    using System;
    using GameVanilla.Game.Common;

    public enum TileType
    {
        Block,
        Booster
    }
    
    [Serializable]
    public class LevelTile
    {
        public BlockerType blockerType;
        public TileType TileType;
    }

    [Serializable]
    public class BlockTile : LevelTile
    {
        public BlockType type;
    }

    [Serializable]
    public class BoosterTile : LevelTile
    {
        public BoosterType type;
    }
