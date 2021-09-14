using Core.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.MainMenu
{
    public class SettingsMenuManager : MonoBehaviour
    {
        [Header("Visibility References")] 
        [SerializeField] private GameObject settingsParent = null;
        [SerializeField] private GameObject mainMenuParent = null;
        [Header("UI Elements")]
        [SerializeField] private Slider fovSlider = null;
        [SerializeField] private Slider renderDistanceSlider = null;
        [SerializeField] private Slider mouseSensitivity = null;
        

        private void Start()
        {
            SettingsData data = MainMenuSavingManager.LoadSettings();
            
            this.fovSlider.value = data.fovSlider;
            this.renderDistanceSlider.value = data.renderDistance;
            this.mouseSensitivity.value = data.mouseSensitivity;
        }

        //Called from Unity
        public void OnBackClick()
        {
            Save();
            settingsParent.SetActive(false);
            mainMenuParent.SetActive(true);
        }

        private void Save()
        {
            MainMenuSavingManager.SaveSettings(new SettingsData(
                (int) fovSlider.value, 
                (int) renderDistanceSlider.value, 
                mouseSensitivity.value)
            );
        }
    }
}