using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeveloperOverlay : MonoBehaviour
{
    [Header("Developer overlay")]
    [SerializeField] private bool showingOverlay = false;
    private Transform[] transforms = null;

    [Header("Outputs")]
    [SerializeField] private TextMeshProUGUI playerPositionOutput = null;
    [SerializeField] private TextMeshProUGUI chunksLoadedOutput = null;
    [SerializeField] private TextMeshProUGUI chunksInGameObjectOutput = null;
    [SerializeField] private TextMeshProUGUI chunksInHashmapOutput = null;

    [Header("Calculations")]
    [SerializeField] private Transform playerTarget = null;
    [SerializeField] private Transform worldParent = null;

    private void Start()
    {
        List<Transform> t = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            t.Add(transform.GetChild(i));
        }

        this.transforms = t.ToArray();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            showingOverlay = !showingOverlay;

            foreach (Transform rectTransform in transforms)
            {
                rectTransform.gameObject.SetActive(showingOverlay);
            }
        }

        if (showingOverlay)
        {
            playerPositionOutput.text = GetPlayerPosition().ToString();
            chunksLoadedOutput.text = GetLoadedChunksAmount().ToString();
            chunksInGameObjectOutput.text = GetAmountChunksInGameObjects().ToString();
            chunksInHashmapOutput.text = GetAmountChunksInHashMap().ToString();
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

    private int GetAmountChunksInGameObjects()
    {
        return worldParent.Cast<Transform>().Count(t => t.name != "Unused chunk");
    }

    private int GetAmountChunksInHashMap()
    {
        return HashSetPositionChecker.Count;
    }
}
