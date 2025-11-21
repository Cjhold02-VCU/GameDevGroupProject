using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWave", menuName = "Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Wave Settings")]
    [Tooltip("Time in seconds to wait before this wave begins (gives player a breather).")]
    public float timeBeforeWave = 2f;

    public List<SpawnBatch> batches;

    [System.Serializable]
    public class SpawnBatch
    {
        public GameObject enemyPrefab;
        public int count = 1;

        [Tooltip("How many seconds after the Wave Starts should this batch spawn?")]
        public float initialDelay = 0f;

        [Tooltip("Time between individual spawns in this batch (0 = all at once).")]
        public float spawnInterval = 1f;

        [Tooltip("Leave empty to pick ANY random spawn point. Set to match a SpawnPoint ID to be specific.")]
        public string spawnPointID = "";
    }
}