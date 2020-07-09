using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

using Core.UI.Console;
using Utilities;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour, IConsoleToggle
{
    [Header("General information")]
    [SerializeField] private bool m_IsWalking;
    [Header("Movement settings")]
    [SerializeField] private float m_WalkSpeed = 5;
    [SerializeField] private float m_RunSpeed = 10;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten = 0;
    [SerializeField] private float m_JumpSpeed = 10;
    [SerializeField] private float m_StickToGroundForce = 10;
    [Header("Gravity")]
    [SerializeField] private float m_GravityMultiplier;
    [Header("Mouse")]
    [SerializeField] private MouseLook m_MouseLook = null;
    [SerializeField] private bool canMoveMouse = true;
    [Header("FOV")]
    [SerializeField] private bool m_UseFovKick = true;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [Header("Head Bob")]
    [SerializeField] private bool m_UseHeadBob = false;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval = 5;


    [Header("Sound")]
    [SerializeField]
    private AudioClip[] m_FootstepSounds = null; // an array of footstep sounds that will be randomly selected from.

    [SerializeField] private AudioClip m_JumpSound = null; // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound = null; // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;
    private bool useGravity = false;
    private Vector3 _desiredMove;
    private RaycastHit _hitInfo;

    private DoubleKeypressChecker doubleKeypressChecker;

    public bool Enabled
    {
        get => this.enabled;
        set => this.enabled = value;
    }

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);
        
        doubleKeypressChecker = new DoubleKeypressChecker(KeyCode.Space);
    }


    // Update is called once per frame
    private void Update()
    {
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        _desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out _hitInfo,
            m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        _desiredMove = Vector3.ProjectOnPlane(_desiredMove, _hitInfo.normal).normalized;

        m_MoveDir.x = _desiredMove.x * speed;
        m_MoveDir.z = _desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * (m_GravityMultiplier * Time.deltaTime);
        }

        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.deltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);


        RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump && m_CharacterController.isGrounded)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
            
            useGravity = true;
            m_GravityMultiplier = useGravity ? 3 : 0;
            doubleKeypressChecker.ForceReset();
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        ControlInput();
    }
    
    private void ControlInput()
    {
        if (doubleKeypressChecker.Check())
        {
            m_MoveDir.y = 0;
            useGravity = !useGravity;
            m_GravityMultiplier = useGravity ? 3 : 0;
        }


        if (useGravity) return;
        
        if (Input.GetKey(KeyCode.Space))
            m_CollisionFlags = m_CharacterController.Move(Vector3.up * (m_IsWalking ? 10 : 50) * Time.deltaTime);
        else if (Input.GetKey(KeyCode.LeftControl))
            m_CollisionFlags = m_CharacterController.Move(Vector3.down * (m_IsWalking ? 10 : 50) * Time.deltaTime);
    }
    
    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }



    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }


    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude +
                            (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                           Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }

        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                    (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }

        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }


    private void RotateView()
    {
        if (canMoveMouse)
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
