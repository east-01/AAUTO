using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingPath : MonoBehaviour
{

    [SerializeField]
    private List<string> travelInstructions;
    public string TravelInstructions => string.Join(", ", travelInstructions);

    public void Initialize() 
    {
        for(int childIdx = 0; childIdx < transform.childCount; childIdx++) {
            transform.GetChild(childIdx).gameObject.SetActive(true);
        }
    }

    private void OnDrawGizmos() 
    {
        if(transform.childCount < 2)
            return;

        Vector3? prevPos = null;
        for(int childIdx = 0; childIdx < transform.childCount; childIdx++) {
            GameObject child = transform.GetChild(childIdx).gameObject;
            float radius = 0.6f;
            if(childIdx == 0) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(child.transform.position, child.transform.position+child.transform.forward);
                Gizmos.DrawWireSphere(child.transform.position+child.transform.forward, 0.25f);

                Gizmos.color = Color.cyan;
                radius = 1f;
            } else if(childIdx == transform.childCount - 1) {
                Gizmos.color = new Color(1, 0.25f, 0f);
                radius = 1f;
            } else {
                Gizmos.color = Color.blue;
            }

            Gizmos.DrawWireSphere(child.transform.position, radius);

            if(prevPos.HasValue) {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(prevPos.Value, child.transform.position);
            }

            prevPos = child.transform.position;
        }
    }

}
