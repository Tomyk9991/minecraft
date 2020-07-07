using System;
using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Math;
using UnityEngine;
using Core.UI.Console;

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

        private readonly int chunkSize = 0x10;

        private Camera cameraRef;
        private ChunkJobManager chunkJobManager;
        private GameManager gameManager;
        
        private RaycastHit hit;
        private Int3 dir;
        private Int3 dirPlusOne;
        private Int3 blockPos;
        
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);
        
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            GoPool = ChunkGameObjectPool.Instance;
            cameraRef = Camera.main;
            chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
            gameManager = GameManager.Instance;
            
        }

        public void SetBlock(BlockUV uv) => blockUV = uv;

        private void Update()
        {
            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

                if (Physics.Raycast(ray, out hit, RaycastHitable))
                {
                    Chunk currentChunk = hit.transform.GetComponent<ChunkReferenceHolder>().Chunk;
                    Int3 localPos = GlobalToRelativeBlock(MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal), currentChunk.GlobalPosition);
                    
                    if (MathHelper.InChunkSpace(localPos))
                    {
                        HandleAddBlock(currentChunk, localPos);
                    }
                    else
                    {
                        GetDirectionPlusOne(localPos, ref dirPlusOne);
                        currentChunk = currentChunk.ChunkNeighbour(dirPlusOne);
                        localPos = GlobalToRelativeBlock(MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal), currentChunk.GlobalPosition);

                        HandleAddBlock(currentChunk, localPos);
                    }
                }
            }
        }

        private void HandleAddBlock(Chunk currentChunk, Int3 localPos)
        {
            currentChunk.AddBlock(new Block(blockUV), localPos);

            if (MathHelper.BorderBlock(localPos))
            {
                GetDirections(localPos, ref dir);

                if (dir.X != 0)
                {
                    Chunk neighbourChunk = currentChunk.ChunkNeighbour(dir.X == -1 ? Chunk.Directions[4] : Chunk.Directions[5]);
                    RelativeToLocalBlockMinusOneX(localPos, ref blockPos);
                    neighbourChunk.AddBlock(new Block(blockUV), blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk);
                }

                if (dir.Y != 0)
                {
                    Chunk neighbourChunk = currentChunk.ChunkNeighbour(dir.Y == -1 ? Chunk.Directions[3] : Chunk.Directions[2]);
                    RelativeToLocalBlockMinusOneY(localPos, ref blockPos);
                    neighbourChunk.AddBlock(new Block(blockUV), blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk);
                }

                if (dir.Z != 0)
                {
                    Chunk neighbourChunk = currentChunk.ChunkNeighbour(dir.Z == -1 ? Chunk.Directions[1] : Chunk.Directions[0]);
                    RelativeToLocalBlockMinusOneZ(localPos, ref blockPos);
                    neighbourChunk.AddBlock(new Block(blockUV), blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk);
                }
            }

            chunkJobManager.RecalculateChunk(currentChunk);
        }

        private void GetDirectionPlusOne(in Int3 localPos, ref Int3 dir)
        {
            dir.X = 0;
            dir.Y = 0;
            dir.Z = 0;
            if (localPos.X < 0) dir.X = -1;
            if (localPos.X >= chunkSize) dir.X = 1;
            
            if (localPos.Y < 0) dir.Y = -1;
            if (localPos.Y >= chunkSize) dir.Y = 1;
            
            if (localPos.Z < 0) dir.Z = -1;
            if (localPos.Z >= chunkSize) dir.Z = 1;
        }
        
        private void GetDirections(in Int3 localPos, ref Int3 dir)
        {
            dir.X = 0;
            dir.Y = 0;
            dir.Z = 0;
            
            if (localPos.X == 0) dir.X = -1;
            if (localPos.X >= 15) dir.X = 1;

            if (localPos.Y == 0) dir.Y = -1;
            if (localPos.Y >= 15) dir.Y = 1;

            if (localPos.Z == 0) dir.Z = -1;
            if (localPos.Z >= 15) dir.Z = 1;
        }

        private void RelativeToLocalBlockMinusOneX(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;
            
            if (relativeBlockPos.X == 15) blockPos.X = -1;
            if (relativeBlockPos.X == 0)  blockPos.X = 16;
        }
        
        private void RelativeToLocalBlockMinusOneY(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;
            
            if (relativeBlockPos.Y == 15) blockPos.Y = -1;
            if (relativeBlockPos.Y == 0)  blockPos.Y = 16;
        }
        
        private void RelativeToLocalBlockMinusOneZ(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;
            
            if (relativeBlockPos.Z == 15) blockPos.Z = -1;
            if (relativeBlockPos.Z == 0)  blockPos.Z = 16;
        }

        private Int3 GlobalToRelativeBlock(Vector3 globalBlockPos, Int3 globalChunkPos) 
            => new Int3((int) globalBlockPos.x - globalChunkPos.X, (int) globalBlockPos.y - globalChunkPos.Y, (int) globalBlockPos.z - globalChunkPos.Z);
    }
}