using System.Collections;
using Attributes;
using Core.Builder;
using Core.UI.Ingame;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace Core.Player
{
    public class BlockInHandRenderer : MonoBehaviour
    {
        [Header("Positions")] 
        [SerializeField] private Vector3 fadeInPosition = Vector3.zero;
        [SerializeField] private Vector3 fadeOutPosition = Vector3.zero;
        [Header("Settings")]
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] private float distanceThreshold = 0.005f;
        [Header("Bobbing")] 
        [SerializeField] private bool useHandBob = true;

        [DrawIfTrue(nameof(useHandBob)), SerializeField] private AnimationCurve bobCurve = new AnimationCurve();
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private Camera virtualCamera = null;
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private float deltaHeight = 1.0f;
        [DrawIfTrue(nameof(useHandBob)), SerializeField] private float stepInterval = 3.0f;
        
        private FirstPersonController playerController;
        private float evaluationValue = 0.0f;
        
        private Mesh mesh = null;
        private BlockUV previousBlock = BlockUV.Air;
        
        private float inAnimationTimer = 0.0f;
        private float outAnimationTimer = 0.0f;
        private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

        private void Start()
        {
            playerController = FirstPersonController.Instance;
            mesh = GetComponent<MeshFilter>().sharedMesh;
            QuickBarSelectionUI.Instance.OnSelectionChanged += SetBlock;
        }

        private void Update()
        {
            UpdateCameraBobbing();
        }

        private void UpdateCameraBobbing()
        {
            if (!this.useHandBob)
                return;

            if (playerController.CharacterController.velocity.magnitude > 0)
            {
                evaluationValue += Time.deltaTime * stepInterval;
                evaluationValue %= 1.0f;
                Vector3 localPos = virtualCamera.transform.localPosition;
                localPos.y = bobCurve.Evaluate(evaluationValue) * deltaHeight;
                
                virtualCamera.transform.localPosition = localPos;
            }
        }

        private void SetBlock(BlockUV block)
        {
            if (block == BlockUV.Air)
            {
                StartCoroutine(FadeOutAnimation());
                return;
            }

            if (previousBlock == BlockUV.Air)
            {
                SetUVFromBlockUV(block);

                StartCoroutine(FadeInAnimation());
            }
            else
            {
                StartCoroutine(nameof(FadeInFadeOutAnimation), block);
            }

            this.previousBlock = block;
        }

        private void SetUVFromBlockUV(BlockUV block)
        {
            Vector2[] uvs = new Vector2[24];
            UVData[] currentUVData = UVDictionary.GetValue(block);

            for (int i = 0; i < 24; i += 4)
            {
                UVData uvData = currentUVData[i / 4];
                uvs[i + 0] = new Vector2(uvData.TileX, uvData.TileY);
                uvs[i + 1] = new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY);
                uvs[i + 2] = new Vector2(uvData.TileX, uvData.TileY + uvData.SizeY);
                uvs[i + 3] = new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY + uvData.SizeY);
            }

            mesh.SetUVs(0, uvs);
        }
        

        private IEnumerator FadeInFadeOutAnimation(BlockUV block)
        {
            yield return FadeOutAnimation();

            SetUVFromBlockUV(block);

            yield return FadeInAnimation();
        }

        private IEnumerator FadeOutAnimation()
        {
            while ((this.fadeOutPosition - this.transform.localPosition).sqrMagnitude > distanceThreshold)
            {
                this.outAnimationTimer += Time.deltaTime;
                this.outAnimationTimer /= this.animationSpeed;
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.fadeOutPosition,
                    this.outAnimationTimer);
                yield return endOfFrame;
            }

            this.transform.localPosition = this.fadeOutPosition;
            this.outAnimationTimer = 0.0f;

            yield return endOfFrame;
        }

        private IEnumerator FadeInAnimation()
        {
            while ((this.fadeInPosition - this.transform.localPosition).sqrMagnitude > distanceThreshold)
            {
                this.inAnimationTimer += Time.deltaTime;
                this.inAnimationTimer /= this.animationSpeed;
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.fadeInPosition,
                    this.inAnimationTimer);
                yield return endOfFrame;
            }

            this.transform.localPosition = this.fadeInPosition;
            this.inAnimationTimer = 0.0f;

            yield return endOfFrame;
        }
    }
}