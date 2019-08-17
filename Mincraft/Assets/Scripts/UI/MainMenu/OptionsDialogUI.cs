using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Menu
{
    public class OptionsDialogUI : MonoBehaviour
    {
        [SerializeField] private Button backButton = null;
        [SerializeField] private RectTransform[] transforms = null;

        private void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                SetChildsVisibility(false);
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
    }
}
