using System;
using System.Runtime.CompilerServices;
using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Math;
using UnityEngine;
using Core.UI.Console;
using Utilities;

namespace Core.Player.Interaction
{
    public class AddBlock : MonoBehaviour, IMouseUsable, IConsoleToggle
    {
        public ChunkGameObjectPool GoPool { get; set; }

        public float DesiredTimeUntilAction
        {
            get => timeBetweenRemove;
            set => timeBetweenRemove = value;

        }

        public float RaycastDistance
        {
            get => raycastHitable;
            set => raycastHitable = value;
        }

        public int MouseButtonIndex
        {
            get => mouseButtonIndex;
            set => mouseButtonIndex = value;
        }

        [Header("References")] [SerializeField]
        private Camera cameraRef;

        [Space] 
        [SerializeField] private int mouseButtonIndex = 1;
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private float timeBetweenRemove = 0.1f;
        [SerializeField] private BlockUV blockUV = BlockUV.Wood;

        private readonly int chunkSize = 0x10;

        private ChunkJobManager chunkJobManager;
        private GameManager gameManager;

        private Timer timer;
        
        private RaycastHit hit;
        private Int3 dir;
        private Int3 dirPlusOne;
        private Int3 blockPos;
        private Vector3 latestGlobalClick;
        private Int3 latestGlobalClickInt;
        private Block currentBlock;

        private Int3 lp;

        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            timer = new Timer(DesiredTimeUntilAction);
            chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
            gameManager = GameManager.Instance;
            currentBlock.ID = blockUV;
        }

        private void OnValidate()
            => timer.HardReset(timeBetweenRemove);

        public void SetBlock(BlockUV uv) => blockUV = uv;

        private void Update()
        {
            if (Input.GetMouseButton(mouseButtonIndex) && timer.TimeElapsed(Time.deltaTime))
            {
                DoRaycast();
            }

            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                DoRaycast();
                timer.Reset();
            }
        }

        private void DoRaycast()
        {
            Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

            if (Physics.Raycast(ray, out hit, RaycastDistance))
            {
                ChunkReferenceHolder holder;
                if (!hit.transform.TryGetComponent(out holder))
                    return;

                Chunk currentChunk = holder.Chunk;

                latestGlobalClick = MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal);

                latestGlobalClickInt.X = (int) latestGlobalClick.x;
                latestGlobalClickInt.Y = (int) latestGlobalClick.y;
                latestGlobalClickInt.Z = (int) latestGlobalClick.z;

                GlobalToRelativeBlock(latestGlobalClick, currentChunk.GlobalPosition, ref lp);

                if (MathHelper.InChunkSpace(lp))
                {
                    HandleAddBlock(currentChunk, lp);
                }
                else
                {
                    GetDirectionPlusOne(lp, ref dirPlusOne);
                    currentChunk = currentChunk.ChunkNeighbour(dirPlusOne);
                    GlobalToRelativeBlock(latestGlobalClick, currentChunk.GlobalPosition, ref lp);

                    HandleAddBlock(currentChunk, lp);
                }
            }
        }

        private void HandleAddBlock(Chunk currentChunk, Int3 localPos)
        {
            if (PlayerMovementTracker.CurrentStandingBlock == latestGlobalClickInt) return;

            currentChunk.AddBlock(currentBlock, localPos);

            if (MathHelper.BorderBlock(localPos))
            {
                GetDirections(localPos, ref dir);

                if (dir.X != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.X == -1 ? Chunk.Directions[4] : Chunk.Directions[5]);
                    RelativeToLocalBlockMinusOneX(localPos, ref blockPos);
                    neighbourChunk.AddBlock(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk, ChunkJobPriority.High);
                }

                if (dir.Y != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.Y == -1 ? Chunk.Directions[3] : Chunk.Directions[2]);
                    RelativeToLocalBlockMinusOneY(localPos, ref blockPos);
                    neighbourChunk.AddBlock(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk, ChunkJobPriority.High);
                }

                if (dir.Z != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.Z == -1 ? Chunk.Directions[1] : Chunk.Directions[0]);
                    RelativeToLocalBlockMinusOneZ(localPos, ref blockPos);
                    neighbourChunk.AddBlock(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk, ChunkJobPriority.High);
                }
            }

            chunkJobManager.RecalculateChunk(currentChunk, ChunkJobPriority.High);
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
            if (relativeBlockPos.X == 0) blockPos.X = 16;
        }

        private void RelativeToLocalBlockMinusOneY(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.Y == 15) blockPos.Y = -1;
            if (relativeBlockPos.Y == 0) blockPos.Y = 16;
        }

        private void RelativeToLocalBlockMinusOneZ(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.Z == 15) blockPos.Z = -1;
            if (relativeBlockPos.Z == 0) blockPos.Z = 16;
        }

        private void GlobalToRelativeBlock(Vector3 globalBlockPos, Int3 globalChunkPos, ref Int3 globalToRel)
        {
            globalToRel.X = (int) globalBlockPos.x - globalChunkPos.X;
            globalToRel.Y = (int) globalBlockPos.y - globalChunkPos.Y;
            globalToRel.Z = (int) globalBlockPos.z - globalChunkPos.Z;
        }
    }
}