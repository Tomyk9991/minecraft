using System.Collections;
using Core.Builder;
using Core.UI.Ingame;
using UnityEngine;

namespace Core.Player
{
    public class BlockInHandRenderer : MonoBehaviour
    {
        [Header("Positions")]
        [SerializeField] private Vector3 fadeInPosition = Vector3.zero;
        [SerializeField] private Vector3 fadeOutPosition = Vector3.zero;
        
        private Mesh mesh = null;
        private MeshRenderer meshRenderer = null;
        private BlockUV previousBlock = BlockUV.Air;

        private float inAnimationTimer = 0.0f;
        private float outAnimationTimer = 0.0f;
        private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        
        private void Start()
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
            meshRenderer = GetComponent<MeshRenderer>();
            
            QuickBarSelectionUI.Instance.OnSelectionChanged += SetBlock;
        }
        
        private void SetBlock(BlockUV block)
        {
            Debug.Log("block changed");
            if (block == BlockUV.Air)
            {
                StartCoroutine(FadeOutAnimation());
                return;
            }
            
            if (previousBlock == BlockUV.Air)
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
                
                StartCoroutine(FadeInAnimation());
            }
            else
            {
                StartCoroutine(nameof(FadeInFadeOutAnimation), block);
            }

            this.previousBlock = block;
        }
        
        private IEnumerator FadeInFadeOutAnimation(BlockUV block)
        {
            yield return FadeOutAnimation();
            
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
            
            yield return FadeInAnimation();
        }

        private IEnumerator FadeOutAnimation()
        {
            while ((this.fadeOutPosition - this.transform.localPosition).sqrMagnitude > 0.05f)
            {
                this.outAnimationTimer += Time.deltaTime;
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
            while ((this.fadeInPosition - this.transform.localPosition).sqrMagnitude > 0.05f)
            {
                this.inAnimationTimer += Time.deltaTime;
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
