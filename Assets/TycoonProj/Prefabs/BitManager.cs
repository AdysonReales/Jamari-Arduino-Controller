using UnityEngine;
using System.Collections.Generic;

public class BitManager : MonoBehaviour
{
    public static BitManager Instance;

    [Header("Global Spawn Settings")]
    public float globalSpawnInterval = 1f; // Time between all bit drops
    private float spawnTimer = 0f;

    private List<Dropper> droppers = new List<Dropper>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        // Register all droppers in the scene
        Dropper[] allDroppers = FindObjectsOfType<Dropper>();
        droppers.AddRange(allDroppers);
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= globalSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnBits();
        }
    }

    private void SpawnBits()
    {
        foreach (var dropper in droppers)
        {
            dropper.TryDropBitGlobal();
        }
    }
}
