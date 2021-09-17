using System;
using Core.Builder;
using Core.Chunks;
using Core.Math;
using Core.UI;
using UnityEngine;
using Utilities;

namespace Core.Player.Interaction
{
    public class RemoveBlock : MonoBehaviour, IMouseUsable, IConsoleToggle, IFullScreenUIToggle
    {
        public static event Action OnRemove;
        private int chunkSize;

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

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        [Header("References")] 
        [SerializeField] private Camera cameraRef = null;
        
        [Space]
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private float timeBetweenRemove = 0.1f;
        [SerializeField] private int mouseButtonIndex = 0;
        [SerializeField] private LayerMask hitMask = 0;
        
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);
        private readonly Vector3 littleBlockSpawnOffset = new Vector3(0.5f, 0.5f, 0.5f);

        private RaycastHit hit;
        
        private PlaceBlockHelper placer;
        private Timer timer;
        private DroppedItemsManager droppedItemsManager;
        

        private void Start()
        {
            timer = new Timer(DesiredTimeUntilAction);
            placer = new PlaceBlockHelper
            {
                currentBlock =
                {
                    ID = BlockUV.None
                }
            };
            
            droppedItemsManager = DroppedItemsManager.Instance;
        }

        private void OnValidate()
            => timer.HardReset(timeBetweenRemove);

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

            if (Physics.Raycast(ray, out hit, RaycastDistance, hitMask))
            {
                ChunkReferenceHolder holder;
                if (!hit.transform.TryGetComponent(out holder))
                    return;

                OnRemove?.Invoke();
                Chunk currentChunk = holder.Chunk;
                

                placer.latestGlobalClick = MathHelper.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;


                placer.latestGlobalClickInt.X = (int) placer.latestGlobalClick.x;
                placer.latestGlobalClickInt.Y = (int) placer.latestGlobalClick.y;
                placer.latestGlobalClickInt.Z = (int) placer.latestGlobalClick.z;

                placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.lp);
                
                Block removedBlock;
                if (MathHelper.InChunkSpace(placer.lp))
                {
                    removedBlock = placer.HandleAddBlock(currentChunk, placer.lp);
                }
                else
                {
                    placer.GetDirectionPlusOne(placer.lp, ref placer.dirPlusOne);
                    currentChunk = currentChunk.ChunkNeighbour(placer.dirPlusOne);
                    placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.lp);

                    removedBlock = placer.HandleAddBlock(currentChunk, placer.lp);
                }
                
                if (droppedItemsManager == null) droppedItemsManager = DroppedItemsManager.Instance;
                
                GameObject go = droppedItemsManager.GetNextBlock();
                
                go.transform.position = placer.latestGlobalClickInt.ToVector3() + littleBlockSpawnOffset;
                go.GetComponent<DroppedItemInformation>().FromBlock(removedBlock, 1);
                
                droppedItemsManager.AddNewItem(go);
                
                // Remove lawn if needed
                RemoveLawn(holder);
            }
        }

        private void RemoveLawn(ChunkReferenceHolder holder)
        {
            Chunk currentChunk = holder.Chunk;
            
            placer.latestGlobalClick = MathHelper.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;

            placer.latestGlobalClick.y += 1;
            
            placer.latestGlobalClickInt.X = (int) placer.latestGlobalClick.x;
            placer.latestGlobalClickInt.Y = (int) placer.latestGlobalClick.y;
            placer.latestGlobalClickInt.Z = (int) placer.latestGlobalClick.z;

            placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.lp);
                
            Block removedBlock;
            if (MathHelper.InChunkSpace(placer.lp))
            {
                removedBlock = placer.HandleAddBlock(currentChunk, placer.lp, BlockUV.Lawn);
            }
            else
            {
                placer.GetDirectionPlusOne(placer.lp, ref placer.dirPlusOne);
                currentChunk = currentChunk.ChunkNeighbour(placer.dirPlusOne);
                placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.lp);

                removedBlock = placer.HandleAddBlock(currentChunk, placer.lp, BlockUV.Lawn);
            }
        }
    }
}