using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using GameVanilla.Core;

public class GamePools : MonoBehaviour
{
    public ObjectPool block1Pool;
    public ObjectPool block2Pool;
    public ObjectPool block3Pool;
    public ObjectPool block4Pool;
    public ObjectPool block5Pool;
    public ObjectPool block6Pool;
    public ObjectPool emptyTilePool;
    public ObjectPool ballPool;
    public ObjectPool stonePool;
    public ObjectPool collectablePool;

    public ObjectPool horizontalBombPool;
    public ObjectPool verticalBombPool;
    public ObjectPool dynamitePool;
    public ObjectPool colorBombPool;

    public ObjectPool icePool;

    public ObjectPool block1ParticlesPool;
    public ObjectPool block2ParticlesPool;
    public ObjectPool block3ParticlesPool;
    public ObjectPool block4ParticlesPool;
    public ObjectPool block5ParticlesPool;
    public ObjectPool block6ParticlesPool;
    public ObjectPool ballParticlesPool;
    public ObjectPool stoneParticlesPool;
    public ObjectPool collectableParticlesPool;
    public ObjectPool boosterSpawnParticlesPool;
    public ObjectPool horizontalBombParticlesPool;
    public ObjectPool verticalBombParticlesPool;
    public ObjectPool dynamiteParticlesPool;
    public ObjectPool colorBombParticlesPool;
    public ObjectPool iceParticlesPool;

    private readonly List<ObjectPool> blockPools = new();
    private readonly List<ObjectPool> powerPools = new();

    private void Awake()
    {
        blockPools.Add(block1Pool);
        blockPools.Add(block2Pool);
        blockPools.Add(block3Pool);
        blockPools.Add(block4Pool);
        blockPools.Add(block5Pool);
        blockPools.Add(block6Pool);

        powerPools.Add(horizontalBombPool);
        powerPools.Add(verticalBombPool);
        powerPools.Add(dynamitePool);
        powerPools.Add(colorBombPool);
    }

    public TileEntity GetTileEntity(LevelData levelData, LevelTile tile)
    {
        switch (tile.TileType)
        {
            case TileType.Block:
                return blockPools[Random.Range(0, blockPools.Count)].GetObject().GetComponent<TileEntity>();

            case TileType.Booster:
                return powerPools[Random.Range(0, powerPools.Count)].GetObject().GetComponent<TileEntity>();
        }

        return null;
    }

    public TileEntity GetTileEntityLegacy(LevelData levelData, LevelTile tile)
    {
        if (tile is BlockTile)
        {
            var blockTile = (BlockTile)tile;
            switch (blockTile.type)
            {
                case BlockType.Block1:
                    return block1Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Block2:
                    return block2Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Block3:
                    return block3Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Block4:
                    return block4Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Block5:
                    return block5Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Block6:
                    return block6Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.RandomBlock:
                {
                    var randomIdx = Random.Range(0, levelData.availableColors.Count);
                    return blockPools[(int)levelData.availableColors[randomIdx]].GetObject().GetComponent<TileEntity>();
                }

                case BlockType.Empty:
                    return emptyTilePool.GetObject().GetComponent<TileEntity>();

                case BlockType.Ball:
                    return ballPool.GetObject().GetComponent<TileEntity>();

                case BlockType.Stone:
                    return stonePool.GetObject().GetComponent<TileEntity>();

                case BlockType.Collectable:
                    return collectablePool.GetObject().GetComponent<TileEntity>();
            }
        }
        else if (tile is BoosterTile)
        {
            var boosterTile = (BoosterTile)tile;
            switch (boosterTile.type)
            {
                case BoosterType.HorizontalBomb:
                    return horizontalBombPool.GetObject().GetComponent<TileEntity>();

                case BoosterType.VerticalBomb:
                    return verticalBombPool.GetObject().GetComponent<TileEntity>();

                case BoosterType.Dynamite:
                    return dynamitePool.GetObject().GetComponent<TileEntity>();

                case BoosterType.ColorBomb:
                    return colorBombPool.GetObject().GetComponent<TileEntity>();
            }
        }

        return null;
    }

    public GameObject GetParticles(TileEntity tileEntity)
    {
        GameObject particles = null;
        var block = tileEntity as Block;
        if (block != null)
        {
            switch (block.type)
            {
                case BlockType.Block1:
                    particles = block1ParticlesPool.GetObject();
                    break;

                case BlockType.Block2:
                    particles = block2ParticlesPool.GetObject();
                    break;

                case BlockType.Block3:
                    particles = block3ParticlesPool.GetObject();
                    break;

                case BlockType.Block4:
                    particles = block4ParticlesPool.GetObject();
                    break;

                case BlockType.Block5:
                    particles = block5ParticlesPool.GetObject();
                    break;

                case BlockType.Block6:
                    particles = block6ParticlesPool.GetObject();
                    break;

                case BlockType.Ball:
                    particles = ballParticlesPool.GetObject();
                    break;

                case BlockType.Stone:
                    particles = stoneParticlesPool.GetObject();
                    break;

                case BlockType.Collectable:
                    particles = collectableParticlesPool.GetObject();
                    break;

                default:
                    return null;
            }
        }

        return particles;
    }
}