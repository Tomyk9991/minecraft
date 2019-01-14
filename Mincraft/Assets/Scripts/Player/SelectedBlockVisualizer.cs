using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class SelectedBlockVisualizer : MonoBehaviour
{
    public float RaycastHitable
    {
        get => raycastHitable;
        set => raycastHitable = value;
    }
    
    [SerializeField] private float raycastHitable = 1000f;
    
    private Camera cameraRef;

    private void Start()
    {
        cameraRef = Camera.main;
    }
    
    private void Update()
    {
        Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
        {
            Vector3 blockPos = ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;
            transform.position = blockPos + Vector3.one / 2;
            transform.rotation = Quaternion.identity;
        }
    }
    
    private Vector3 gizmosPos;
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawWireCube(gizmosPos + Vector3.one / 2, Vector3.one);
        }
    }
}