using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Managers
{
    public class CursorVisibilityManager : MonoBehaviour
    {
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                ToggleMouseVisibility();
            }
        }

        public void ToggleMouseVisibility()
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
