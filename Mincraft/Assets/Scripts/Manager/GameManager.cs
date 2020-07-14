using UnityEditor;
using UnityEngine;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static bool WorldSelected { get; set; }
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance != null) return instance;
                GameObject g = new GameObject("Manager");
                DontDestroyOnLoad(g);
                instance = g.AddComponent<GameManager>();

                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 500;
            Screen.SetResolution(1920, 1080, FullScreenMode.MaximizedWindow);


            // Debug.Log(Application.persistentDataPath);

            if (WorldSelected)
            {
            }
        }
    }
}