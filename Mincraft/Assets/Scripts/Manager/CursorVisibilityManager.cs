using Extensions;
using UnityEngine;

namespace Core.Managers
{
    public class CursorVisibilityManager : SingletonBehaviour<CursorVisibilityManager>
    {
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void ToggleMouseVisibility()
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
        
        public void ToggleMouseVisibility(bool state)
        {
            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
