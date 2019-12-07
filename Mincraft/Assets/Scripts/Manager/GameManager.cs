using Core.Saving;
using UnityEngine;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private string currentWorldName = "";
        [SerializeField] private string absolutePath;

        private static GameManager instance;
        private SavingJob savingJob;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject g = new GameObject("Manager");
                    DontDestroyOnLoad(g);
                    instance = g.AddComponent<GameManager>();
                }

                return instance;
            }
            private set => instance = value;
        }

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        public static string AbsolutePath
        {
            get => Instance.absolutePath;
            set => Instance.absolutePath = value;
        }

        public SavingJob SavingJob
        {
            get => savingJob;
        }
        public static string CurrentWorldName
        {
            get
            {
                return Instance.currentWorldName;
            }
            set => Instance.currentWorldName = value;
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 500;
            Screen.SetResolution(1920, 1080, FullScreenMode.MaximizedWindow);

            savingJob = new SavingJob();
            savingJob.Start();
        }

        private void OnDestroy()
        {
            savingJob.Dispose();
        }
    }
}
