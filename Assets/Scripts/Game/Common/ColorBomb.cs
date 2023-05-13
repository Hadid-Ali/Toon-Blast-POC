using System.Collections.Generic;
using UnityEngine;

public class ColorBomb : Booster
{
    public override List<GameObject> Resolve(GameScene scene, int idx)
    {
        var tiles = new List<GameObject>();

        var x = idx % scene.LevelData.width;
        var y = idx / scene.LevelData.width;
        var combo = GetCombo(scene, x, y);
        if (combo)
        {
            for (var j = 0; j < scene.LevelData.height; j++)
            {
                for (var i = 0; i < scene.LevelData.width; i++)
                {
                    var tileIndex = i + (j * scene.LevelData.width);
                    var tile = scene.tileEntities[tileIndex];
                    if (tile != null)
                    {
                        var block = tile.GetComponent<Block>();
                        if (block != null &&
                            (block.type == BlockType.Block1 ||
                             block.type == BlockType.Block2 ||
                             block.type == BlockType.Block3 ||
                             block.type == BlockType.Block4 ||
                             block.type == BlockType.Block5 ||
                             block.type == BlockType.Block6))
                        {
                            AddTile(tiles, scene, i, j);
                        }
                    }
                }
            }

            AddTile(tiles, scene, x, y);

            var up = new TileDef(x, y - 1);
            var down = new TileDef(x, y + 1);
            var left = new TileDef(x - 1, y);
            var right = new TileDef(x + 1, y);

            if (IsCombo(scene, up.x, up.y))
            {
                AddTile(tiles, scene, x, y - 1);
            }

            if (IsCombo(scene, down.x, down.y))
            {
                AddTile(tiles, scene, x, y + 1);
            }

            if (IsCombo(scene, left.x, left.y))
            {
                AddTile(tiles, scene, x - 1, y);
            }

            if (IsCombo(scene, right.x, right.y))
            {
                AddTile(tiles, scene, x + 1, y);
            }
        }
        else
        {
            var randomIdx = Random.Range(0, scene.LevelData.availableColors.Count);
            var randomBlock = scene.LevelData.availableColors[randomIdx];
            var randomType = BlockType.Block1;
            switch (randomBlock)
            {
                case ColorBlockType.ColorBlock1:
                    randomType = BlockType.Block1;
                    break;
                case ColorBlockType.ColorBlock2:
                    randomType = BlockType.Block2;
                    break;
                case ColorBlockType.ColorBlock3:
                    randomType = BlockType.Block3;
                    break;
                case ColorBlockType.ColorBlock4:
                    randomType = BlockType.Block4;
                    break;
                case ColorBlockType.ColorBlock5:
                    randomType = BlockType.Block5;
                    break;
                case ColorBlockType.ColorBlock6:
                    randomType = BlockType.Block6;
                    break;
            }

            for (var j = 0; j < scene.LevelData.height; j++)
            {
                for (var i = 0; i < scene.LevelData.width; i++)
                {
                    var tileIndex = i + (j * scene.LevelData.width);
                    var tile = scene.tileEntities[tileIndex];
                    if (tile != null)
                    {
                        var block = tile.GetComponent<Block>();
                        if (block != null && block.type == randomType)
                        {
                            AddTile(tiles, scene, i, j);
                        }
                    }
                }
            }

            AddTile(tiles, scene, x, y);
        }

        return tiles;
    }

    public override void ShowFx(GamePools gamePools, GameScene scene, int idx)
    {
        var x = idx % scene.LevelData.width;
        var y = idx / scene.LevelData.width;
        var particles = gamePools.colorBombParticlesPool.GetObject();
        particles.AddComponent<AutoKillPooled>();
        var tileIndex = x + (y * scene.LevelData.width);
        var hitPos = scene.tilePositions[tileIndex];
        particles.transform.position = hitPos;

        foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }

        SoundManager.Instance.PlaySound("ColorBomb");
    }

    protected bool GetCombo(GameScene scene, int x, int y)
    {
        var up = new TileDef(x, y - 1);
        var down = new TileDef(x, y + 1);
        var left = new TileDef(x - 1, y);
        var right = new TileDef(x + 1, y);

        if (IsCombo(scene, up.x, up.y) ||
            IsCombo(scene, down.x, down.y) ||
            IsCombo(scene, left.x, left.y) ||
            IsCombo(scene, right.x, right.y))
        {
            return true;
        }

        return false;
    }

    protected bool IsCombo(GameScene scene, int x, int y)
    {
        var idx = x + (y * scene.LevelData.width);
        if (IsValidTile(scene.LevelData, x, y) &&
            scene.tileEntities[idx] != null &&
            scene.tileEntities[idx].GetComponent<ColorBomb>() != null)
        {
            return true;
        }

        return false;
    }
}