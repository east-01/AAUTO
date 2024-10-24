using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

/// <summary>
/// Controls a single traffic light- purpose built for the specific traffic light prefab that we're
///   going to be using. See why it's purpose built in SetEmissive.
/// </summary>
public class TrafficLightController : MonoBehaviour
{
   
    private Renderer ren;
    private MaterialPropertyBlock[] mpbs;

    private TrafficLightState trafficLightState;
    public TrafficLightState State { 
        get => trafficLightState;
        set {
            // If we're not making a change don't do anything
            if(trafficLightState == value)
                return;

            // Disable old emission
            foreach(TrafficLightState tls in Enum.GetValues(typeof(TrafficLightState))) {
                if(tls != value)
                    SetEmissive(tls, false);
            }

            trafficLightState = value;
            SetEmissive(trafficLightState, true);
        }
    }

    private void Awake() 
    {
        ren = GetComponent<Renderer>();

        if(ren.materials.Length < 3) {
            Debug.LogError($"TrafficLightController on gameObject \"{gameObject.name}\" failed to resolve the materials necessary for changing emission status.");
            gameObject.SetActive(false);
            return;
        }

        mpbs = new MaterialPropertyBlock[ren.materials.Length];
        State = TrafficLightState.RED;
    }

    // TODO: Test code to get traffic lights flashing, should use TrafficIntersection for control
    private void Update() 
    {        
        if(Mathf.Floor(Time.time-Time.deltaTime) != Mathf.Floor(Time.time)) {
            State = (TrafficLightState)(Mathf.Floor(Time.time)%Enum.GetValues(typeof(TrafficLightState)).Length);
        }
    }

    /// <summary>
    /// Set the emissive property for a specific TrafficLightState
    /// </summary>
    private void SetEmissive(TrafficLightState tls, bool emissive)
    {
        int matIdx = TLSToMaterialIndex(tls);

        if(mpbs[matIdx] == null)
            mpbs[matIdx] = new MaterialPropertyBlock(); // Assign new object if null

        mpbs[matIdx].SetColor("_EmissionColor", emissive ? Color.white : new Color(0, 0, 0, 0));

        ren.SetPropertyBlock(mpbs[matIdx], matIdx);
    }

    /// <summary>
    /// Get the material index relating to a specific TrafficLightState.
    /// If we're looking for the RED state this method will return the red material.
    /// This method is REQUIRED as the way the materials are ordered in the object's Renderer
    ///   should correspond to the index of the enum.
    /// </summary>
    private int TLSToMaterialIndex(TrafficLightState tls) 
    {
        // Right now, the materials are laid out in the format of BASE (the black background), RED,
        //   YELLOW, GREEN.
        return (int)tls+1;
    }

}

public enum TrafficLightState { RED, YELLOW, GREEN }