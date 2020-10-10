using System;
using System.Diagnostics.PerformanceData;
using Attributes;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private bool rotateBodyWithCamera = true;
        [DrawIfTrue(nameof(rotateBodyWithCamera)), SerializeField] 
        private Transform cameraTransform = null;
        
        
        [Header("Movement properties")] 
        [SerializeField] private float movementSpeed = 100.0f;
        [SerializeField] private float maxVelocity = 10.0f;
        

        private float x, z; //Input 
        private Vector3 move;
        private Rigidbody rb;

        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        private void Update()
        {
            //Animation
            if (rotateBodyWithCamera)
                transform.localRotation = Quaternion.Euler(0f, cameraTransform.rotation.eulerAngles.y, 0f);
        }

        private void FixedUpdate()
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            move = (transform.right * x + transform.forward * z) * movementSpeed;
            rb.AddForce(move);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        }
    }
}
