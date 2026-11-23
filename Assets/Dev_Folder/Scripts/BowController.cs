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
    private bool isChargingBow;

    private Camera mainCamera;
    private float currentArrowSpeed;

    [HideInInspector] public bool isAiming;

    void Start()
    {
        mainCamera = Camera.main;
        currentArrowSpeed = minArrowSpeed;
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

    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            if (currentSwitchingCamRoutine != null)
            {
                StopCoroutine(currentSwitchingCamRoutine);
            }
            aimCamera.Priority = 15;
            isChargingBow = true;
            isAiming = true;
            currentArrowSpeed = minArrowSpeed;
        }
        else
        {
            isChargingBow = false;
            currentSwitchingCamRoutine = StartCoroutine(SwitchingCamRoutine());
            ShootArrow();
            currentArrowSpeed = minArrowSpeed;
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void ShootArrow()
    {
        // 1. Raycast depuis le centre exact de l'écran (le crosshair)
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
        Rigidbody arrowRb = arrow.GetComponentInChildren<Rigidbody>();

        if (arrowRb != null)
        {
            arrowRb.linearVelocity = shootDirection * currentArrowSpeed;
        }

        // On détruit la flèche après 5 secondes si elle ne touche rien
        Destroy(arrow, 5f);
    }

    IEnumerator SwitchingCamRoutine()
    {
        yield return new WaitForSeconds(delayBeforeSwitchingCam);

        aimCamera.Priority = 5;

        isAiming = false;
    }
}
