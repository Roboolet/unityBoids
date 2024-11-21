using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NaiveBoids : MonoBehaviour
{
    [FormerlySerializedAs("spawnAmount")]
    [Header("Initial parameters")]
    [SerializeField] private int initialSpawnAmount;
    [SerializeField, Range(1, 10)] private int initialFlockAmount;
    
    [Header("Boid parameters")]
    [SerializeField, Range(0, 100)] private float maxSpeed;
    [SerializeField, Range(0, 1)] private float flockCohesion;
    [SerializeField, Range(0, 1)] private float nonFlockCohesion;
    [SerializeField, Range(0, 1)] private float flockAlignment;
    [SerializeField, Range(0, 1)] private float nonFlockAlignment;
    [SerializeField, Range(0, 20)] private float seperationDistance;
    
    [Header("Container parameters")]
    [SerializeField, Range(5, 50)] private float fishBowlSize;
    
    private Boid[] boids;
    
    void Start()
    {
        // populate
        boids = new Boid[initialSpawnAmount];
        for (int i = 0; i < initialSpawnAmount; i++)
        {
            int flockID = Mathf.FloorToInt((float)i / (float)initialSpawnAmount * (initialFlockAmount));
            Debug.Log(flockID);
            boids[i] = new Boid(flockID);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < boids.Length; i++)
        {
            // move all the boids
            Boid b = boids[i];
            b.velocity += EvalSeperation(b);
            b.velocity += EvalCohesion(b);
            b.velocity += EvalAlignment(b);
            b.velocity += EvalBoundary(b);
            b.velocity += EvalMaxSpeed(b);
            
            b.position += b.velocity * (Time.deltaTime);
        
            // draw boids
            b.Redraw();
        }
    }

    Vector3 EvalSeperation(Boid b)
    {
        Vector3 total = Vector3.zero;
        for (int i = 0; i < boids.Length; i++)
        {
            Boid bf = boids[i];
            if (bf != b)
            {
                if (Mathf.Abs((b.position - bf.position).sqrMagnitude) < seperationDistance)
                {
                    total += (b.position - bf.position);
                }
            }
        }

        return total;
    }
    
    Vector3 EvalCohesion(Boid b)
    {
        // get average positions
        Vector3 averageFlockPosition = Vector3.zero;
        int flockCounter = 0;
        
        Vector3 averagePosition = Vector3.zero;
        
        for (int i = 0; i < boids.Length; i++)
        {
            Boid bf = boids[i];
            if (bf != b)
            {
                if (bf.flockID == b.flockID)
                {
                    flockCounter++;
                    averageFlockPosition += bf.position;
                }
                averagePosition += bf.position;
            }
        }
        
        averageFlockPosition /= flockCounter;
        averagePosition /= boids.Length - 1;
        
        return (averageFlockPosition - b.position) * flockCohesion
               + (averagePosition - b.position) * nonFlockCohesion;
    }
    
    Vector3 EvalAlignment(Boid b)
    {
        Vector3 averageFlockVelocity = Vector3.zero;
        int flockCounter = 0;
        
        Vector3 averageVelocity = Vector3.zero;
        
        for (int i = 0; i < boids.Length; i++)
        {
            Boid bf = boids[i];
            if (bf != b)
            {
                if (bf.flockID == b.flockID)
                {
                    flockCounter++;
                    averageFlockVelocity += bf.velocity;
                }
                
                averageVelocity += bf.velocity;
            }
        }
        averageFlockVelocity /= (float)flockCounter;
        averageVelocity /= (float)boids.Length - 1;
        
        return (averageFlockVelocity - b.velocity) * flockAlignment
               + (averageVelocity - b.velocity) * nonFlockAlignment;
    }

    Vector3 EvalBoundary(Boid b)
    {
        float mag = b.position.magnitude;
        if (mag > fishBowlSize)
        {
            return -b.position * (mag - fishBowlSize);
        }
        else return Vector3.zero;
    }

    Vector3 EvalMaxSpeed(Boid b)
    {
        return Vector3.ClampMagnitude(b.velocity, maxSpeed) - b.velocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Vector3.zero, fishBowlSize);
    }
}

public class Boid
{
    public Vector3 position;
    public Vector3 velocity;
    public int flockID;
    Transform transform;

    public Boid(int _flockID)
    {
        flockID = _flockID;
        position = new Vector3(Random.Range(-5,5), Random.Range(-5,5), Random.Range(-5,5));
        velocity = new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1));
        transform = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
    }

    public void Redraw()
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward,velocity);
    }
    
}


