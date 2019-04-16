using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        UVDictionary.Init();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 500;
    }
}
