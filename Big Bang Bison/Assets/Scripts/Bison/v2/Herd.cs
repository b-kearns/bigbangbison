﻿/*
    NewHerd.cs
    Caetano 
    1/27/19
    Caetano
    Class for bison behavior
    Functions in file:
        GetNearbyObjects: In, agent - Out, list of nearby objects not ignored by bison
        .SquareAvoidanceRadius: Out, the square of the avoidance radius
        .SpawnBison: In, number to spawn - Out, spawns 'em
    
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The Herd is an object that will contain and manage each HerdAgent (Bison).   Herd = Flock
public class Herd : MonoBehaviour
{
    // These are set in the editor
    public HerdAgent agentPrefab; // The prefab used when instantiating new agents
    public HerdBehavior idleBehavior;
    public HerdBehavior runningBehavior;
    public HerdBehavior panicBehavior;
    public HerdBehavior outOfPlayBehavior;

    private List<HerdBehavior> behaviors;// The behaviors run on each agent, sorted by state

    private float bisonHeight;

    // A list of all the agents in this herd
    List<HerdAgent> agents = new List<HerdAgent>(); // it starts empty

    // The maximum number of agents to spawn in this herd
    [Range(1, 200)]
    public int startingCount = 77; // defaults to 77

    // These variables affect the behaviors in the herd
    [Range(0f, 20f)]
    public float driveFactor = 4f; // increases the intensity of behaviors
    [Range(0f, 1f)]
    public float idleDrag = 0.15f; // drag while idle
    [Range(1f, 100f)]
    public float maxSpeed = 5f; // the max acceleration
    [Range(1f, 20f)]
    public float neighborRadius = 15f; // how close something has to be to be detected as a neighbor
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.33f; // the radius for agents to avoid things, relative to neighborRadius
    [Range(0, 20)]
    public int crowdingThreshhold = 15;

    // Math is faster when done with squares, save them here
    float squareMaxSpeed;
    float squareNeighBorRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius {
        get { return squareAvoidanceRadius; }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Math is faster when done with squares, calculate them here
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighBorRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighBorRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        // Get the distance bison should be from the ground
        bisonHeight = agentPrefab.GetComponent<Collider>().bounds.extents.y;

        behaviors = new List<HerdBehavior>() {
            idleBehavior,
            runningBehavior,
            panicBehavior,
            outOfPlayBehavior };// The behaviors run on each agent, sorted by state

        // Make all the Bison
        for (int i = 0; i < startingCount; i++)
        {
            SpawnBison(20);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Iterate through all agents and  move them
        foreach (HerdAgent agent in agents)
        {
            List<Transform> context = GetNearbyObjects(agent); // get the context

            // Determine state
            if (!Physics.Raycast(agent.transform.position, -Vector3.up, bisonHeight + 0.5f))
            {
                agent.state = 3; // Off the ground
                agent.agentBody.drag = 0.75f;
            } else if (agent.state != 2)
            {
                agent.state = 0;
                agent.agentBody.drag = idleDrag;
            }

            // Calculate move
            Vector3 move = behaviors[agent.state].CalculateMove(agent, context, this); // figure out how much the agent should move, and which behavior it's using
            move *= driveFactor; // make it bigger
            if (move.sqrMagnitude > squareMaxSpeed) move = move.normalized * maxSpeed; // cap it at max
            agent.Move(move); // tell it to move itself

            // colors crowded bison
            // agent.GetComponentInChildren<MeshRenderer>().material.color = Color.Lerp(Color.white, Color.red, context.Count / 8f);
        }
    }

    // Gets nearby objects for bison, and sets their state
    List<Transform> GetNearbyObjects(HerdAgent agent)
    {
        int layerMask = ~(1 << 10); // used to ignore objects in layer 10

        List<Transform> context = new List<Transform>(); // the list to return

        Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, neighborRadius, layerMask); // gets all colliders near the agent

        if (contextColliders.Length > crowdingThreshhold) // If i'm crowded, panic
        {
            agent.state = 2;
            agent.agentBody.drag = 0;
        }

        foreach (Collider c in contextColliders)
        {
            if (c != agent.AgentCollider) // if the collider isn't its own
            {
                context.Add(c.transform); // add it to the context
            }
            if (c.gameObject.layer == 11 && agent.state != 2)
            {
                agent.state = 1;
                agent.agentBody.drag = 0;
            }
        }
        return context;
    }

    // Make some new boys
    void SpawnBison(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawn = (Vector3)Random.insideUnitCircle * 100; // random point in a circle with an x and y, and a radius of 
            spawn.z = spawn.y; // we want z
            spawn.y = 0; // not y

            // Yo, it's a new bison
            HerdAgent newAgent = Instantiate(
                agentPrefab,
                spawn,
                Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)), // random start rotation
                transform // herd position
                );
            newAgent.name = "Bison " + agents.Count;
            newAgent.Initialize(this); // agent has startup function
            agents.Add(newAgent);
        }
    }
}
