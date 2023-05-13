using System.Collections.Generic;
using UnityEngine;
public class Dynamite : Booster
{
    public override List<GameObject> Resolve(GameScene scene, int idx)
    {
        var tiles = new List<GameObject>();
        var x = idx % scene.LevelData.width;
        var y = idx / scene.LevelData.width;
        AddTile(tiles, scene, x - 1, y - 1);
        AddTile(tiles, scene, x, y - 1);
        AddTile(tiles, scene, x + 1, y - 1);
        AddTile(tiles, scene, x - 1, y);
        AddTile(tiles, scene, x, y);
        AddTile(tiles, scene, x + 1, y);
        AddTile(tiles, scene, x - 1, y + 1);
        AddTile(tiles, scene, x, y + 1);
        AddTile(tiles, scene, x + 1, y + 1);

        var combo = GetCombo(scene, x, y);
        if (combo)
        {
            AddTile(tiles, scene, x - 2, y - 2);
            AddTile(tiles, scene, x - 1, y - 2);
            AddTile(tiles, scene, x, y - 2);
            AddTile(tiles, scene, x + 1, y - 2);
            AddTile(tiles, scene, x + 2, y - 2);
            AddTile(tiles, scene, x - 2, y - 1);
            AddTile(tiles, scene, x + 2, y - 1);
            AddTile(tiles, scene, x - 2, y);
            AddTile(tiles, scene, x + 2, y);
            AddTile(tiles, scene, x - 2, y + 1);
            AddTile(tiles, scene, x + 2, y + 1);
            AddTile(tiles, scene, x - 2, y + 2);
            AddTile(tiles, scene, x - 1, y + 2);
            AddTile(tiles, scene, x, y + 2);
            AddTile(tiles, scene, x + 1, y + 2);
            AddTile(tiles, scene, x + 2, y + 2);
        }

        return tiles;
    }

    /// <summary>
    /// Shows the visual effects when the booster effect is resolved.
    /// </summary>
    /// <param name="gamePools">The game pools containing the visual effects.</param>
    /// <param name="scene">The scene in which to apply the booster.</param>
    /// <param name="idx">The tile index in which the booster is located.</param>
    public override void ShowFx(GamePools gamePools, GameScene scene, int idx)
    {
        var x = idx % scene.LevelData.width;
        var y = idx / scene.LevelData.width;
        var particles = gamePools.dynamiteParticlesPool.GetObject();
        particles.AddComponent<AutoKillPooled>();
        var tileIndex = x + (y * scene.LevelData.width);
        var hitPos = scene.tilePositions[tileIndex];
        particles.transform.position = hitPos;

        foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }

        SoundManager.Instance.PlaySound("Dynamite");
    }

    /// <summary>
    /// Returns the combo type at the specified coordinates.
    /// </summary>
    /// <param name="scene">The scene.</param>
    /// <param name="x">The x coordinate of the booster.</param>
    /// <param name="y">The y coordinate of the booster.</param>
    /// <returns>The combo type at the specified coordinates.</returns>
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

    /// <summary>
    /// Returns true if there is a combo at the specified coordinates and false otherwise.
    /// </summary>
    /// <param name="scene">The scene.</param>
    /// <param name="x">The x coordinate of the booster.</param>
    /// <param name="y">The y coordinate of the booster.</param>
    /// <returns>True if there is a combo at the specified coordinates; false otherwise.</returns>
    protected bool IsCombo(GameScene scene, int x, int y)
    {
        var idx = x + (y * scene.LevelData.width);
        if (IsValidTile(scene.LevelData, x, y) &&
            scene.tileEntities[idx] != null &&
            scene.tileEntities[idx].GetComponent<Dynamite>() != null)
        {
            return true;
        }

        return false;
    }
}