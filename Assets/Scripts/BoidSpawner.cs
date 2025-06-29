using UnityEngine;
using UnityEngine.Windows;

public class BoidSpawner : MonoBehaviour
{
    public Boid boid;
    public float boidQty = 50f;
    public float spawnInterval = 1f;
    private float nextSpawn = 0.0f;
    private bool spawnAllowed = true;
    public float boidSpawnSinceStart = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (spawnAllowed && boid != null && nextSpawn <= 0.0f && boidSpawnSinceStart < boidQty)
        {
            Instantiate(boid, transform.position, transform.rotation);
            nextSpawn = spawnInterval;
            boidSpawnSinceStart += 1f;
        }
        
        nextSpawn -= Time.deltaTime;

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && boid != null)
        {
            boidSpawnSinceStart = 0f;
        }
    }
}
