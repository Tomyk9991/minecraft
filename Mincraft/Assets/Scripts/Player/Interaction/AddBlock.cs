using System;
using Core.Builder;
using Core.Chunks;
using Core.Managers;
using Core.Math;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Core.UI;
using UnityEngine;
using Core.UI.Ingame;
using Utilities;

namespace Core.Player.Interaction
{
    public class AddBlock : MonoBehaviour, IMouseUsable, IConsoleToggle, IFullScreenUIToggle
    {
        public static event Action OnAdd;
        public static event Action<BlockUV> OnAddBlock;
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
        [SerializeField]
        private Camera cameraRef = null;

        [Space] 
        [SerializeField] private int mouseButtonIndex = 1;
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private float timeBetweenRemove = 0.1f;
        [SerializeField] private LayerMask hitMask = 0;
        [SerializeField] private BlockUV blockUV = BlockUV.Wood;
        private GameManager gameManager;

        
        private RaycastHit hit;
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        private PlaceBlockHelper placer;
        
        private QuickBarSelectionUI currentSelectionUI;
        private Inventory inventory;

        private Timer timer;

        private void Start()
        {
            timer = new Timer(DesiredTimeUntilAction);

            placer = new PlaceBlockHelper
            {
                currentBlock =
                {
                    ID = blockUV
                }
            };
            
            inventory = Inventory.Instance;
            currentSelectionUI = QuickBarSelectionUI.Instance;
        }

        private void OnValidate()
            => timer.HardReset(timeBetweenRemove);

        private void CalculateQuickbarIndex(int index)
        {
            ItemData data = inventory.QuickBar[index];
            SetBlock(data == null ? BlockUV.Air : (BlockUV) data.ItemID);
        }

        public void SetBlock(BlockUV uv)
        {
            blockUV = uv;
            placer.currentBlock.ID = uv;
        }

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
                OnAdd?.Invoke();
                CalculateQuickbarIndex(currentSelectionUI.SelectedIndex);
                
                ChunkReferenceHolder holder;
                if (!hit.transform.TryGetComponent(out holder))
                    return;

                OnAddBlock?.Invoke(blockUV);


                if (placer.currentBlock.CanFaceInDifferentDirections())
                {
                    Vector3 delta = new Vector3(hit.point.x - ray.origin.x, 0.0f, hit.point.z - ray.origin.z);
                    BlockDirection blockDirection = MathHelper.BlockDirectionFromSignedAngle(Vector3.SignedAngle(delta, Vector3.forward, Vector3.up));
                    placer.currentBlock.Direction = blockDirection;
                }
                
                Chunk currentChunk = holder.Chunk;

                placer.latestGlobalClick = MathHelper.CenteredClickPositionOutSide(hit.point, hit.normal);


                placer.latestGlobalClickInt.X = (int) placer.latestGlobalClick.x;
                placer.latestGlobalClickInt.Y = (int) placer.latestGlobalClick.y;
                placer.latestGlobalClickInt.Z = (int) placer.latestGlobalClick.z;

                placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.LocalPosition);

                if (MathHelper.InChunkSpace(placer.LocalPosition))
                {
                    placer.HandleAddBlock(currentChunk, placer.LocalPosition);
                }
                else
                {
                    placer.GetDirectionPlusOne(placer.LocalPosition, ref placer.dirPlusOne);
                    currentChunk = currentChunk.ChunkNeighbour(placer.dirPlusOne);
                    placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.LocalPosition);

                    placer.HandleAddBlock(currentChunk, placer.LocalPosition);
                }
            }
        }
    }
}