using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Core.Managers;
using Core.Saving;

namespace Core.UI.Menu
{
    public class PlayDialogUI : MonoBehaviour
    {
        //wenn später Welten einen eigenen Namen haben können, achte darauf, dass die Namen verschieden sind
        [Header("Button")]
        [SerializeField] private Button backButton = null;
        [SerializeField] private Button loadButton = null;
        [SerializeField] private Button createNewWorldButton = null;
        [SerializeField] private Button deleteButton = null;

        [Header("Dropdown")]
        [SerializeField] private TMP_Dropdown dropdown = null;

        [Header("Transforms to invert")]
        [SerializeField] private RectTransform[] transforms = null;

        private readonly string PATH = @"C:/Users/thoma/Documents/MinecraftCloneWorlds";
        private int highestIndex = 0;

        List<string> directoryNames = new List<string>();

        private void Start()
        {
            //Dropdown
            RefreshDropdown();
            dropdown.captionText.text = dropdown.value != -1 ? dropdown.options[dropdown.value].text : "";
            dropdown.onValueChanged.AddListener((value) =>
            {
                if (value != -1)
                {
                    loadButton.interactable = true;
                    deleteButton.interactable = true;
                }
            });

            if (dropdown.value == -1)
            {
                loadButton.interactable = false;
                deleteButton.interactable = false;
            }

            //Backbutton
            backButton.onClick.AddListener(() =>
            {
                SetChildsVisibility(false);
            });

            //CreateButton
            createNewWorldButton.onClick.AddListener(() =>
            {
                ContextIO.CreateDirectory(highestIndex);
                RefreshDropdown();
                dropdown.value = dropdown.options.Count - 1;

                loadButton.interactable = dropdown.value != -1;
                deleteButton.interactable = dropdown.value != -1;
            });

            //DeleteButton
            deleteButton.onClick.AddListener(() =>
            {
                string fullPath = PATH + "/" + dropdown.captionText.text + "/";

                if (Directory.Exists(fullPath))
                    Directory.Delete(fullPath, true);

                if (dropdown.captionText.text == "" || GetOptionData().Count == 0)
                {
                    dropdown.value = -1;
                    loadButton.interactable = false;
                    deleteButton.interactable = false;

                    Debug.Log("bitte nicht oberordner löschen. danke");
                }

                RefreshDropdown();
            });

            loadButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < directoryNames.Count; i++)
                {
                    if (dropdown.options[dropdown.value].text == directoryNames[i])
                    {
                        GameManager.CurrentWorldName = directoryNames[i];
                        break;
                    }
                }  
                SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            });
        }
        public void SetChildsVisibility(bool state)
        {
            foreach (Transform t in this.transform)
            {
                t.gameObject.SetActive(state);
            }

            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.SetActive(!state);
            }
        }

        private List<TMP_Dropdown.OptionData> GetOptionData()
        {
            directoryNames.Clear();
            if (!Directory.Exists(PATH))
                Directory.CreateDirectory(PATH);

            Directory.GetDirectories(PATH)
                .ToList()
                .ForEach(t =>
                {
                    directoryNames.Add(t.Split('\\').Last());
                });

            List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < directoryNames.Count; i++)
            {
                data.Add(new TMP_Dropdown.OptionData(directoryNames[i]));
            }

            highestIndex = directoryNames.Count;

            return data;
        }

        private void RefreshDropdown()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(GetOptionData());
        }
    }
}
