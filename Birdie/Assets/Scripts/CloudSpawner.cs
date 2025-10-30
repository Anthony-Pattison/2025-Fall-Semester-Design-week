using UnityEngine;
using System.Collections.Generic;

public class CloudSpawner : MonoBehaviour
{
    [Header("Cloud Prefab")]
    [SerializeField] private GameObject cloudPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int maxClouds = 15;
    [SerializeField] private float spawnHeight = 45f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnRangeX = 100f;
    [SerializeField] private float spawnRangeZ = 100f;

    [Header("Cloud Behavior")]
    [SerializeField] private float cloudLifetime = 60f;
    [SerializeField] private float moveSpeedMin = 1f;
    [SerializeField] private float moveSpeedMax = 3f;
    [SerializeField] private float cloudScaleMin = 15f;
    [SerializeField] private float cloudScaleMax = 30f;

    [Header("Follow Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceFromPlayer = 80f;

    private List<CloudData> activeClouds = new List<CloudData>();
    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;

        // Spawn initial clouds
        for (int i = 0; i < maxClouds / 2; i++)
        {
            SpawnCloud();
        }
    }

    void Update()
    {
        // Spawn new clouds over time
        if (Time.time >= nextSpawnTime && activeClouds.Count < maxClouds)
        {
            SpawnCloud();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // Update existing clouds
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            CloudData cloud = activeClouds[i];

            if (cloud.cloudObject == null)
            {
                activeClouds.RemoveAt(i);
                continue;
            }

            // Move cloud
            cloud.cloudObject.transform.position += cloud.moveDirection * cloud.moveSpeed * Time.deltaTime;

            // Check if cloud should despawn
            if (Time.time >= cloud.spawnTime + cloudLifetime)
            {
                Destroy(cloud.cloudObject);
                activeClouds.RemoveAt(i);
            }
        }
    }

    void SpawnCloud()
    {
        if (cloudPrefab == null)
        {
            Debug.LogWarning("Cloud prefab is not assigned!");
            return;
        }

        // Calculate spawn position
        Vector3 spawnPosition;

        if (player != null)
        {
            // Spawn around the player's position
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(spawnDistanceFromPlayer * 0.5f, spawnDistanceFromPlayer);

            float offsetX = Mathf.Cos(randomAngle) * distance;
            float offsetZ = Mathf.Sin(randomAngle) * distance;

            spawnPosition = new Vector3(
                player.position.x + offsetX,
                spawnHeight,
                player.position.z + offsetZ
            );
        }
        else
        {
            // Spawn in random area if no player reference
            spawnPosition = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                spawnHeight,
                Random.Range(-spawnRangeZ, spawnRangeZ)
            );
        }

        // Instantiate cloud
        GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);
        cloud.transform.parent = transform;

        // Random scale
        float scale = Random.Range(cloudScaleMin, cloudScaleMax);
        cloud.transform.localScale = new Vector3(scale, scale * 0.5f, scale);

        // Random movement direction (only horizontal)
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 moveDirection = new Vector3(randomDir.x, 0f, randomDir.y);
        float moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);

        // Add to active clouds list
        CloudData cloudData = new CloudData
        {
            cloudObject = cloud,
            moveDirection = moveDirection,
            moveSpeed = moveSpeed,
            spawnTime = Time.time
        };

        activeClouds.Add(cloudData);
    }

    // Helper class to store cloud data
    private class CloudData
    {
        public GameObject cloudObject;
        public Vector3 moveDirection;
        public float moveSpeed;
        public float spawnTime;
    }
}