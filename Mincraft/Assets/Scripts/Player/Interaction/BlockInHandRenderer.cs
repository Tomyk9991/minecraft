using System.Collections;
using Attributes;
using Core.Builder;
using Core.Math;
using Core.UI.Ingame;
using UnityEngine;

namespace Core.Player
{
    public class BlockInHandRenderer : MonoBehaviour
    {
        [Header("Positions")] 
        [SerializeField] private Vector3 fadeInPosition = Vector3.zero;
        [SerializeField] private Vector3 fadeOutPosition = Vector3.zero;
        [Header("Settings")] 
        [SerializeField] private float fadeAnimationSpeed = 0.8f;
        
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
        private BlockUV previousBlock = BlockUV.Air;

        private const float distanceThreshold = 0.005f;

        private readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        private readonly Vector2[] uvBuffer = new Vector2[24];

        private void Start()
        {
            this.initialLocalXPosition = transform.localPosition.x;
            playerController = FirstPersonController.Instance;
            mesh = GetComponentInChildren<MeshFilter>().sharedMesh;

            QuickBarSelectionUI.Instance.OnSelectionChanged += SetBlock;
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


        private IEnumerator FadeInFadeOutAnimation(BlockUV block)
        {
            yield return FadeOutAnimation();

            SetUVFromBlockUV(block);

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