using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingEnvironment : MonoBehaviour
{
    [SerializeField]
    private List<TrainingPath> pathOptions;

    /// <summary>
    /// Initialize the training environment and set the drive
    /// </summary>
    /// <param name="agent"></param>
    /// <returns></returns>
    public TrainingPath Initialize(DriverAgent agent) 
    {

        // Pick path from random options
        TrainingPath selectedPath = pathOptions[Random.Range(0, pathOptions.Count)];

        // Set path to active
        selectedPath.gameObject.SetActive(true);

        // Set car to start of path
        Transform startTransform = selectedPath.transform.GetChild(0);
        agent.transform.SetPositionAndRotation(startTransform.position, startTransform.rotation);

        return selectedPath;

    }
}
