using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Builder.Generation;
using Core.Managers;
using Core.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.UI.MainMenu
{
    public class PlayMenuManager : MonoBehaviour
    {
        //TODO
        //C:\Users\thoma\AppData\LocalLow\DefaultCompany\Minecraft
        [Header("Visibility References")] [SerializeField]
        private GameObject mainMenuParent = null;

        [SerializeField] private GameObject playMenuParent = null;

        [Header("Instantiation")] [SerializeField]
        private GameObject viewPortParent = null;

        [SerializeField] private int subTitleFontSize = 13;
        [SerializeField] private GameObject itemPrefab = null;

        [Header("Scrollbar visibility")] [SerializeField]
        private int visibilityThreshold = 3;

        [SerializeField] private ScrollRect scrollRect = null;
        [SerializeField] private GameObject slidingArea = null;
        [SerializeField] private RectTransform contentHeightAdjustable = null;

        [Header("Create Game settings")] [SerializeField]
        private GameObject toggleCreateGameDialog = null;

        [SerializeField] private TMP_Text createGameButtonText = null;
        [SerializeField] private TMP_Text invalidWorldNameLabel = null;
        [SerializeField] private TMP_InputField seedInputField = null;
        [SerializeField] private TMP_InputField inputText = null;
        [SerializeField] private int maxCharsInputWorld = 15;


        private WaitForSeconds waiter = new WaitForSeconds(2.0f);

        private List<string> currentAvailableMaps = new List<string>();


        private void Start()
        {
            ConstructAvailableMaps();
        }

        private void ConstructAvailableMaps()
        {
            var worlds = MainMenuSavingManager.LoadWorldInformation();

            currentAvailableMaps.Clear();

            foreach (Transform child in viewPortParent.transform)
            {
                Destroy(child.gameObject);
            }

            if (worlds != null)
            {
                foreach (var t in worlds)
                {
                    currentAvailableMaps.Add(t.WorldName);
                    GameObject g = Instantiate(itemPrefab, viewPortParent.transform);
                    g.GetComponentInChildren<TMP_Text>().text = $"{t.WorldName.Split('\\').Last()}\n" +
                                                                $"<color=#EEEEEE><size={subTitleFontSize}>World Size: {t.Size:F}MB</size></color>";

                    Button[] buttons = g.GetComponentsInChildren<Button>();
                    buttons[1].onClick.AddListener(() =>
                    {
                        GameManager.Instance.NoiseSettings = t.NoiseSettings;
                        GameManager.CurrentWorldPath = Path.Combine(Application.persistentDataPath, "Worlds", t.WorldName);
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
                    });

                    buttons[2].onClick.AddListener(() =>
                    {
                        MainMenuSavingManager.DeleteWorldInformation(t);
                        ConstructAvailableMaps();
                    });
                }
            }

            contentHeightAdjustable.sizeDelta = new Vector2(contentHeightAdjustable.sizeDelta.x,
                worlds.Count * 50 + worlds.Count * 3);
        }

        //Called from Unity
        public void CreateWorld()
        {
            if (inputText.text == "" || currentAvailableMaps.Contains(inputText.text))
            {
                StopAllCoroutines();
                StartCoroutine(nameof(ShowInvalidName2Sec));
                return;
            }

            NoiseSettings settings = new NoiseSettings(2.5f, seedInputField.text == "" ? 
                UnityEngine.Random.Range(-100_000, 100_000) : 
                (seedInputField.text.GetHashCode() % 100_000)
            );
            MainMenuSavingManager.SaveWorldInformation(new WorldInformation(0, inputText.text, settings), inputText.text);
            
            inputText.text = "";
            seedInputField.text = "";
            
            ToggleCreateGameDialog();
            ConstructAvailableMaps();
        }

        private IEnumerator ShowInvalidName2Sec()
        {
            invalidWorldNameLabel.gameObject.SetActive(true);
            yield return waiter;
            invalidWorldNameLabel.gameObject.SetActive(false);
        }

        //Called from Unity
        public void InputFieldValueChanged(string newValue)
        {
            if (newValue.Any(c =>
                c == '\\' || c == '/' || c == ':' || c == '*' || c == '?' || c == '<' || c == '>' || c == '|' ||
                c == '.'))
            {
                StopAllCoroutines();
                StartCoroutine(nameof(ShowInvalidName2Sec));
                inputText.text = newValue.Substring(0, newValue.Length - 1);
            }

            if (newValue.Length >= maxCharsInputWorld)
            {
                inputText.text = newValue.Substring(0, maxCharsInputWorld);
            }
        }

        //Called from Unity
        public void ToggleCreateGameDialog()
        {
            toggleCreateGameDialog.SetActive(!toggleCreateGameDialog.activeSelf);
            createGameButtonText.SetText(toggleCreateGameDialog.activeSelf ? "Hide Dialog" : "Create Game");
        }

        //Called from Unity
        public void OnBackClick()
        {
            playMenuParent.SetActive(false);
            mainMenuParent.SetActive(true);
        }
    }
}