using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Level Configuration")]
    public List<WaveConfig> waves;
    public List<SpawnPoint> spawnPoints;

    [Header("State")]
    public int currentWaveIndex = 0;
    public int enemiesAlive = 0;
    public bool isLevelComplete = false;

    [Header("Events")]
    public UnityEvent<int, int> OnWaveChanged; // (Current Wave, Total Waves)
    public UnityEvent OnLevelComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Find all spawn points in the scene automatically
        spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None).ToList();

        if (waves.Count > 0)
        {
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            LevelComplete();
            yield break;
        }

        WaveConfig wave = waves[currentWaveIndex];

        // Wait for the buffer time
        Debug.Log($"Wave {currentWaveIndex + 1} starting in {wave.timeBeforeWave} seconds...");
        yield return new WaitForSeconds(wave.timeBeforeWave);

        // Notify UI
        OnWaveChanged?.Invoke(currentWaveIndex + 1, waves.Count);

        // Start all batches for this wave
        foreach (var batch in wave.batches)
        {
            StartCoroutine(ProcessBatch(batch));
        }
    }

    private IEnumerator ProcessBatch(WaveConfig.SpawnBatch batch)
    {
        // Wait for this batch's specific delay
        if (batch.initialDelay > 0)
            yield return new WaitForSeconds(batch.initialDelay);

        for (int i = 0; i < batch.count; i++)
        {
            SpawnEnemy(batch.enemyPrefab, batch.spawnPointID);

            // Wait for interval (trickle spawning)
            if (batch.spawnInterval > 0)
                yield return new WaitForSeconds(batch.spawnInterval);
        }
    }

    private void SpawnEnemy(GameObject prefab, string tagID)
    {
        // Filter spawn points based on ID
        List<SpawnPoint> validPoints = spawnPoints;

        if (!string.IsNullOrEmpty(tagID))
        {
            validPoints = spawnPoints.Where(p => p.spawnID == tagID).ToList();
            if (validPoints.Count == 0)
            {
                Debug.LogWarning($"No spawn points found with ID '{tagID}'. Defaulting to random.");
                validPoints = spawnPoints;
            }
        }

        // Pick random point
        SpawnPoint sp = validPoints[Random.Range(0, validPoints.Count)];

        GameObject newEnemy = Instantiate(prefab, sp.transform.position, sp.transform.rotation);

        // Ensure it has the notifier
        if (newEnemy.GetComponent<EnemyNotifier>() == null)
        {
            newEnemy.AddComponent<EnemyNotifier>();
        }

        enemiesAlive++;
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            // Wave Cleared!
            Debug.Log("Wave Cleared!");
            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }

    private void LevelComplete()
    {
        isLevelComplete = true;
        Debug.Log("LEVEL COMPLETE!");

        // Trigger UI and Game Logic
        OnLevelComplete?.Invoke();

        // Example: Call your GameManager to handle the menu
        GameManager.Instance.ShowLevelCompleteUI();
    }
}