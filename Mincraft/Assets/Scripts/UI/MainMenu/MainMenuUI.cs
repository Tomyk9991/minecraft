using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Menu
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button optionsButton = null;
        [SerializeField] private Button exitButton = null;

        [Header("Dialogs")]
        [SerializeField] private 
            PlayDialogUI playDialogUI = null;
        [SerializeField] private OptionsDialogUI optionsDialogUI = null;

        private bool playDialogVisibility = false;
        private bool optionsDialogVisibility = false;

        private void Start()
        {
            playButton.onClick.AddListener(() =>
            {
                //Open "Create a new world or load maps" Dialog
                playDialogUI.SetChildsVisibility(!playDialogVisibility);
            });

            optionsButton.onClick.AddListener(() =>
            {
                //Open "options" Dialog
                optionsDialogUI.SetChildsVisibility(!optionsDialogVisibility);
            });

            exitButton.onClick.AddListener(Application.Quit);
        }
    }
}
