using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class SCR_PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Transform camTarget;
    private Camera sceneCamera;

    private bool isCursorLocked = false;

    private GameObject UI_playPanel;
    private GameObject spawnPoints;

    #region Movement Variables
    [SerializeField]
    private float walkSpeed; //Change these in animator blend tree to match;
    [SerializeField]
    private float runSpeed; //Change these in animator blend tree to match;
    [SerializeField]
    private Transform targetAim;
    [SerializeField]
    private bool isAiming;
    [SerializeField]
    private bool aimStarted;
    private float gravity;
    private float jumpHeight = 3f;
    private float jumpTime = 8f;
    private float initJumpVelocity;
    private float verticalMovement;
    private float JumpCoefficient = 4f;
    private bool isJumping =false;
    private bool isJumpPressed = false; //get this from input controller
    private bool wasKilled = false;
    private bool isGrounded = true;

    [SerializeField]
    Behaviour[] compsToDisable;

    private float currentCamRotX;
    private bool isRunning;

    private Vector3 currentMovement;
    private float currentSpeed;
    private float deltaSpeed = 0.1f;

    private PlayerControls playerControls;
    private CharacterController characterController;
    private Vector3 playerMovement;
    private bool isMovementPressed;

    #endregion
    #region Animation
    [SerializeField] private Animator animator;
    [SerializeField] private Rig aimRig;
    [SerializeField] private Transform rigTarget;
    [SerializeField] private float aimTransitionTime =0.1f;
    private float layerWeightVelocityTrue;
    private float layerWeightVelocityFalse;
    private float weightValue;
    #endregion

    private void Awake()
    {
        playerControls = new PlayerControls();
        characterController = GetComponent<CharacterController>();
        SetupJumpParams();

        playerControls.CharacterControls.Move.started += OnMovementInput;
        playerControls.CharacterControls.Move.canceled += OnMovementInput;
        playerControls.CharacterControls.Move.performed += OnMovementInput;

        playerControls.CharacterControls.Run.started += OnRunInput;
        playerControls.CharacterControls.Run.canceled += OnRunInput;

        playerControls.CharacterControls.Jump.started += OnJumpInput;
        playerControls.CharacterControls.Jump.canceled += OnJumpInput;

        playerControls.CharacterControls.ToggleCursor.started += OnCursorInput;
        playerControls.CharacterControls.ToggleCursor.canceled += OnCursorInput;

        playerControls.CharacterControls.Dance.started += context => 
        {
            animator.SetBool("isDancing", context.ReadValueAsButton());
        };
        playerControls.CharacterControls.Dance.canceled += context => { animator.SetBool("isDancing", context.ReadValueAsButton()); };

        playerControls.CharacterControls._DEBUG_KILL.started += OnDebugK;
        playerControls.CharacterControls._DEBUG_KILL.canceled += OnDebugK;

        playerControls.CharacterControls.Aim.started += OnTargetting;
        playerControls.CharacterControls.Aim.performed += OnTargetting;
        playerControls.CharacterControls.Aim.canceled += OnTargetting;

    }



    // Start is called before the first frame update
    void Start()
    {
        UI_playPanel=GameObject.Find("PlayMenu");
        spawnPoints=GameObject.Find("SpawnPoints");

        if(!IsOwner)
        {
            for(int i=0;i<compsToDisable.Length;i++)
            {
                compsToDisable[i].enabled=false;
            }
        } 
        else
        {
            sceneCamera=Camera.main;
            if(sceneCamera!=null)
                sceneCamera.gameObject.SetActive(false);
            isCursorLocked = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
        //Spawn in one of the spawn points.
        this.transform.position=spawnPoints.transform.GetChild(Random.Range(0,spawnPoints.transform.childCount-1)).transform.position;
    }

    private void OnEnable()
    {
        playerControls.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.CharacterControls.Disable();
        if(sceneCamera!=null)
        {
            sceneCamera.gameObject.SetActive(true);
        }

        UI_playPanel.SetActive(true);
        UI_playPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (isRunning)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + deltaSpeed, 0, runSpeed);
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed + deltaSpeed, 0, walkSpeed);
        }
        HandleRotation();
        HandleAnimation();
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dance"))
        {
            currentMovement = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * playerMovement * Time.deltaTime * currentSpeed;
            currentMovement.y = verticalMovement;
            characterController.Move(currentMovement);
            if (isMovementPressed)
            {
                if (isRunning)
                    deltaSpeed = 0.1f;
                else
                {
                    if (currentSpeed >= walkSpeed)
                        deltaSpeed = -0.1f;
                    else
                        deltaSpeed = 0.1f;
                }
            }
            else
            {
                deltaSpeed = -0.1f;
            }
        }
        isGrounded = characterController.isGrounded;
        HandleGravity();
        HandleJump();
        
    }

    private void LateUpdate()
    {
        cam.transform.position = camTarget.position;
        rigTarget.position = gameObject.transform.position + cam.transform.forward * 10;
    }

    #region InputHandling
    private void OnTargetting(InputAction.CallbackContext context)
    {
        //targetAim
        if(context.canceled)
        {
            isAiming = false;
            aimStarted = false;
        }
        if (context.started || context.performed)
        {
            isAiming = true;
        }
    }
    private void OnMovementInput(InputAction.CallbackContext context)
    {

        playerMovement.x = context.ReadValue<Vector2>().x;
        // playerMovement.y = currentMovement.y;
        playerMovement.z = context.ReadValue<Vector2>().y;
        isMovementPressed = playerMovement.x != 0 || playerMovement.z != 0;
 
    }

    private void OnRunInput(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void OnDebugK(InputAction.CallbackContext context)
    {
        wasKilled = context.ReadValueAsButton();
    }

    private void OnCursorInput(InputAction.CallbackContext obj)
    {
        // Check if mouse should be locked/unlocked.
        if (obj.ReadValueAsButton())
        {
            isCursorLocked = !isCursorLocked;
            if (isCursorLocked)
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            else
                UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnJumpInput(InputAction.CallbackContext obj)
    {
        isJumpPressed = obj.ReadValueAsButton();
    }

    #endregion
    private void HandleGravity()
    {
        if(isGrounded)
        {
            verticalMovement = -0.1f * Time.deltaTime * JumpCoefficient;
        }
        else
        {
            verticalMovement =(2*verticalMovement + gravity *Time.deltaTime)/2;
        }
    }

    private void HandleAnimation()
    {
        if (wasKilled)
        {
            animator.SetTrigger("wasKilled");
        }


        if (isAiming)
        {
            //play aiming animation
            if (aimStarted)
            {
                animator.Play("Aiming", 1, 0f);
            }
            weightValue=Mathf.SmoothDamp(weightValue, 1f, ref layerWeightVelocityTrue, aimTransitionTime);
            animator.SetLayerWeight(1, weightValue);
            aimRig.weight = weightValue;
        }
        else
        {
            //release arrow
            weightValue = Mathf.SmoothDamp(weightValue, 0f, ref layerWeightVelocityFalse, aimTransitionTime);
            animator.SetLayerWeight(1, weightValue);
            aimRig.weight = weightValue;
        }

        if (isAiming == false && aimStarted == false)
        {
            aimStarted = true;
        }
        else if (isAiming == true && aimStarted == true)
        {
            aimStarted = false;
        }

        animator.SetFloat("Horizontal",playerMovement.x*currentSpeed);
        animator.SetFloat("Vertical",playerMovement.z * currentSpeed);
        animator.SetBool("Grounded", isGrounded);
        if (isJumpPressed && isGrounded && !isJumping)
        {
            animator.SetBool("Jump", true);
        }
        else if(!isJumpPressed && isGrounded && isJumping)
        {
            animator.SetBool("Jump", false);
            animator.SetBool("Freefall", true);
        }
        else
        {
            animator.SetBool("Jump", false);
            animator.SetBool("Freefall", false);
        }
    }

    private void HandleRotation()
    {
        Vector3 rotatDisplacement = new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);

        transform.Rotate(Vector3.up * rotatDisplacement.y);

        if (cam != null && rotatDisplacement.x != 0)
        {
            currentCamRotX -= rotatDisplacement.x;
            currentCamRotX = Mathf.Clamp(currentCamRotX, -85, 85);
            cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0, 0);
        }
    }

    private void HandleJump()
    {
        if (isJumpPressed && isGrounded && !isJumping)
        {
            Debug.Log("Jumping");
            isJumping = true;
            verticalMovement = initJumpVelocity * Time.deltaTime *JumpCoefficient;
        }
        else if (!isJumpPressed && isGrounded && isJumping)
        {
            Debug.Log("Setting isJumping to false;");
            isJumping = false;
        }
    }

    private void SetupJumpParams()
    {
        float apexTime = jumpTime / 2;
        gravity = (-2 * jumpHeight) / Mathf.Pow(apexTime,2);
        initJumpVelocity = (2 * jumpHeight) / apexTime;
    }
}
