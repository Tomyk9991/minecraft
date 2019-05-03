using UnityEngine;

public class SelectedBlockVisualizer : MonoBehaviour, IConsoleToggle
{
    public float RaycastHitable
    {
        get => raycastHitable;
        set => raycastHitable = value;
    }
    public bool Enabled
    {
        get => this.enabled;
        set => this.enabled = value;
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
        }
        else
        {
            transform.position = default;
        }
    }
}