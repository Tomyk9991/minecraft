using UnityEditor;
using UnityEngine;

public class ChunkSavingSettings : MonoBehaviour
{
    [SerializeField] private string absolutePath;

    private ContextIO<Chunk> chunkIOManager;
    private ChunkJobManager manager;
    private MeshModifier modifier;

    private void SaveChunk()
    {
        GameObject go = GameObject.Find("Chunks").transform.GetChild(0).gameObject;
        Chunk c = ChunkDictionary.GetValue(go.transform.position.ToInt3());

        chunkIOManager = new ContextIO<Chunk>(path: absolutePath, worldIndex: 0);
        chunkIOManager.SaveContext(c);
        Debug.Log("Chunk saved");
    }

    private void LoadChunk()
    {
        if (chunkIOManager == null)
        {
            chunkIOManager = new ContextIO<Chunk>(path: absolutePath, worldIndex: 0);
        }
        Chunk c = chunkIOManager.LoadContext();
        Debug.Log("Chunk loaded");
        Debug.Log(c.Position);

        c.CurrentGO = ChunkGameObjectPool.Instance.GetNextUnusedChunk();
        c.CurrentGO.transform.position = c.Position.ToVector3();
        c.CurrentGO.SetActive(true);
        c.CurrentGO.name = "New Chunk";
        
        manager = new ChunkJobManager();
        manager.Start();
        manager.Add(new ChunkJob(c));
        modifier = new MeshModifier();
    }

    private void Update()
    {
        if (manager == null)
            return;


        for (int i = 0; i < manager.FinishedJobs.Count; i++)
        {
            ChunkJob job = manager.DequeueFinishedJobs();

            modifier.SetMesh(job.Chunk.CurrentGO, job.MeshData, job.ColliderData);
        }
    }

    #region CustomEditor
    #if UNITY_EDITOR
    [CustomEditor(typeof(ChunkSavingSettings))]
    public class ChunkSavingSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ChunkSavingSettings css = (ChunkSavingSettings) target;

            if (GUILayout.Button("Load path"))
            {
                string path = EditorUtility.SaveFolderPanel("Folderlocation for saving worlds", "", "");
                if (path.Length != 0)
                {
                    css.absolutePath = path;
                }
                else
                {
                    css.absolutePath = "";
                }
            }

            if (GUILayout.Button("Save chunk [DEBUG]"))
            {
                css.SaveChunk();
            }
            if (GUILayout.Button("Load chunk [DEBUG]"))
            {
                css.LoadChunk();
            }
        }
    }
    #endif
    #endregion
}

