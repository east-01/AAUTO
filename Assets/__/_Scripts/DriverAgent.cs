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
    private Rigidbody _rb;

    private void Awake()
    {
        _carController = GetComponent<CarController>();
        _rb = GetComponent<Rigidbody>();
    }

    private TrainingPath selectedPath;

    // Used to set spawn location
    public override void OnEpisodeBegin()
    {
        selectedPath = environment.Initialize(this);
        _directions = selectedPath.TravelInstructions;

        Debug.Log($"Picked path \"{selectedPath.gameObject.name}\" with directions: \"{_directions}\"");
    }

    private void FinishEpisode()
    {
        EndEpisode();        
    }

#region Data In/Out
    // Information fed to the agent
    // REQUIRES: Behavior Parameters -> Vector Observation -> Space Size = # of observations fed to agent
    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(transform.position); //counts as 3 observations (x,y,z)
        sensor.AddObservation(transform.InverseTransformDirection(_rb.linearVelocity)); // 3 observations, (local x,y,z velocity)
        sensor.AddObservation(_carController.NormalizedSpeed); // 1 observation, values 0 to 1
        sensor.AddObservation(_carController.NormalizedSteer); // 1 observation, values -1 to 1
    }
    
    private float lastSeenProgress;
    // Actions agent can do
    // REQUIRES: Behavior Parameters -> Actions -> Discrete/Continuous nch = # Discrete/Continuous actions
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // NOTE: Reversing is forbidden- change lower bound of clamp to -1 for reverse
        float forwardAmount = Mathf.Clamp(actionBuffers.ContinuousActions[0], 0f, 1f);
        float turnAmount = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        float breakAmount = Mathf.Clamp(actionBuffers.ContinuousActions[2], 0f, 1f);
        //bool breakAmount = actionBuffers.DiscreteActions[0] == 1;

        // Persistent negative reward over time
        float timeReward = -0.01f;
        AddReward(timeReward);

        Vector3 startPos = selectedPath.transform.GetChild(0).position;
        Vector3 endPos = selectedPath.transform.GetChild(selectedPath.transform.childCount - 1).position;
        float progress = GetLineProgress(transform.position, startPos, endPos);
        float progressDelta = progress - lastSeenProgress;
        AddReward(progressDelta * 2f);
        lastSeenProgress = progress;

        _carController.SetInput(forwardAmount, turnAmount, breakAmount);
    }

    //Used for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //bool isBreaking = Input.GetKey(KeyCode.Space);
        //int boolAction = 0;
        //if(isBreaking) boolAction = 1;

        //ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        //discreteActions[0] = boolAction;
        float breakInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis(VERTICAL);
        continuousActions[1] = Input.GetAxis(HORIZONTAL);
        continuousActions[2] = breakInput;
    }
#endregion

    // Used to determine the reward for the Agent
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Curb")) {
            FinishEpisode();
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
                FinishEpisode();
        }      
    }

    public float GetLineProgress(Vector3 position, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineMagnitude = lineDirection.magnitude;
        Vector3 lineNormalized = lineDirection / lineMagnitude;

        Vector3 pointLineStart = transform.position - lineStart;
        float dotProduct = Vector3.Dot(pointLineStart, lineNormalized);

        dotProduct = Mathf.Clamp(dotProduct, 0f, lineMagnitude);
        return dotProduct/lineMagnitude;
    }
}
