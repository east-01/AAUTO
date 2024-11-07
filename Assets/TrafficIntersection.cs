using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates all traffic lights in an intersection.
/// </summary>
public class TrafficIntersection : MonoBehaviour
{

    [SerializeField]
    private List<TrafficDirection> trafficDirections;

    private int currDirIdx;
    private float lastChangeTime;

    private void Start() 
    {
        lastChangeTime = Time.time;

        // Cycle all lights to ensure triggers load properly
        trafficDirections.ForEach(dir => {
            foreach(TrafficLightState tlState in Enum.GetValues(typeof(TrafficLightState)))
                dir.SetLightsState(tlState);
            dir.SetLightsState(TrafficLightState.RED);
        });
    }

    private void Update() 
    {
        TrafficDirection currentDirection = trafficDirections[currDirIdx];

        float targetGreenTime = lastChangeTime + currentDirection.greenDuration;
        float targetYellowTime = targetGreenTime + currentDirection.yellowDuration;
        float lastFrameTime = Time.time - Time.deltaTime;

        if(lastFrameTime == lastChangeTime) { 
            currentDirection.SetLightsState(TrafficLightState.GREEN);
            Debug.Log("    Switched to green");
        } else if(Time.time > targetGreenTime && lastFrameTime <= targetGreenTime) {
            currentDirection.SetLightsState(TrafficLightState.YELLOW);
            Debug.Log("    Switched to yellow");
        } else if(Time.time > targetYellowTime && lastFrameTime <= targetYellowTime) {
            currentDirection.SetLightsState(TrafficLightState.RED);
            currDirIdx = (currDirIdx+1)%trafficDirections.Count;
            lastChangeTime = Time.time;

            Debug.Log($"Intersection \"{gameObject.name}\" on direction {currDirIdx}");
        }

    }

}

[Serializable]
public struct TrafficDirection {
    public float greenDuration;
    public float yellowDuration;
    public List<TrafficLightController> trafficLights;

    public readonly void SetLightsState(TrafficLightState tlState) { 
        if(trafficLights != null && trafficLights.Count > 0)
            trafficLights.ForEach(tl => tl.State = tlState); 
    }
}