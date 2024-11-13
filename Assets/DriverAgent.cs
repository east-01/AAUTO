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
    
    private CarController _carController;

    private void Awake()
    {
        _carController = GetComponent<CarController>();
    }

    // Used to set spawn location
    public override void OnEpisodeBegin()
    {
        
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
        continuousActions[0] = Input.GetAxis(HORIZONTAL);;
        continuousActions[1] = Input.GetAxis(VERTICAL);
    }

    // Used to determine the reward for the Agent
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PositiveReward>(out PositiveReward positiveReward))
        {
            AddReward(1f);
            //EndEpisode();
        }

        if (other.TryGetComponent<NegativeReward>(out NegativeReward negativeReward))
        {
            AddReward(-1f);
            //EndEpisode();
        }
        
    }
}
