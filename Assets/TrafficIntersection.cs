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

    

}

public struct TrafficDirection {
    public float greenDuration;
    public List<TrafficLightController> trafficLights;
}