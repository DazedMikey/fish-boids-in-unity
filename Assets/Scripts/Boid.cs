using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("[Boid Settings]")]
    public List<Boid> neighbors = new();
    public float speed = 10f;
    public float neighbourDistance = 10f;
    public float raycastLength = 20f;
    private Rigidbody rb;
    // public float turnSpeed = 10f;
    // public float avoidanceRadius = 5f;
    // public float avoidanceDistance = 5f;
    public float avoidanceStrength = 1.0f;
    public float cohesionStrength = 0.3f;
    public float alignmentStrength = 0.5f;
    public float separationStrength = 1.5f;
    
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    public float stuckRadius = 2f;
    public float stuckTimeLimit = 3f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        neighbors = GetNeighbors();
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 obstacleAvoidance = Vector3.zero;
        
        // Process neighbors
        if (neighbors.Count > 0)
        {
            Vector3 centerOfMass = Vector3.zero;
            
            foreach (Boid boid in neighbors)
            {
                Vector3 offset = transform.position - boid.transform.position;
                float distance = offset.magnitude;
                
                // Separation - only when very close
                if (distance < neighbourDistance * 0.5f && distance > 0.1f)
                    separation += offset.normalized / distance;
                    
                // Alignment
                alignment += boid.transform.forward;
                
                // Cohesion
                centerOfMass += boid.transform.position;
            }
            
            alignment = (alignment / neighbors.Count).normalized;
            cohesion = ((centerOfMass / neighbors.Count) - transform.position).normalized;
        }

        // Obstacle avoidance
        Vector3[] directions = {
            transform.forward,
            Quaternion.AngleAxis(-20, transform.up) * transform.forward,
            Quaternion.AngleAxis(20, transform.up) * transform.forward,
            Quaternion.AngleAxis(-20, transform.right) * transform.forward,
            Quaternion.AngleAxis(20, transform.right) * transform.forward
        };
        
        int hitCount = 0;
        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, raycastLength) && hit.collider.CompareTag("Obsticle"))
            {
                hitCount++;
                Vector3 avoidDir = hit.normal;
                Vector3 sideDir = Vector3.Cross(hit.normal, Vector3.up);
                
                if (sideDir.magnitude < 0.1f)
                    sideDir = Vector3.Cross(hit.normal, Vector3.forward);
                    
                obstacleAvoidance += avoidDir + sideDir.normalized * Random.Range(-1f, 1f);
            }
        }
        
        // Emergency corner escape
        if (hitCount >= 3)
        {
            obstacleAvoidance += Vector3.up * 2f + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        }

        // Combine behaviors
        Vector3 finalDirection = transform.forward + 
                               separation * separationStrength + 
                               alignment * alignmentStrength + 
                               cohesion * cohesionStrength + 
                               obstacleAvoidance * avoidanceStrength;
        
        // Check if stuck in same area
        if (Vector3.Distance(transform.position, lastPosition) < stuckRadius)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > stuckTimeLimit)
            {
                transform.rotation *= Quaternion.Euler(0, 180, 0);
                stuckTimer = 0f;
                lastPosition = transform.position;
            }
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
        
        if (finalDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(finalDirection), Time.fixedDeltaTime * 2f);
        }
        
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnDrawGizmos()
    {
        // Represent neighbor zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, neighbourDistance);

        // Draw all raycast directions
        Vector3[] directions = {
            transform.forward,
            Quaternion.AngleAxis(-20, transform.up) * transform.forward,
            Quaternion.AngleAxis(20, transform.up) * transform.forward,
            Quaternion.AngleAxis(-20, transform.right) * transform.forward,
            Quaternion.AngleAxis(20, transform.right) * transform.forward
        };
        
        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, raycastLength))
            {
                if (hit.collider.gameObject.CompareTag("Obsticle"))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(transform.position, dir * hit.distance);
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(transform.position, dir * raycastLength);
                }
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, dir * raycastLength);
            }
        }
    }

    private List<Boid> GetNeighbors()
    {
        List<Boid> neighbors = new();

        foreach (Collider obj in Physics.OverlapSphere(transform.position, neighbourDistance))
        {
            if (obj.CompareTag("Boid") && obj.gameObject != gameObject)
            {
                neighbors.Add(obj.GetComponent<Boid>());
            }
        }
        return neighbors;
    }
}
