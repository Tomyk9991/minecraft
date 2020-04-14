using UnityEngine;
using UnityInspector;

namespace Core.Player.Movement
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private float mouseSensitivity = 2f;
        
        [Header("References")] 
        [SerializeField] private Transform playerBody = null;
        
        [SerializeField, ShowOnly] private float mouseX = 0f;
        [SerializeField, ShowOnly] private float mouseY = 0f;

        private const string mouseXString = "Mouse X";
        private const string mouseYString = "Mouse Y";

        private float xRotation = 0f;
        
        private void Update()
        {
            mouseX = Input.GetAxis(mouseXString) * mouseSensitivity * 100f * Time.deltaTime;
            mouseY = Input.GetAxis(mouseYString) * mouseSensitivity * 100f * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
