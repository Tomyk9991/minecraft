using System.Collections;
using Attributes;
using Core.Builder;
using Core.Math;
using Core.Player.Interaction;
using Core.UI.Ingame;
using UnityEngine;

namespace Core.Player
{
    public class ItemInHandRenderer : MonoBehaviour
    {
        [Header("Positions")] 
        [SerializeField] private Vector3 fadeInPosition = Vector3.zero;
        [SerializeField] private Vector3 fadeOutPosition = Vector3.zero;
        
        [Header("Settings")] 
        [SerializeField] private float fadeAnimationSpeed = 0.8f;
        
        [Header("Custom mesh settings")] 
        [SerializeField] private Mesh blockMesh = null;
        [SerializeField] private Material blockMaterial = null;
        [Space(10)]
        [SerializeField] private Vector3 blockPosition = Vector3.zero;
        [SerializeField] private Vector3 blockRotation = Vector3.zero;
        [Space(10)]
        [SerializeField] private Vector3 itemPosition = Vector3.zero;
        [SerializeField] private Vector3 itemRotation = Vector3.zero;
        
        [Header("Bobbing")] 
        [SerializeField] private bool useHandBob = true;
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private AnimationCurve bobCurve = new AnimationCurve();
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private Camera virtualCamera = null;
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private float deltaHeight = 1.0f;
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private float stepInterval = 3.0f;

        [Header("Rapid turns")] 
        [SerializeField] private bool useRapidTurns = true;
        [DrawIfTrue(nameof(useRapidTurns)), SerializeField] private float localPositionOffset = 0.0f;
        [DrawIfTrue(nameof(useRapidTurns)), SerializeField] private float rapidTurnAnimationSpeed = 0.0f;

        private float initialLocalXPosition = 0.0f;
        private float turnTimer = 0.0f;

        private FirstPersonController playerController;
        private float evaluationValue = 0.0f;

        private Mesh mesh = null;
        private int previousItem = 0;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private const float distanceThreshold = 0.005f;

        private readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        private readonly Vector2[] uvBuffer = new Vector2[24];

        private void Start()
        {
            this.initialLocalXPosition = transform.localPosition.x;
            playerController = FirstPersonController.Instance;
            
            meshFilter = GetComponentInChildren<MeshFilter>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            
            mesh = meshFilter.sharedMesh;

            QuickBarSelectionUI.Instance.OnSelectionChanged += SetItem;
        }

        private void Update()
        {
            UpdateCameraBobbing();
            UpdateRapidTurnAnimation();
        }

        private void UpdateRapidTurnAnimation()
        {
            if (!this.useRapidTurns)
                return;

            float deltaMouseX = Input.GetAxis("Mouse X");
            float clampLimit = 10.0f;
            deltaMouseX = Mathf.Clamp(deltaMouseX, -clampLimit, clampLimit);


            Vector3 finalLocalPosition = transform.localPosition;

            finalLocalPosition.x = Mathf.SmoothDamp(
                transform.localPosition.x,
                this.initialLocalXPosition - MathHelper.Map(deltaMouseX, -clampLimit, clampLimit,
                    -this.localPositionOffset, this.localPositionOffset),
                ref turnTimer, rapidTurnAnimationSpeed);


            transform.localPosition = finalLocalPosition;
        }

        private void UpdateCameraBobbing()
        {
            if (!this.useHandBob)
                return;

            if (playerController.CharacterController.velocity.magnitude > 0 &&
                playerController.CharacterController.isGrounded)
            {
                evaluationValue += Time.deltaTime * stepInterval;
                evaluationValue %= 1.0f;
                Vector3 localPos = virtualCamera.transform.localPosition;
                localPos.y = bobCurve.Evaluate(evaluationValue) * deltaHeight;

                virtualCamera.transform.localPosition = localPos;
            }
        }

        private void SetItem(int item)
        {
            StopAllCoroutines();
            
            if (item == -1)
            {
                StartCoroutine(FadeOutAnimation());
                return;
            }

            if (previousItem == -1)
            {
                FindProperMeshMaterialPair(item);
                StartCoroutine(FadeInAnimation());
            }
            else
            {
                StartCoroutine(nameof(FadeInFadeOutAnimation), item);
            }

            this.previousItem = item;
        }

        private void FindProperMeshMaterialPair(int item)
        {
            bool isBlock = item <= short.MaxValue;
            BlockUV blockUV = isBlock ? (BlockUV) item : BlockUV.Air;
            var technique = UVDictionary.RenderingTechnique(blockUV);

            if (technique == RenderingTechnique.Block)
                SetUVFromBlockUV(blockUV);
            else if (technique == RenderingTechnique.CustomMesh) 
                SetMeshFromItemMesh(item);
        }

        private void SetUVFromBlockUV(BlockUV block)
        {
            bool isPreviousItemBlock = previousItem <= short.MaxValue;

            if (isPreviousItemBlock)
            {
                UpdateMeshSettings(blockMesh, blockMaterial);
                UpdateTransform(this.blockPosition, this.blockRotation);
            }
            
            UVData[] currentUVData = UVDictionary.GetValue(block);

            for (int i = 0; i < 24; i += 4)
            {
                UVData uvData = currentUVData[i / 4];
                uvBuffer[i + 0] = new Vector2(uvData.TileX, uvData.TileY);
                uvBuffer[i + 1] = new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY);
                uvBuffer[i + 2] = new Vector2(uvData.TileX, uvData.TileY + uvData.SizeY);
                uvBuffer[i + 3] = new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY + uvData.SizeY);
            }

            mesh.SetUVs(0, uvBuffer);
        }

        private void SetMeshFromItemMesh(int item)
        {
            MeshMaterialPair pair = ItemDictionary.GetMeshMaterialPair(item);
            
            // Must be set every time its called. If the item type changes the mesh must still be upgraded 
            UpdateMeshSettings(pair.Mesh, pair.Material);
            this.UpdateTransform(this.itemPosition, this.itemRotation);
        }


        private void UpdateMeshSettings(Mesh mesh, Material material)
        {
            this.meshFilter.sharedMesh = mesh;
            this.meshRenderer.material = material;
        }

        private void UpdateTransform(Vector3 targetPos, Vector3 targetRotation)
        {
            this.transform.localPosition = targetPos;
            this.transform.localRotation = Quaternion.Euler(targetRotation);

            this.fadeInPosition.y = targetPos.y;
        }


        private IEnumerator FadeInFadeOutAnimation(int item)
        {
            yield return FadeOutAnimation();

            FindProperMeshMaterialPair(item);

            yield return FadeInAnimation();
        }
        
        private IEnumerator FadeOutAnimation()
        {
            float timer = 0.0f;
            while ((this.fadeOutPosition - this.transform.localPosition).sqrMagnitude > distanceThreshold)
            {
                timer += Time.deltaTime;
                timer /= this.fadeAnimationSpeed;
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.fadeOutPosition, timer);
                yield return endOfFrame;
            }

            this.transform.localPosition = this.fadeOutPosition;

            yield return endOfFrame;
        }

        private IEnumerator FadeInAnimation()
        {
            float timer = 0.0f;
            while ((this.fadeInPosition - this.transform.localPosition).sqrMagnitude > distanceThreshold)
            {
                timer += Time.deltaTime;
                timer /= this.fadeAnimationSpeed;
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.fadeInPosition, timer);
                yield return endOfFrame;
            }

            this.transform.localPosition = this.fadeInPosition;

            yield return endOfFrame;
        }
    }
}