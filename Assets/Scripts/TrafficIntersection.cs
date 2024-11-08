using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates all traffic lights in an intersection.
/// </summary>
[DefaultExecutionOrder(1)]
public class TrafficIntersection : MonoBehaviour
{

    [SerializeField]
    private bool randomLightInitialization;

    [SerializeField]
    private List<TrafficDirection> trafficDirections;

    /// <summary>
    /// Represents the phase of the intersection, phase numbers are determined by:
    ///   (light direction index)*(light phase) so the indices of a two direction intersection are:
    ///      0            1             2            3            4             5
    /// [Dir1 Green] [Dir1 Yellow] [Dir1 Clear] [Dir2 Green] [Dir2 Yellow] [Dir2 Clear]
    /// </summary>
    private int _phaseIndex;
    private int phaseIndex {
        get => _phaseIndex;
        set {
            value %= maxPhaseIndex;
            _phaseIndex = value;

            int subPhaseIndex = _phaseIndex % TrafficDirection.PHASE_COUNT;
            int indexesDirection = Mathf.FloorToInt(phaseIndex / trafficDirections.Count);
            TrafficDirection trafficDirection = trafficDirections[indexesDirection];

            float subPhaseDuration = 0;
            TrafficLightState subPhaseTLState = TrafficLightState.RED;

            switch(subPhaseIndex) {
                case 0:
                    subPhaseDuration = trafficDirection.greenDuration;
                    subPhaseTLState = TrafficLightState.GREEN;
                    break;
                case 1:
                    subPhaseDuration = trafficDirection.yellowDuration;
                    subPhaseTLState = TrafficLightState.YELLOW;
                    break;
                case 2:
                    subPhaseDuration = trafficDirection.clearDuration;
                    subPhaseTLState = TrafficLightState.RED;
                    break;
                default:
                    Debug.LogError("Failed to handle phase change. (did TrafficDirection#PHASE_COUNT change?)");
                    break;
            }

            // Sub phase durations that are zero are not allowed since, in order to transition to
            //   the next phase, the Update method needs to see that __some__ time has passed.
            if(subPhaseDuration == 0)
                subPhaseDuration = 0.2f;

            phaseIncrementTime = Time.time + subPhaseDuration;
            trafficDirection.SetLightsState(subPhaseTLState);
        }
    }
    private int maxPhaseIndex;
    /// <summary>
    /// The target time for the program to reach for the phase to increment+1
    /// </summary>
    private float phaseIncrementTime;

    private void Start() 
    {
        // Cycle all lights to ensure triggers load properly
        trafficDirections.ForEach(dir => {
            foreach(TrafficLightState tlState in Enum.GetValues(typeof(TrafficLightState)))
                dir.SetLightsState(tlState);
            dir.SetLightsState(TrafficLightState.RED);
        });

        // Initialize max phase index values
        maxPhaseIndex = trafficDirections.Count*TrafficDirection.PHASE_COUNT;
        phaseIndex = randomLightInitialization ? UnityEngine.Random.Range(0, maxPhaseIndex) : 0;
    }

    private void Update() 
    {
        float lastFrameTime = Time.time - Time.deltaTime;
        if(Time.time > phaseIncrementTime && lastFrameTime <= phaseIncrementTime)
            phaseIndex++;
    }

}

[Serializable]
public struct TrafficDirection {
    public static readonly int PHASE_COUNT = 3;
    public float greenDuration;
    public float yellowDuration;
    public float clearDuration;
    public readonly float TotalDuration => greenDuration + yellowDuration + clearDuration;
    public List<TrafficLightController> trafficLights;

    public readonly void SetLightsState(TrafficLightState tlState) { 
        if(trafficLights != null && trafficLights.Count > 0)
            trafficLights.ForEach(tl => tl.State = tlState); 
    }
}