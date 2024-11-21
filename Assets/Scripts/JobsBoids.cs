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
    private NativeArray<float3> newBoidPositions;
    private NativeArray<float3> boidVelocities;
    private NativeArray<float3> newBoidVelocities;
    private int[] boidInstanceIDs;
    private Dictionary<int, GameObject> instanceMappings = new Dictionary<int, GameObject>();
    
    void Start()
    {
        // set up arrays
        boidPositions = new NativeArray<float3>(initialSpawnAmount, Allocator.Persistent);
        newBoidPositions = new NativeArray<float3>(initialSpawnAmount, Allocator.Persistent);
        boidVelocities = new NativeArray<float3>(initialSpawnAmount, Allocator.Persistent);
        newBoidVelocities = new NativeArray<float3>(initialSpawnAmount, Allocator.Persistent);
        boidInstanceIDs = new int[initialSpawnAmount];
        
        // populate and set up the boids
        for (int i = 0; i < initialSpawnAmount; i++)
        {
            boidPositions[i] = Random.insideUnitSphere * fishBowlSize;
            boidVelocities[i] = Random.insideUnitSphere * maxSpeed;
            
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            instance.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            instanceMappings.Add(instance.GetInstanceID(), instance);
            boidInstanceIDs[i] = instance.GetInstanceID();
        }
    }

    void FixedUpdate()
    {
        // make and schedule job
        JobsBoidsJob job = new JobsBoidsJob()
        {
            oldPositions = boidPositions,
            oldVelocities = boidVelocities,
            
            newPositions = newBoidPositions,
            newVelocities = newBoidVelocities,
        };
        JobHandle jobHandle = job.Schedule(initialSpawnAmount, 64);
        jobHandle.Complete();
        
        // update the boid positions
        boidPositions = job.newPositions;
        boidVelocities = job.newVelocities;
        
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

    private void OnDestroy()
    {
        boidPositions.Dispose();
        boidVelocities.Dispose();
    }
}

public struct JobsBoidsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> oldVelocities;
    [ReadOnly] public NativeArray<float3> oldPositions;

    [WriteOnly] public NativeArray<float3> newVelocities;
    [WriteOnly] public NativeArray<float3> newPositions;
    
    public void Execute(int index)
    {
        
    }
}


