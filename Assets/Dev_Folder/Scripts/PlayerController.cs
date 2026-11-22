using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    [SerializeField] private float rotationSpeed;
    [SerializeField] private float aimingRotationSpeed;

    private bool isGrounded;
    [SerializeField] private float groundCheckRadius;
    private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private bool startLoadingTakeOff;
    private bool isLoadingTakeOff;
    [SerializeField] private float takeOffChargeRate;
    [SerializeField] private float loadingTakeOffDelay;
    private float holdingTime = 0f;
    private bool isFlying;

    [SerializeField] private float initFlightDuration;
    private float flightDuration;
    [SerializeField] private float flightDurationIncreaseRate;
    [SerializeField] private float maxFlightDuration;

    private Vector3 flightDirection;
    private Vector3 initFlightDirection;
    [Range(0f, 1f)]
    [SerializeField] private float maxSteerAngleFactor;


    [SerializeField] private float initFlightSpeed;
    private float flightSpeed;
    [SerializeField] private float maxFlightSpeed;
    [SerializeField] private float airTurnSpeed;

    private Camera mainCamera;

    private Rigidbody rb;

    private BowController bowControllerScript;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = transform.Find("GroundCheck");
        mainCamera = Camera.main ;
        flightSpeed = initFlightSpeed;
        flightDuration = initFlightDuration;

        bowControllerScript = GetComponent<BowController>();
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
            if (isGrounded && !isFlying)
            {
                startLoadingTakeOff = true;
            }
        }
        else
        {
            if (startLoadingTakeOff && isGrounded) JumpOrTakeOff();
            startLoadingTakeOff = false;
            isLoadingTakeOff = false;
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

        else if (!isFlying && !startLoadingTakeOff)
        {
            BasicMovements(moveDirection);
        }

        else if (isFlying)
        {
            FlyingMovements();
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
            Vector3 camForward = mainCamera.transform.forward;

            if (camForward.y < 0.25f)
            {
                camForward.y = 0.25f;
            }

            flightDirection = camForward.normalized;
            initFlightDirection = flightDirection;
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
            flightSpeed = Mathf.Min(flightSpeed + takeOffChargeRate * Time.fixedDeltaTime, maxFlightSpeed);
            flightDuration = Mathf.Min(flightDuration + flightDurationIncreaseRate * Time.fixedDeltaTime, maxFlightDuration);
        }
    }

    IEnumerator TakingOffRoutine()
    {
        isFlying = true;
        rb.useGravity = false;

        yield return new WaitForSeconds(flightDuration);

        rb.useGravity = true ;
        isFlying = false;
        flightSpeed = initFlightSpeed;
        flightDuration = initFlightDuration;
    }

    private Vector3 MoveDirectionCalculation()
    {
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = ((camForward * moveInput.y) + (camRight * moveInput.x)).normalized;

        return moveDirection;
    }

    private void BasicMovements(Vector3 moveDirection)
    {
        Vector3 movement = moveDirection * moveSpeed;

        Quaternion targetRotation;

        Quaternion nextRotation;

        if (isGrounded && !bowControllerScript.isAiming && moveDirection.sqrMagnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(moveDirection);

            nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(nextRotation);
            
        }

        else if (isGrounded && bowControllerScript.isAiming)
        {
            Vector3 camForward = mainCamera.transform.forward;

            camForward.y = 0;

            camForward.Normalize();

            if (camForward.sqrMagnitude > 0.001)
            {
                targetRotation = Quaternion.LookRotation(camForward);

                nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, aimingRotationSpeed * Time.fixedDeltaTime);

                rb.MoveRotation(nextRotation);
            }
        }

        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }

    private void FlyingMovements()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 inputDir = (mainCamera.transform.right * moveInput.x + mainCamera.transform.up * moveInput.y).normalized;

            Vector3 targetDir = (initFlightDirection + inputDir * maxSteerAngleFactor).normalized;

            flightDirection = Vector3.RotateTowards(flightDirection, targetDir, airTurnSpeed * Time.fixedDeltaTime, 0f);
        }

        rb.linearVelocity = flightSpeed * flightDirection;
    }
}
