using Core.Builder;
using Core.Chunking;
using UnityEngine;

using Core.Chunking.Threading;
using Core.UI.Console;
using Core.Managers;

namespace Core.Player
{
    public class AddBlock : MonoBehaviour, IMouseUsable, IConsoleToggle
    {
        public ChunkGameObjectPool GoPool { get; set; }
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
        
        [SerializeField] private int mouseButtonIndex = 1;
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private BlockUV blockUV = BlockUV.Wood;

        private int chunkSize;
        
        private Camera cameraRef;

        private ChunkJobManager chunkJobManager;
        private MeshModifier modifier;

        private GameManager gameManager;

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            chunkSize = ChunkSettings.ChunkSize;
            GoPool = ChunkGameObjectPool.Instance;
            cameraRef = Camera.main;
            gameManager = GameManager.Instance;

            chunkJobManager = new ChunkJobManager();
            modifier = new MeshModifier();
            chunkJobManager.Start();
        }

        public void SetBlock(BlockUV uv) => blockUV = uv;

        private void Update()
        {
            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
    //            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);
    //
    //            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
    //            {
    //                Int3 globalCenterCubePosition =
    //                    Int3.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal));
    //
    //                Block block = new Block(globalCenterCubePosition)
    //                {
    //                    ID = (int)blockUV
    //                };
    //
    //                Chunk chunkOnClicked = ChunkDictionary.GetValue(hit.transform.position.ToInt3());
    //
    //                //Chunk chunk = chunkOnClicked.TryAddBlockFromGlobal(block, out Int3 chunkPos);
    //                Chunk chunk = chunkOnClicked.GetChunkFromGlobalBlock(block, out Int3 chunkPos);
    //
    //                if (chunk != null)
    //                {
    //                    chunk.AddBlock(block);
    //                    //TODO add to chunkjobmanager somehow
    //                    chunk.SaveChunk();
    //                }
    //
    //                //if (chunk != null)
    //                //{
    //                //    chunkJobManager.Add(new ChunkJob(chunk));
    //                //    chunk.SaveChunk();
    //                //}
    //            }
            }

            for (int i = 0; i < chunkJobManager.FinishedJobsCount; i++)
            {
                ChunkJob task = chunkJobManager.DequeueFinishedJob();

                if (task != null && task.Completed)
                {
                    modifier.SetMesh(task.Chunk.CurrentGO, task.MeshData, task.ColliderData);
                }
            }
        }

        private void OnDestroy()
        {
            //chunkJobManager.Dispose();
        }
    }
}
