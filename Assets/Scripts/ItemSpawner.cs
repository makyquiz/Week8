using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemSpawner : MonoBehaviour
{
    public Obstacle obstaclePrefab;
    public float speed = 15f;
    public List<Obstacle> obstacles = new List<Obstacle>();
    public Transform parent;

    private readonly float[] lanes = { -2f, 0f, 2f };

    // where enemies start and end
    public float startY = 3f;    // start high up visually
    public float endY = -2f;     // same level as player
    public float spawnZ = 300f;  // far back for perspective
    public float destroyZ = -20f;

    public float minSpawnDelay = 1.2f;
    public float maxSpawnDelay = 2.2f;
    private float spawnTimer;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void ScheduleNextSpawn()
    {
        spawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            BeginSpawn();
            ScheduleNextSpawn();
        }

        foreach (var obs in obstacles.ToList())
        {
            obs.itemPosition.z -= speed * Time.deltaTime;

            // interpolate Y downward as they approach
            float t = Mathf.InverseLerp(spawnZ, destroyZ, obs.itemPosition.z);
            obs.itemPosition.y = Mathf.Lerp(startY, endY, t);

            if (obs.itemPosition.z < destroyZ)
            {
                Destroy(obs.gameObject);
                obstacles.Remove(obs);
            }
        }

        // z-sorting
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
            children.Add(child);
        children = children.OrderBy(x => x.position.z).ToList();
        for (int i = 0; i < children.Count; i++)
            children[i].SetSiblingIndex(i);
    }

    void BeginSpawn()
    {
        float laneX = lanes[Random.Range(0, lanes.Length)];
        var spawned = Instantiate(obstaclePrefab, parent);
        spawned.itemPosition = new Vector3(laneX, startY, spawnZ);
        obstacles.Add(spawned);
    }
}
