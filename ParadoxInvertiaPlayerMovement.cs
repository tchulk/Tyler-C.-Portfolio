using System.Collections;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Animations;

public class PlayerMovement : MonoBehaviour
{
    public float MouseSensitivity = 2.0f;
    public float pitchRange = 60.0f;

    public bool IsInUI = false;

    private bool ButtonToChangeGravityLeft;
    private bool ButtonToChangeGravityRight;
    public int GravityValueLeft = 1;
    public int GravityValueRight = 1;
   public int GravityValue;


    private float RotationSpeed = 1f;
    private bool jumpInput;
    private bool Grounded;

    private bool MoveForward;
    private bool StopMoveForward;
    private bool MoveBackward;
    private bool StopMoveBackward;
    private bool Jump;
    private bool MovingForward;
    private bool MovingBackward;
    private bool Fire;

    private float verticalDirection = -2;
    public Animator animator;

    public AudioSource GunFire;
    public AudioSource GravityChangeSound;

    private float GTimer = 1;
    

    private float RotateCameraPitch;

    private Camera FirstPersonCam;
    private CharacterController characterController;
    private ConstantForce ConstantForce;
    private Vector3 ForceDirection;
    private Rigidbody Rigidbodys;
    private CapsuleCollider TriggerCollider;
    public GameObject PlayerGun;
    public GameObject PlayerLeftHand;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        FirstPersonCam = GetComponentInChildren<Camera>();
        ConstantForce = GetComponent<ConstantForce>();
        Rigidbodys = GetComponent<Rigidbody>();
        
        TriggerCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        ForceDirection = new Vector3(0, -1000f, 0);
        ConstantForce.force = ForceDirection;
        GravityValue = 1;


    }

    // Update is called once per frame
    void Update()
    {
        // Assigning input values to variables
        jumpInput = Input.GetButtonDown("Jump");
        ButtonToChangeGravityLeft = Input.GetButtonDown("GravityLeft");
        ButtonToChangeGravityRight = Input.GetButtonDown("GravityRight");
        MoveForward = Input.GetButtonDown("Vertical");
        StopMoveForward = Input.GetButtonUp("Vertical");
        MoveBackward = Input.GetButtonDown("VerticalBack");
        StopMoveBackward = Input.GetButtonUp("VerticalBack");
        Fire = Input.GetMouseButtonDown(0);

        animator.SetBool("MovingForward", MovingForward);
        animator.SetBool("MoveingBackward", MoveBackward);
        animator.SetBool("Grounded", Grounded);
        animator.SetBool("Shoot", Fire);

        if (Jump == true) 
        {
            // reduces the gravity timer while the player is in the air. when it reaches zero, the player will fall toward the ground.
            GTimer -= Time.deltaTime;
            
        }
        if (Fire == true)
        {
            gameObject.GetComponent<PlayerFire>().Fire();
        }
        CursorLock();
        Movement();
        JumpandGravity();
        CameraMovement();
        ChangeGravity();
    }

    void CameraMovement()
    {
        
        float rotateYaw = Input.GetAxis("Mouse X") * MouseSensitivity;
        transform.Rotate(0, rotateYaw, 0);

        RotateCameraPitch += Input.GetAxis("Mouse Y") * MouseSensitivity;

        RotateCameraPitch = Mathf.Clamp(RotateCameraPitch, -pitchRange, pitchRange);
        FirstPersonCam.transform.localRotation = Quaternion.Euler(RotateCameraPitch, 0, 0);


    }

    private void OnCollisionStay(Collision other)
    {
        // checks if the player is colliding with an object tagged as "Obstacle" such as a wall or ceiling to determine if they are grounded
        Rigidbody TargetRigidbody = other.gameObject.GetComponent<Rigidbody>();

        if (other.gameObject.tag == "Obstacle") 
        {
            Grounded = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        // when the player is no longer colliding with an object tagged as "Obstacle", they are considered to be in the air and not grounded
        Grounded = false;
    }

    void Movement()
    {

        if (MoveForward & IsInUI == false)
        {
            MovingForward = true;
            ConstantForce.relativeForce = new Vector3 (0, 0, 800);
            return;
        }

        if (StopMoveForward)
        {
            MovingForward = false;
            ConstantForce.relativeForce = new Vector3(0, 0, 0);
            return;
        }

        if (MoveBackward & IsInUI == false) 
        {
            MoveBackward = true;
            ConstantForce.relativeForce = new Vector3(0, 0, -800);
            return;
        }

        if (StopMoveBackward)
        {
            MoveBackward = false;
            ConstantForce.relativeForce = new Vector3(0, 0, 0);
            return;
        }



    }

    void JumpandGravity()
    {
        if (Grounded == true)
        {

            if (jumpInput & IsInUI == false)
            {
                animator.SetBool("Jumping", true);
                ConstantForce.relativeForce = new Vector3(0, 1500, 0);
                Jump = true;
            }
            else if (GTimer <= 0)
            {
                //If the gravity timer runs out while the player is grounded, reset the jump state and make the player fall toward the ground
                animator.SetBool("Jumping", false);
                GTimer = 1;
                Jump = false;

                if (GravityValue == 2 || GravityValue == 4)
                {
                    ConstantForce.relativeForce = new Vector3(0, 0, 0);
                }
                else
                {
                    ConstantForce.relativeForce = new Vector3(0, 0, 0);
                }

            }

        }

        else if (GTimer <= 0) 
        {
            //If the gravity timer runs out while the player is in the air, reset the jump state and make the player fall toward the ground
            animator.SetBool("Jumping", false);
            GTimer = 1;
            Jump = false;

            if (GravityValue == 2 || GravityValue == 4)
            {
                ConstantForce.relativeForce = new Vector3(0, 0, 0);
            }
            else
            {
                ConstantForce.relativeForce = new Vector3(0, 0, 0);
            }

        }
    }

    // this function changes the gravity of the player to one of four directions, which are 0, 90, 180 and -90 degrees
    // it either goes in the positive direction or the negative direction depending on whether the player presses the left button or the right button.
    private void ChangeGravity()
    {
        
        if (Grounded & IsInUI == false)
        {
            Quaternion TargetRotation;
            // this allows the player to change the gravity by pressing the left button, which is bound to the "GravityLeft" input in the Input Manager and set to the Q key.
            if (ButtonToChangeGravityLeft == true)
            {
                
                GravityChangeSound.Play();

                if (GravityValueLeft == 4)
                {
                    // this sets the target rotation to 0 degrees, which is the default rotation of the player.
                    TargetRotation = Quaternion.Euler(0, 0, 0);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(0, -1000f, 0);
                    verticalDirection = -2;
                    CheckGravityValue();
                    GravityValueLeft = 1;
                    GravityValue = 4;
                    GravityValueRight = GravityValueLeft;
                    return;

                }
                if (GravityValueLeft == 3)
                {
                    // this sets the target rotation to 90 degrees, which is the right direction of the player.
                    TargetRotation = Quaternion.Euler(0, 0, 90);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(1000, 0, 0);
                    verticalDirection = 2;
                    CheckGravityValue();
                    GravityValueLeft++;
                    GravityValue = 3;
                    GravityValueRight = 2;
                    return;
                }

                if (GravityValueLeft == 2)
                {
                    // this sets the target rotation to 180 degrees, which is the upside down direction of the player.
                    TargetRotation = Quaternion.Euler(0, 0, 180);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(0, 1000f, 0);
                    verticalDirection = 2;
                    CheckGravityValue();
                    GravityValueLeft++;
                    GravityValue = 2;
                    GravityValueRight = GravityValueLeft;
                    return;
                }

                if (GravityValueLeft == 1)
                {
                    // this sets the target rotation to -90 degrees, which is the left direction of the player.
                    TargetRotation = Quaternion.Euler(0, 0, -90);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(-1000f, 0, 0);
                    verticalDirection = 2;
                    CheckGravityValue();
                    GravityValueLeft++;
                    GravityValue = 1;
                    GravityValueRight = 4;
                    return;
                }



            }
            // this allows the player to change the gravity by pressing the right button, which is bound to the "GravityRight" input in the Input Manager and set to the E key.
            else if (ButtonToChangeGravityRight == true)
            {
                GravityChangeSound.Play();

                if (GravityValueRight == 4)
                {
                    // this sets the target rotation to 0 degrees, which is the default rotation of the player.
                    TargetRotation = Quaternion.Euler(0, 0, 0);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(0, -1000f, 0);
                    verticalDirection = -2;
                    CheckGravityValue();
                    GravityValueRight = 1;
                    GravityValue = GravityValueRight;
                    GravityValueLeft = GravityValueRight;
                    return;

                }
                if (GravityValueRight == 1)
                {
                    // this sets the target rotation to 90 degrees, which is the right direction of the player.
                    TargetRotation = Quaternion.Euler(0, 0, 90);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(1000, 0, 0);
                    verticalDirection = 2;
                    CheckGravityValue();
                    GravityValueRight++;
                    GravityValue = 3;
                    GravityValueLeft = 4;
                    return;
                }

                if (GravityValueRight == 2)
                {
                    // this sets the target rotation to 180 degrees, which is the upside down direction of the player.
                    TargetRotation = Quaternion.Euler(0, 0, 180);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(0, 1000f, 0);
                    verticalDirection = 2;
                    CheckGravityValue();
                    GravityValueRight++;
                    GravityValue = GravityValueRight;
                    GravityValueLeft = GravityValueRight;
                    return;
                }

                if (GravityValueRight == 3)
                {
                    // this sets the target rotation to -90 degrees, which is the left direction of the player.
                    TargetRotation = Quaternion.Euler(0, 0, -90);
                    StartCoroutine(RotateToTarget(TargetRotation));

                    ConstantForce.force = new Vector3(-1000f, 0, 0);
                    verticalDirection = 2;
                    CheckGravityValue();
                    GravityValueRight++;
                    GravityValue = 1;
                    GravityValueLeft = 2;
                    return;
                }
            }
        }
    } 
    
   IEnumerator RotateToTarget(Quaternion TargetRotation)
    {
        // this coroutine smoothly rotates the player to the target rotation over time
        float elapsedTime = 0;
        Quaternion StartRotation = transform.rotation;

        while (elapsedTime < 1)
        {
            transform.rotation = Quaternion.Slerp(StartRotation, TargetRotation, elapsedTime);
            elapsedTime += RotationSpeed * Time.deltaTime;
            yield return null;
        }

        transform.rotation = TargetRotation;
    }

    // this function locks the cursor to the center of the screen when the player is not in the UI, and confines it to the window when the player is in the UI.
    private void CursorLock()
    {
        if (IsInUI == true)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void CheckGravityValue()
    {
        if (GravityValueLeft > 4)
        {
            GravityValueLeft = 1;
        }
        else if (GravityValueRight > 4)
        {
            GravityValueRight = 1;
        }
    }
}
