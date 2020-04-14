using UnityEngine;
using UnityInspector;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Rigidbody rb = null;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask = 0;

    [Header("Options")] 
    [SerializeField] private float speed = 12f;
    [SerializeField] private float jumpHeight = 1f;
    

    [SerializeField, ShowOnly] private float horizontal = 0f;
    [SerializeField, ShowOnly] private float vertical = 0f;
    [SerializeField, ShowOnly] private bool isGrounded = false;

    private Vector3 velocity;
    
    private const string horizontalString = "Horizontal";
    private const string verticalString = "Vertical";

    private const float GRAVITIY = -9.81f * 2f;
    
    private void LateUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

//        if (isGrounded && velocity.y < 0)
//        {
//            velocity.y = -2f;
//        }
        
        horizontal = Input.GetAxis(horizontalString);
        vertical = Input.GetAxis(verticalString);
        velocity = transform.right * horizontal + transform.forward * vertical;
        
        
        //rb.MovePosition(velocity * speed * deltaTime);

//        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
//        {
//            velocity.y = Mathf.Sqrt(jumpHeight * -2f * GRAVITIY);
//        }
    }

    private void FixedUpdate()
        => rb.velocity = velocity.normalized * speed * Time.fixedDeltaTime;
}
