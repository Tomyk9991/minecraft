using Core.Builder.Generation;
using UnityEngine;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static bool WorldSelected { get; set; }

        private static readonly string currentPathWorldDefault =
            @"C:\Users\thoma\AppData\LocalLow\DefaultCompany\Minecraft\Worlds\_debug_world_";

        private static string currentWorldPath = "";

        public static string CurrentWorldPath
        {
            get => currentWorldPath == "" ? currentPathWorldDefault : currentWorldPath;
            set => currentWorldPath = value;
        }

        private static GameManager instance;

        private NoiseSettings _noiseSettings;

        public NoiseSettings NoiseSettings
        {
            get => _noiseSettings;
            set
            {
                WorldSelected = true;
                _noiseSettings = value;
            }
        }

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
        }
    }
}