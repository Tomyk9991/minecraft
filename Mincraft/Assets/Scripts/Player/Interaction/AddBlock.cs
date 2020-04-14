using Core.Builder;
using Core.Chunking;
using UnityEngine;

using Core.Chunking.Threading;
using Core.UI.Console;
using Core.Managers;
using Core.Math;

namespace Core.Player.Interaction
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
        private GameManager gameManager;

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            chunkSize = 0x10;
            GoPool = ChunkGameObjectPool.Instance;
            cameraRef = Camera.main;
            gameManager = GameManager.Instance;

        }

        public void SetBlock(BlockUV uv) => blockUV = uv;

        private void Update()
        {
//            if (Input.GetMouseButtonDown(mouseButtonIndex))
//            {
//                Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);
//    
//                if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
//                {
//                    Int3 globalCenterBlockPosition =
//                        Int3.FloorToInt(MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal));

            //Convert the global chunkposition (hit.transform.position.ToInt3() to local space
                    //Chunk chunkOnClicked =  get chunk reference
    
                    //Chunk chunk = chunkOnClicked.GetChunkFromGlobalBlock(block, out Int3 chunkPos);
                    //if blocks over-ranges the chunk you have to add the block to the neighbourchunk
                    //get the index for the neighbouringchunk
                    //int idx = getNeighbourIndex(chunkOnClicked, globalCenterBlockPosition);
                    //get the chunkposition
                    //chunkOnClicked.CalculateNeighbour()
                    //chunk != null ?!?!? notwendig? => chunk.addblock(createBlockWithGlobalPosToLocalPos)
                    //chunk.save()
            //TODO add to chunkJobManager a priority so the chunks, that were manipulated get recalculated first
            //Do the same for the noisejobmanager, if necessary
        }

    }
}
