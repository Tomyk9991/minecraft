using System.Linq;
using Core.Managers;
using Core.Player;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Core.UI.Console;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.UI.Ingame
{
    public class EscapeMenuManager : SingletonBehaviour<EscapeMenuManager>, IConsoleToggle
    {
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        [Header("References")]
        [SerializeField] private Transform[] escapeMenuToggleTransforms = null;
        
        [Header("Settings")] 
        [SerializeField] private Transform settingsTransformParent = null;
        
        [Header("Settings references")]
        [Header("Field of View")]
        [SerializeField] private Camera cameraRef = null;
        [SerializeField] private Slider fovSlider = null;

        [Header("Player mouse sensitivity")] 
        [SerializeField] private FirstPersonController playerController = null;
        [SerializeField] private Slider mouseSensitivitySlider = null;
        

        [Space]
        public static bool showingEscapeMenu = false;
        private IConsoleToggle[] disableOnInventoryAppear = null;

        private void Start()
        {
            disableOnInventoryAppear = FindObjectsOfType<MonoBehaviour>().OfType<IConsoleToggle>().Where(t => (object)t != this).ToArray();
            
            fovSlider.onValueChanged.AddListener((float value) =>
            {
                cameraRef.fieldOfView = value;
            });
            
            mouseSensitivitySlider.onValueChanged.AddListener((float value) =>
            {
                playerController.MouseBehaviour.YSensitivity = value;
                playerController.MouseBehaviour.XSensitivity = value;
            });
        }


        public void OnResumeButtonClick()
        {
            SetEscapeWorkability(false);
        }

        public void OnSettingsButtonClick()
        {
            var settings = MainMenuSavingManager.LoadSettings();
            
            fovSlider.value = settings.fovSlider;
            
            settingsTransformParent.gameObject.SetActive(true);
            
            foreach (Transform child in escapeMenuToggleTransforms)
            {
                child.gameObject.SetActive(false);
            }
        }

        public void OnBackToMainMenu()
        {
            var settings = MainMenuSavingManager.LoadSettings();
            settings.fovSlider = (int) cameraRef.fieldOfView;
            
            PlayerMovementTracker.Instance.OnApplicationQuit();
            Inventory.Instance.OnApplicationQuit();
            MainMenuSavingManager.SaveSettings(settings);
            
            
            SceneManager.LoadScene(0);
        }

        public void OnBackFromSettingsMenuButtonClick()
        {
            settingsTransformParent.gameObject.SetActive(false);
            
            foreach (Transform child in escapeMenuToggleTransforms)
            {
                child.gameObject.SetActive(true);
            }
        }
        
        private void Update()
        {
            if (!InventoryUI.showingInventory && !ConsoleInputer.showingConsole && !ItemDragHandler.Dragging && Input.GetKeyDown(KeyCode.Escape))
                SetEscapeWorkability(!showingEscapeMenu);
        }
        
        private void SetEscapeWorkability(bool state)
        {
            showingEscapeMenu = state;
            CursorVisibilityManager.Instance.ToggleMouseVisibility(showingEscapeMenu);

            foreach (IConsoleToggle toggleObject in disableOnInventoryAppear)
            {
                toggleObject.Enabled = !showingEscapeMenu;
            }

            foreach (Transform child in escapeMenuToggleTransforms)
            {
                child.gameObject.SetActive(showingEscapeMenu);
            }
            
            settingsTransformParent.gameObject.SetActive(false);
        }
    }
}
