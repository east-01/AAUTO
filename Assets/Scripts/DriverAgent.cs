using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class DriverAgent : Agent
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    
    [SerializeField]
    private TrainingEnvironment environment;
    [SerializeField]
    private List<string> pathPrefixWhitelist;
    public List<string> PathPrefixWhitelist => pathPrefixWhitelist;

    private CarController _carController;
    /// <summary>
    /// Directions string to be used by model
    /// </summary>
    private string _directions;

    private void Awake()
    {
        _carController = GetComponent<CarController>();
    }

    // Used to set spawn location
    public override void OnEpisodeBegin()
    {
        TrainingPath path = environment.Initialize(this);
        _directions = path.TravelInstructions;

        Debug.Log($"Picked path \"{path.gameObject.name}\" with directions: \"{_directions}\"");
    }

    // Information fed to the agent
    // REQUIRES: Behavior Parameters -> Vector Observation -> Space Size = # of observations fed to agent
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
    }
    
    // Actions agent can do
    // REQUIRES: Behavior Parameters -> Actions -> Discrete Branch & Branches = # of actions
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;
        bool breakAmount = false;

        switch (actionBuffers.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break; //nothing
            case 1: forwardAmount = 1f; break; //forward
            case 2: forwardAmount = -1f; break; //backward
        }
        switch (actionBuffers.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break; //nothing
            case 1: turnAmount = 1f; break; //right
            case 2: turnAmount = -1f; break; //left
        }
        switch (actionBuffers.DiscreteActions[2])
        {
            case 0: breakAmount = false; break; //nothing
            case 1: breakAmount = true; break; //break
        }
        
        _carController.SetInput(forwardAmount, turnAmount, breakAmount);
    }

    //Used for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float horizontalInput = Input.GetAxis(HORIZONTAL);
        float verticalInput = Input.GetAxis(VERTICAL);
        bool isBreaking = Input.GetKey(KeyCode.Space);

        int forwardAction = 0;
        int turnAction = 0;
        int boolAction = 0;
        
        if(horizontalInput > 0.01f) forwardAction = 1;
        if(horizontalInput < -0.01f) forwardAction = 2;
        if(verticalInput > 0.01f) turnAction = 1;
        if(verticalInput < -0.01f) turnAction = 2;
        if(isBreaking) boolAction = 1;
        
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
        discreteActions[2] = boolAction;
        
        // ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        // discreteActions[0] = boolAction;
        // ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        // continuousActions[0] = Input.GetAxis(HORIZONTAL);;
        // continuousActions[1] = Input.GetAxis(VERTICAL);
    }

    // Used to determine the reward for the Agent
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("TrainingTrigger"))
            return;

        // Get the reward value defined by TrainingTrigger in scene
        float reward = other.GetComponent<TrainingTrigger>().reward;
        AddReward(reward);

        // Check if it's a path trigger
        if(other.transform.parent.TryGetComponent(out TrainingPath path)) {
            // Disable trigger since we've passed through it
            other.gameObject.SetActive(false);

            // Check if self is last trigger in path
            bool isLast = other == other.transform.parent.GetChild(other.transform.parent.childCount-1).gameObject;
            if(isLast)
                EndEpisode();
        }        
    }
}
