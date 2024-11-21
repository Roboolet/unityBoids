using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class JobsBoids : MonoBehaviour
{
    [FormerlySerializedAs("spawnAmount")]
    [Header("Initial parameters")]
    [SerializeField] private int initialSpawnAmount;
    
    [Header("Boid parameters")]
    [SerializeField, Range(0, 100)] private float maxSpeed;
    [SerializeField, Range(0, 1)] private float nonFlockCohesion;
    [SerializeField, Range(0, 1)] private float nonFlockAlignment;
    [SerializeField, Range(0, 20)] private float seperationDistance;
    
    [Header("Container parameters")]
    [SerializeField, Range(5, 50)] private float fishBowlSize;
    
    // data representing each boid
    // order of data must NOT be modified, as each index corresponds to a single boid
    private NativeArray<float3> boidPositions; 
    private NativeArray<float3> boidVelocities;
    private NativeArray<int> boidInstanceIDs;
    private Dictionary<int, GameObject> instanceMappings = new Dictionary<int, GameObject>();
    
    void Start()
    {
        // populate and set up the boids
        for (int i = 0; i < initialSpawnAmount; i++)
        {
            boidPositions[i] = Random.insideUnitSphere * fishBowlSize;
            boidVelocities[i] = Random.insideUnitSphere * maxSpeed;
            
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instance.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        }
    }

    void FixedUpdate()
    {
        RefreshInstanceObjects();
    }

    void RefreshInstanceObjects()
    {
        for (int i = 0; i < initialSpawnAmount; i++)
        {
            GameObject instance = instanceMappings[boidInstanceIDs[i]];
            instance.transform.position = boidPositions[i];
            
            // rotation isnt actually accurate but whatever
            instance.transform.rotation = 
                Quaternion.LookRotation(Vector3.forward,boidVelocities[i]);
        }
    }
}

public struct JobsBoidsJob : IJobParallelFor
{
    public void Execute(int index)
    {
        
    }
}


