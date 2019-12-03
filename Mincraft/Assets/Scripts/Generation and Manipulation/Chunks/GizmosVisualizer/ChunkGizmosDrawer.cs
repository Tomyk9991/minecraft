﻿using UnityEngine;

namespace Core.Chunking.Debugging
{
    public class ChunkGizmosDrawer : MonoBehaviour
    {
        private Vector3 position;
        private Vector3 size;

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !WorldSettings.Instance.drawGizmosChunks /*|| gameObject.name == "Unused chunk"*/ || !GetComponent<MeshRenderer>().enabled)
                return;

            if (position == default || size == default)
            {
                    position = this.transform.position;
                size = Vector3.one * WorldSettings.ChunkSize;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(position + size / 2, size);
        }
    }
}
