using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeveloperOverlay : MonoBehaviour
{
    [Header("Developer overlay")]
    [SerializeField] private bool showingOverlay = false;
    [SerializeField] private RectTransform[] transforms = null;

    [Header("Outputs")]
    [SerializeField] private TextMeshProUGUI playerPositionOutput = null;
    [SerializeField] private TextMeshProUGUI chunksLoadedOutput = null;

    [Header("Calculations")]
    [SerializeField] private Transform playerTarget = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            showingOverlay = !showingOverlay;

            foreach (RectTransform rectTransform in transforms)
            {
                rectTransform.gameObject.SetActive(showingOverlay);
            }
        }

        if (showingOverlay)
        {
            playerPositionOutput.text = GetPlayerPosition().ToString();
            chunksLoadedOutput.text = GetLoadedChunksAmount().ToString();
        }
    }

    private Int3 GetPlayerPosition()
    {
        return playerTarget.position.ToInt3();
    }

    private int GetLoadedChunksAmount()
    {
        return ChunkDictionary.Count;
    }
}
