
using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    void Awake() { LevelBalance.Apply(this); }
    [Header("Falling Objects")]
    public Hazard[] hazards;

    [Header("Spawn Settings")]
    public float initialDelay;
    public float spawnInterval;
    public float spawnHeight;
    public float minX;
    public float maxX;

    void Start()
    {
        InvokeRepeating(
            nameof(SpawnHazard),
            initialDelay,
            spawnInterval
        );
    }

    void SpawnHazard()
    {
        if (hazards == null || hazards.Length == 0)
            return;

        int randomIndex = Random.Range(0, hazards.Length);

        Hazard selectedHazard = hazards[randomIndex];

        if (selectedHazard == null)
            return;

        Vector3 spawnPosition = new Vector3(
            Random.Range(minX, maxX),
            spawnHeight,
            0f
        );

        Instantiate(
            selectedHazard,
            spawnPosition,
            Quaternion.identity
        );
    }
}
