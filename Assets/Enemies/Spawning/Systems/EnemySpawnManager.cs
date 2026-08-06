using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EncounterData currentEncounter;
    [SerializeField] private float minSpawnRange = 15f;
    [SerializeField] private float maxSpawnRange = 100f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private bool autoStartEncounter = false;
    [SerializeField] private bool infiniteSpawning = false;
    [SerializeField] private float spawnPointRadius = 3f;

    [Header("Infinite Spawning Config")]
    //[SerializeField] private float spawnInterval = 5f;       
    [SerializeField] private int maxEnemiesAlive = 50;    
    [SerializeField] private float initialSpawnBudget = 50f;
    [SerializeField] private float budgetIncreaseRate = 2f;
    [SerializeField] private float minSpawnInterval = 8f; // Min time between bursts
    [SerializeField] private float maxSpawnInterval = 15f; // Max time between bursts
    [SerializeField][Range(0, 1)] private float budgetToSpendPercent = 0.75f; // Spend 75% of budget per burst

    private float currentSpawnBudget;
    private List<EnemyData> infiniteSpawnPool = new List<EnemyData>();

    [Header("Elite Modifiers")]
    [SerializeField] private EnemyModifierDatabase modifierDatabase;
    [Range(0, 1)]
    [SerializeField] private float eliteChance = 0.05f; // 5% chance to be elite

    [Header("Difficulty Tweak")]
    [SerializeField] private float difficultyIncreaseInterval = 180f; // 3 minutes
    [SerializeField][Range(1.0f, 2.0f)] private float difficultyMultiplier = 1.15f; // 15% increase
    private float difficultyLevel = 1f;
    private float difficultyTimer;

    [Header("Runtime State")]
    [SerializeField] private List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();
    private int currentWaveIndex = -1;
    private bool isSpawning = false;
    private int enemiesAlive = 0;

    // Event for UI or game logic
    public static event System.Action<int> OnWaveStart;
    public static event System.Action OnEncounterComplete;

    public bool EncounterInProgress = false;

    private Coroutine currentWaveCoroutine;
    private Transform playerTransform;
    private Camera playerCamera;

    void Start()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
            playerCamera = Camera.main ;
            
        }
        else
        {
            Debug.LogError("Spawn Manager could not find Player!", this);
        }

        // Automatically find all spawn points in the scene
        availableSpawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include).ToList();

        if(autoStartEncounter)
            StartEncounter();
    }

    void FixedUpdate()
    {
        if (infiniteSpawning && EncounterInProgress)
        {
            // --- New Difficulty Scaling Logic ---
            difficultyTimer += Time.deltaTime;
            if (difficultyTimer >= difficultyIncreaseInterval)
            {
                difficultyLevel *= difficultyMultiplier; // Exponential increase
                difficultyTimer = 0f;
                SystemLog.Instance.PostMessage($"Threat level increasing... (x{difficultyLevel:F2})");
            }

            // Budget now increases based on the current difficulty level
            currentSpawnBudget += (budgetIncreaseRate * difficultyLevel) * Time.deltaTime;
        }
    }

    public void StopEncounter(bool interrupted = true)
    {
        if(interrupted) SystemLog.Instance.PostMessage("Stopping current encounter");
        else SystemLog.Instance.PostMessage("Encounter Complete!");

        // Stop any existing wave spawning
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }
        isSpawning = false;
        EncounterInProgress = false;

        OnEncounterComplete?.Invoke();

        // Destroy all currently active enemies spawned by this manager
        GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy.GetComponent<EnemyBrain>().source == this)
                Destroy(enemy);
        }
        enemiesAlive = 0;
    }

    public void StartEncounter()
    {
        if (currentEncounter == null) // Simplified check
        {
            Debug.LogError("Encounter data is invalid.");
            return;
        }

        EncounterInProgress = true;
        if (infiniteSpawning)
        {
            SystemLog.Instance.PostMessage("Starting infinite encounter!");
            currentSpawnBudget = initialSpawnBudget;
            BuildInfiniteSpawnPool(); 

            currentWaveCoroutine = StartCoroutine(InfiniteSpawnLoop());
        }
        else
        {
            // This is your original, wave-based logic
            if (currentEncounter.waves.Count == 0)
            {
                Debug.LogError("Encounter data has no waves.");
                return;
            }

            SystemLog.Instance.PostMessage("Starting wave encounter!");
            currentWaveIndex = -1;
            StartNextWave();
        }
    }

    void StartNextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= currentEncounter.waves.Count)
        {
            // Encounter complete!
            StopEncounter(false);
            return;
        }
        SystemLog.Instance.PostMessage($"Starting wave {currentWaveIndex + 1} of {currentEncounter.waves.Count}!");

        WaveData wave = currentEncounter.waves[currentWaveIndex];
        OnWaveStart?.Invoke(currentWaveIndex + 1);
        StartCoroutine(SpawnWave(wave));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        isSpawning = true;
        yield return new WaitForSeconds(wave.initialDelay);

        foreach (SpawnGroupData group in wave.spawnGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyToSpawn);
            }
            yield return new WaitForSeconds(group.delayAfter);
        }

        isSpawning = false;
        currentWaveCoroutine = null;
    }

    IEnumerator InfiniteSpawnLoop()
    {
        while (EncounterInProgress)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Check if we *can* spawn
            if (enemiesAlive < maxEnemiesAlive)
            {
                // Decide how much budget to spend in this burst
                float budgetForThisBurst = currentSpawnBudget * budgetToSpendPercent;
                SpendBudget(budgetForThisBurst);
            }
        }
    }

    // Change the signature to accept the burst budget
    void SpendBudget(float budgetToSpend)
    {
        float budgetRemaining = budgetToSpend;
        int enemiesInThisBurst = 0;
        int maxEnemiesForBurst = maxEnemiesAlive - enemiesAlive; // How many can we spawn?

        // --- 1. Build Spawn Pool ---
        var affordableEnemies = infiniteSpawnPool
            .Where(e => e.spawnCost > 0 && e.spawnCost <= budgetRemaining)
            .OrderByDescending(e => e.spawnCost) // Most expensive first
            .ToList();

        if (affordableEnemies.Count == 0)
        {
            // Can't afford anything, save the budget
            return;
        }

        // --- 2. Spawn Loop (Builds a squad) ---
        while (budgetRemaining > 0 && enemiesInThisBurst < maxEnemiesForBurst)
        {
            // Re-check what we can afford with the remaining budget
            var currentlyAffordable = affordableEnemies.Where(e => e.spawnCost <= budgetRemaining).ToList();
            if (currentlyAffordable.Count == 0)
                break; // Can't afford anything else

            EnemyData enemyToSpawn = null;

            // 30% chance to pick the most expensive "leader" we can afford
            if (Random.value < 0.30f)
            {
                enemyToSpawn = currentlyAffordable[0]; // [0] is most expensive
            }
            // 70% chance to pick any random "grunt" we can afford
            else
            {
                enemyToSpawn = currentlyAffordable[Random.Range(0, currentlyAffordable.Count)];
            }

            SpawnEnemy(enemyToSpawn);
            budgetRemaining -= enemyToSpawn.spawnCost;
            enemiesInThisBurst++;
        }

        // We spent the budget, so remove it from the total
        currentSpawnBudget -= (budgetToSpend - budgetRemaining);
    }


    void BuildInfiniteSpawnPool()
    {
        infiniteSpawnPool.Clear();
        if (currentEncounter == null) return;

        // Create a unique list of all enemies available in this encounter
        foreach (var wave in currentEncounter.waves)
        {
            foreach (var group in wave.spawnGroups)
            {
                if (!infiniteSpawnPool.Contains(group.enemyToSpawn))
                {
                    infiniteSpawnPool.Add(group.enemyToSpawn);
                }
            }
        }
    }

    void SpawnEnemy(EnemyData enemyData)
    {
        SpawnPoint spawnPoint = GetValidSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("No valid spawn point found for enemy!");
            return;
        }

        // 1. Get the base spawn position from the chosen point
        Vector3 basePosition = spawnPoint.transform.position;

        // 2. Calculate a random offset
        Vector2 randomOffset = Random.insideUnitCircle * spawnPointRadius;
        Vector3 finalSpawnPosition = basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);

        if (UnityEngine.AI.NavMesh.SamplePosition(finalSpawnPosition, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            finalSpawnPosition = hit.position;
        }
        else
        {
            finalSpawnPosition = basePosition; // Fallback to the exact point
        }

        GameObject enemyObj = Instantiate(enemyData.modelPrefab, finalSpawnPosition, spawnPoint.transform.rotation);

        // --- IMPORTANT: Initialize the EnemyBrain ---
        EnemyBrain brain = enemyObj.GetComponent<EnemyBrain>();
        if (brain != null)
        { 
            brain.source = this;
            enemiesAlive++;
            brain.OnDeath += HandleEnemyDeath;
        }
        else
        {
            Debug.LogError($"Enemy prefab {enemyData.modelPrefab.name} is missing EnemyBrain!", enemyObj);
        }

        // Add Modifiers
        if (modifierDatabase != null && Random.value < eliteChance)
        {
            EnemyModifierManager modifierManager = enemyObj.GetComponent<EnemyModifierManager>();
            if (modifierManager != null)
            {
                // Pick a random modifier from the database
                EnemyModifierData modifierToApply = modifierDatabase.allModifiers[Random.Range(0, modifierDatabase.allModifiers.Count)];

                // Apply it!
                modifierManager.ApplyModifier(modifierToApply);
                Debug.Log($"A {modifierToApply.Name} is approaching!");
            }
        }
    }

    SpawnPoint GetValidSpawnPoint()
    {
        List<SpawnPoint> priorityPoints = new List<SpawnPoint>();
        List<SpawnPoint> fallbackPoints = new List<SpawnPoint>(); // For points > maxSpawnRange

        if (playerTransform == null)
        {
            if (availableSpawnPoints.Count > 0)
                return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

            Debug.LogError("No spawn points found in scene!");
            return null;
        }

        foreach (SpawnPoint point in availableSpawnPoints)
        {
            float distanceToPlayer = Vector3.Distance(point.transform.position, playerTransform.position);

            // 1. Check if too close
            if (distanceToPlayer < minSpawnRange)
            {
                continue;
            }

            // 2. Line of Sight check (Do not spawn if player is looking straight at point
            

            // 3. Sort into lists based on the "sweet spot"
            if (distanceToPlayer <= maxSpawnRange)
            {
                // This is a "sweet spot" spawn
                priorityPoints.Add(point);
            }
            else
            {
                // This is a valid, but very far, spawn
                fallbackPoints.Add(point);
            }
        }

        // --- 4. Pick a point ---
        if (priorityPoints.Count > 0)
        {
            return priorityPoints[Random.Range(0, priorityPoints.Count)];
        }
        else if (fallbackPoints.Count > 0)
        {
            Debug.LogWarning("No priorty points detected");
            // This ensures enemies can still spawn if the player is in a corner
            return fallbackPoints[Random.Range(0, fallbackPoints.Count)];
        }

        // No valid points at all
        Debug.LogWarning("Could not find any valid spawn points! Check min/max ranges and spawn point locations.");
        return null;
    }

    // --- Enemy Death Tracking ---
    void HandleEnemyDeath(EnemyBrain deadEnemy)
    {
        enemiesAlive--;
        deadEnemy.OnDeath -= HandleEnemyDeath;

        if (!infiniteSpawning)
        {
            CheckWaveCompletion();
        }
    }

    void CheckWaveCompletion()
    {
        if (infiniteSpawning) return;

        if (!isSpawning && enemiesAlive <= 0)
        {
            SystemLog.Instance.PostMessage("Wave complete!");

            // Cooldown, then start the next wave.
            WaveData currentWave = currentEncounter.waves[currentWaveIndex];
            Invoke(nameof(StartNextWave), currentWave.timeBetweenWaves);
        }
    }
}