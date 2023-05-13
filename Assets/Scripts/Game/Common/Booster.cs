using UnityEngine;
using System.Collections.Generic;

public class Booster : TileEntity
{
    public BoosterType type;

    public virtual List<GameObject> Resolve(GameScene scene, int idx)
    {
        return new List<GameObject>();
    }

    public virtual void ShowFx(GamePools gamePools, GameScene scene, int idx)
    {
    }

    protected virtual void AddTile(List<GameObject> tiles, GameScene scene, int x, int y)
    {
        if (x < 0 || x >= scene.LevelData.width ||
            y < 0 || y >= scene.LevelData.height)
        {
            return;
        }

        var tileIndex = x + (y * scene.LevelData.width);
        var tile = scene.tileEntities[tileIndex];
        if (tile != null)
        {
            var block = tile.GetComponent<Block>();
            if (block != null && (block.type == BlockType.Empty || block.type == BlockType.Collectable))
            {
                return;
            }

            if (tiles.Contains(tile))
            {
                return;
            }

            tiles.Add(tile);
        }
    }

    protected bool IsValidTile(LevelData levelData, int x, int y)
    {
        return x >= 0 && x < levelData.width && y >= 0 && y < levelData.height;
    }
}