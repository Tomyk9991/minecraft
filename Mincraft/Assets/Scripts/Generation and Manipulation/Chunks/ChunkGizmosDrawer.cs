using UnityEngine;

public class ChunkGizmosDrawer : MonoBehaviour
{
    private Vector3 position;
    private Vector3 size;

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !ChunkSettings.Instance.drawGizmosChunks || gameObject.name == "Unused chunk" || !GetComponent<MeshRenderer>().enabled)
            return;

        if (position == default || size == default)
        {
            position = this.transform.position;
            size = Vector3.one * ChunkSettings.GetMaxSize;
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(position + size / 2, size);
    }
}
