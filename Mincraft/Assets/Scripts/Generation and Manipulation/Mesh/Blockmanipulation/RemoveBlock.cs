using System.Collections.Concurrent;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RemoveBlock : MonoBehaviour, IMouseUsable
{
    private int chunkSize;
    public float RaycastHitable
    {
        get => raycastHitable;
        set => raycastHitable = value;
    }

    public int MouseButtonIndex
    {
        get => mouseButtonIndex;
        set => mouseButtonIndex = value;
    }
    
    [SerializeField] private float raycastHitable = 1000f;
    [SerializeField] private int mouseButtonIndex = 0;

    private Camera cameraRef;
    
    private ChunkManager chunkManager;
    private MeshModifier modifier;
    private ConcurrentQueue<MeshData> meshDatas;
    private (Vector3Int lowerBound, Vector3Int higherBound) tuple;

    private Vector3Int latest;

    private void Start()
    {
        cameraRef = Camera.main;
        chunkManager = ChunkManager.Instance;
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();
        chunkSize = ChunkManager.GetMaxSize;
        modifier.MeshAvailable += (s, data) => meshDatas.Enqueue(data);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
            {
                //GET BLOCK POSITION
                Vector3Int centerCube = Vector3Int.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal);
                //GET CHUNK TO OPERATE IN
                IChunk chunk = ChunkGameObjectDictionary.GetValue(hit.transform.gameObject);
                chunk.RemoveBlock(centerCube);

                MeshData data = ModifyMesh.Combine(chunk);
                modifier.RedrawMeshFilter(data.GameObject, data);
                
                var bounds = chunk.GetChunkBounds();
                this.tuple = bounds;
                var tuple = chunkManager.IsBoundBlock(bounds, centerCube);
                
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + chunk.Position;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            MeshData data1 = ModifyMesh.Combine(neigbourChunk);
                            modifier.RedrawMeshFilter(data1.GameObject, data1);
                        }
                    }
                }
            }
        }
    }
}
