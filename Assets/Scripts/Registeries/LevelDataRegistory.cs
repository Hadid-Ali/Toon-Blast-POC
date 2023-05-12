using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataRegistory : MonoBehaviourSingleton<LevelDataRegistory>
{
    // Registry should load data from json created using an editor or ingestion but we are keeping level data as scriptables for now
    [SerializeField] private List<LevelData> m_LevelsData = new();
    

    public LevelData GetLevelData(int number) => m_LevelsData[number - 1];
}
