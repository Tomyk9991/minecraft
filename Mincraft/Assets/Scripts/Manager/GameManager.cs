using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        UVDictionary.Init();
        Application.targetFrameRate = 300;
        QualitySettings.vSyncCount = 0;
    }
}
