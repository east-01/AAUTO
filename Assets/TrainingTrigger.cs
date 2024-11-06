using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrainingTrigger : MonoBehaviour
{
    private BoxCollider _collider;
    private new BoxCollider collider { get {
        if(_collider == null)
            _collider = GetComponent<BoxCollider>();
        return _collider;
    } }

    public float reward = 0;

    private void OnDrawGizmos() 
    {
        if(reward > 0)
            Gizmos.color = Color.green;
        else if(reward < 0)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.yellow;
        
        // Save the current Gizmos matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Set the Gizmos matrix to match the BoxCollider's transform
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw the wireframe mesh using the BoxCollider's size
        Gizmos.DrawWireMesh(
            GetCubeMesh(),
            collider.center,
            Quaternion.identity,
            collider.size
        );

        // Restore the previous Gizmos matrix
        Gizmos.matrix = oldMatrix;
    }

    private Mesh GetCubeMesh()
    {
        // Unity's built-in cube mesh is used. You can also create your own if needed.
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = cube.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(cube); // Cleanup the temporary cube
        return cubeMesh;
    }
}
