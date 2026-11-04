
using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    [Header("Falling Objects")]
    public Hazard[] hazards;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnHeight = 7f;
    public float minX = -7f;
    public float maxX = 7f;

    void Start()
    {
        InvokeRepeating(
            nameof(SpawnHazard),
            2f,
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
