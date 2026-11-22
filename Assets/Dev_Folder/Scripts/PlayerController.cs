using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxJumpForce;
    private float initJumpForce;

    private bool isGrounded;
    [SerializeField] private float groundCheckRadius;
    private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private bool startLoadingTakeOff;
    private bool isLoadingTakeOff;
    [SerializeField] private float takeOffChargeRate;
    [SerializeField] private float loadingTakeOffDelay;
    private bool isTakingOff;
    [SerializeField] private float takingOffDuration;
    private float holdingTime = 0f;

    private GameObject camera;


    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initJumpForce = jumpForce;
        groundCheck = transform.Find("GroundCheck");
        camera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSpace(InputValue value)
    {
        
        if (value.isPressed)
        {
            startLoadingTakeOff = true;
        }
        else
        {
            if (isGrounded) JumpOrTakeOff();
            startLoadingTakeOff = false;
            isLoadingTakeOff = false;
            jumpForce = initJumpForce;
            holdingTime = 0f;
        }

    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        Vector3 moveDirection = MoveDirectionCalculation();

        if (isLoadingTakeOff)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else if (isGrounded && !isTakingOff)
        {
            Vector3 movement = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        }
        else if (isTakingOff)
        {
            // ajouter les controles aériens pendant le décollage
            Vector3 movement = new Vector3(moveDirection.x, moveDirection.z, 0);
        }
        else
        {
            // ajouter les controles pendant la chute, le planage 
        }

        if (startLoadingTakeOff && isGrounded)
        {
            LoadingTakeOff();
        }
    }

    private void JumpOrTakeOff()
    {
        if (isLoadingTakeOff)
        {
            Vector3 takeOffDirection = camera.transform.forward.normalized;
            rb.AddForce( jumpForce * takeOffDirection, ForceMode.Impulse);
            StartCoroutine(TakingOffRoutine());
        }
        else
        {
            rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }

    private void LoadingTakeOff()
    {
        holdingTime += Time.fixedDeltaTime;

        if (holdingTime >= loadingTakeOffDelay)
        {
            isLoadingTakeOff = true;
            jumpForce += takeOffChargeRate * Time.fixedDeltaTime;
            if (jumpForce >= maxJumpForce)
            {
                jumpForce = maxJumpForce;
            }
        }
    }

    IEnumerator TakingOffRoutine()
    {
        isTakingOff = true;

        yield return new WaitForSeconds(takingOffDuration);

        isTakingOff = false;
    }

    private Vector3 MoveDirectionCalculation()
    {
        Vector3 camForward = camera.transform.forward;
        Vector3 camRight = camera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = ((camForward * moveInput.y) + (camRight * moveInput.x)).normalized;

        return moveDirection;
    }
}
