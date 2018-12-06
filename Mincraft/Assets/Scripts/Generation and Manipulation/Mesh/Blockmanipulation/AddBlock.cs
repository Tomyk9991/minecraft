using UnityEngine;

public class AddBlock : MonoBehaviour, IMouseUsable
{
    public int MouseButtonIndex
    {
        get => mouseButtonIndex;
        set => mouseButtonIndex = value;
    }
    
    [SerializeField] private int mouseButtonIndex = 1;
    [SerializeField] private GameObject[] blocks = null;
    private GameObject newBlock;
    private ChunkManager chunkManager;
    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;
        SetBlock(0);
    }

    private void SetBlock(int index) => newBlock = blocks[index];

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 centeredCubePosition = ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal);
                GameObject block = Instantiate(newBlock, centeredCubePosition, Quaternion.identity, hit.transform);
                block.name = block.transform.position.ToString();
                
                chunkManager.AddBlock(block);
            }
        }
    }

}