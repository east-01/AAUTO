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

#region Data In/Out
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
        float forwardAmount = actionBuffers.ContinuousActions[0];
        float turnAmount = actionBuffers.ContinuousActions[1];
        bool breakAmount = false;
        
        switch (actionBuffers.DiscreteActions[0])
        {
            case 0: breakAmount = false; break; //nothing
            case 1: breakAmount = true; break; //break
        }
        
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
        continuousActions[0] = Input.GetAxis(HORIZONTAL);
        continuousActions[1] = Input.GetAxis(VERTICAL);
    }
#endregion

    // Used to determine the reward for the Agent
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Curb")) {
            EndEpisode();
        } else if(other.CompareTag("TrainingTrigger") && other.TryGetComponent(out TrainingTrigger trigger)) {
            HandleTrainingTrigger(trigger);
        }          
    }

    private void HandleTrainingTrigger(TrainingTrigger trigger)
    {
        // Get the reward value defined by TrainingTrigger in scene
        float reward = trigger.reward;
        AddReward(reward);

        // Check if it's a path trigger
        if(trigger.transform.parent.TryGetComponent(out TrainingPath path)) {
            // Disable trigger since we've passed through it
            trigger.gameObject.SetActive(false);

            // Check if self is last trigger in path
            bool isLast = trigger == trigger.transform.parent.GetChild(trigger.transform.parent.childCount-1).gameObject;
            if(isLast)
                EndEpisode();
        }      
    }
}
