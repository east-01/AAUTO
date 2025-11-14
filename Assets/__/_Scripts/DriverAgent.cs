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
        sensor.AddObservation(transform.position); //counts as 3 observations (x,y,z)
    }
    
    // Actions agent can do
    // REQUIRES: Behavior Parameters -> Actions -> Discrete/Continuous Branch = # Discrete/Continuous actions
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float forwardAmount = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float turnAmount = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        bool breakAmount = actionBuffers.DiscreteActions[0] == 1;
        
        _carController.SetInput(forwardAmount, turnAmount, breakAmount);
    }

    //Used for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        bool isBreaking = Input.GetKey(KeyCode.Space);
        int boolAction = 0;
        if(isBreaking) boolAction = 1;
        
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = boolAction;
        
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis(HORIZONTAL);;
        continuousActions[1] = Input.GetAxis(VERTICAL);
    }

    // Used to determine the reward for the Agent
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Curb"))
        {
            EndEpisode();
            return;
        }
        if(!other.CompareTag("TrainingTrigger"))
            return;

        // Get the reward value defined by TrainingTrigger in scene
        float reward = other.GetComponent<TrainingTrigger>().reward;
        AddReward(reward);
        // if(reward < 0f) EndEpisode();

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
