using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrafficLightController : MonoBehaviour
{
   
    private Renderer ren;
    private MaterialPropertyBlock[] mpbs;

    private TrafficLightState trafficLightState;
    public TrafficLightState State { 
        get => trafficLightState;
        set {
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

        mpbs = new MaterialPropertyBlock[3];
        State = TrafficLightState.YELLOW;
    
        Debug.Log("Initialized state to " + State);
    }

    private void SetEmissive(TrafficLightState tls, bool emissive)
    {
        if(mpbs[(int)tls] == null) {
            mpbs[(int)tls] = new MaterialPropertyBlock();
            ren.SetPropertyBlock(mpbs[(int)tls], (int)tls);
        }

        mpbs[(int)tls].SetColor("_EmissionColor", emissive ? Color.white : new Color(0, 0, 0, 0));
        ren.SetPropertyBlock(mpbs[(int)tls], (int)tls);
        Debug.Log("Set property block for " + tls + " to " + mpbs[(int)tls].GetColor("_EmissionColor"));
    }

}

public enum TrafficLightState { YELLOW, GREEN, RED }