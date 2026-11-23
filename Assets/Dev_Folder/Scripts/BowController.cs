using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;

public class BowController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimCamera;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float maxAimDistance = 150f;
    [SerializeField] private LayerMask aimLayerMask;

    [SerializeField] private float aimSensitivity = 0.15f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float pitchResetSpeed = 5f;
    [SerializeField] private float delayBeforeSwitchingCam;
    private Coroutine currentSwitchingCamRoutine;

    private Vector2 lookInput;
    private float currentPitch;
    [HideInInspector] public float horizontalTurnAmount;

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float minArrowSpeed = 25f;
    [SerializeField] private float maxArrowSpeed = 70f;
    [SerializeField] private float chargeRate = 30f;
    [SerializeField] private float releaseArrowMinDelay;
    private bool canReleaseArrow;
    private bool shouldReleaseArrow;
    private bool isChargingBow;
    private bool anArrowCanBeLoaded;

    private Camera mainCamera;
    private float currentArrowSpeed;

    [HideInInspector] public bool isAiming;

    void Start()
    {
        mainCamera = Camera.main;
        currentArrowSpeed = minArrowSpeed;
        anArrowCanBeLoaded = true;
    }

    private void Update()
    {
        if (isChargingBow)
        {
            currentArrowSpeed = Mathf.Min(currentArrowSpeed + chargeRate * Time.deltaTime, maxArrowSpeed);
        }

        if (isAiming)
        {
            currentPitch -= lookInput.y * aimSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            cameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

            horizontalTurnAmount += lookInput.x * aimSensitivity;
        }

        else
        {
            horizontalTurnAmount = 0f;
            currentPitch = Mathf.Lerp(currentPitch, 0f, pitchResetSpeed * Time.deltaTime);
            if (Mathf.Abs(currentPitch) < 0.05f) currentPitch = 0f;
            cameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (canReleaseArrow && shouldReleaseArrow)
        {
            ShootArrow();
            shouldReleaseArrow = false;
            canReleaseArrow = false;
            isChargingBow = false;
            anArrowCanBeLoaded = true;
            currentArrowSpeed = minArrowSpeed;
        }
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            if (currentSwitchingCamRoutine != null)
            {
                StopCoroutine(currentSwitchingCamRoutine);
            }
            aimCamera.Priority = 15;
            if (anArrowCanBeLoaded)
            {
                anArrowCanBeLoaded = false;
                StartCoroutine(canReleaseArrowRoutine());
                isChargingBow = true;
            }
            isAiming = true;
            currentArrowSpeed = minArrowSpeed;
        }
        else
        {
            currentSwitchingCamRoutine = StartCoroutine(SwitchingCamRoutine());
            shouldReleaseArrow = true;
            isChargingBow = false;
        }
    }
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void ShootArrow()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }

        Vector3 shootDirection = (targetPoint - arrowSpawnPoint.position).normalized;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(shootDirection));
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();

        if (arrowRb != null)
        {
            arrowRb.linearVelocity = shootDirection * currentArrowSpeed;
        }

        Destroy(arrow, 5f);
    }

    IEnumerator SwitchingCamRoutine()
    {
        yield return new WaitForSeconds(delayBeforeSwitchingCam);

        aimCamera.Priority = 5;

        isAiming = false;
    }

    IEnumerator canReleaseArrowRoutine()
    {
        yield return new WaitForSeconds(releaseArrowMinDelay);

        canReleaseArrow = true;
    }
}
