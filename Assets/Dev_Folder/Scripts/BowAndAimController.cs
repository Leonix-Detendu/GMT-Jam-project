using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;
using System.Runtime.CompilerServices;

public class BowAndAimController : MonoBehaviour
{
    public CinemachineCamera aimCamera;
    [SerializeField] private Transform aimCameraTarget;

    public CinemachineCamera standartCamera;
    private CinemachineOrbitalFollow orbitStandartCamera;

    public CinemachineCamera flyingCamera;

    [SerializeField] private float maxAimDistance = 150f;
    [SerializeField] private LayerMask aimLayerMask;

    [SerializeField] private float aimSensitivity = 0.15f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float pitchResetSpeed = 5f;
    [SerializeField] private float delayBeforeSwitchingCam;
    [HideInInspector] public Coroutine currentSwitchingCamRoutine;

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
    [HideInInspector] public bool isChargingBow;
    private bool anArrowCanBeLoaded;

    private Camera mainCamera;
    private float currentArrowSpeed;

    [HideInInspector] public bool isAiming;

    [SerializeField] private IslandController islandControllerScript;


    void Start()
    {
        orbitStandartCamera = standartCamera.GetComponent<CinemachineOrbitalFollow>();
        mainCamera = Camera.main;
        currentArrowSpeed = minArrowSpeed;
        anArrowCanBeLoaded = true;
        islandControllerScript = GetComponent<IslandController>();
    }

    private void Update()
    {
        if (isChargingBow)
        {
            currentArrowSpeed = Mathf.Min(currentArrowSpeed + chargeRate * Time.deltaTime, maxArrowSpeed);
        }

        if (isAiming)
        {
            if (!islandControllerScript.isFocusingIsland)
            {
                currentPitch -= lookInput.y * aimSensitivity;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
                aimCameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

                horizontalTurnAmount += lookInput.x * aimSensitivity;
            }
        }

        else
        {
            horizontalTurnAmount = 0f;
            currentPitch = Mathf.Lerp(currentPitch, 0f, pitchResetSpeed * Time.deltaTime);
            if (Mathf.Abs(currentPitch) < 0.05f) currentPitch = 0f;
            aimCameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
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
            if (!islandControllerScript.isFocusingIsland)
            {

                Aiming();

                if (anArrowCanBeLoaded)
                {
                    anArrowCanBeLoaded = false;
                    StartCoroutine(canReleaseArrowRoutine());
                    isChargingBow = true;
                }

                currentArrowSpeed = minArrowSpeed;
            }
        }

        else
        {
            currentSwitchingCamRoutine = StartCoroutine(SwitchingCamRoutine(false));
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

        Vector3 shootDirection = CursorDirection();
        
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(shootDirection));
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();

        if (arrowRb != null)
        {
            arrowRb.linearVelocity = shootDirection * currentArrowSpeed;
        }

        Destroy(arrow, 5f);
    }

    public IEnumerator SwitchingCamRoutine(bool isTakingOff)
    {

        float baseDelay = delayBeforeSwitchingCam;

        if (isTakingOff)
        {
            delayBeforeSwitchingCam = 0f;
        }

        yield return new WaitForSeconds(delayBeforeSwitchingCam);
        yield return new WaitWhile(() => islandControllerScript.isFocusingIsland);

        orbitStandartCamera.HorizontalAxis.Value = gameObject.transform.eulerAngles.y;
        
        float ratio;
        float relativePitch;

        if (currentPitch <= 0f)
        {
            ratio = Mathf.InverseLerp(minPitch, 0f, currentPitch);
            relativePitch = Mathf.Lerp(orbitStandartCamera.VerticalAxis.Range.x, 0f, ratio);
        }

        else
        {
            ratio = Mathf.InverseLerp(0f, maxPitch, currentPitch);
            relativePitch = Mathf.Lerp(orbitStandartCamera.VerticalAxis.Center, orbitStandartCamera.VerticalAxis.Range.y, ratio);
        }

        orbitStandartCamera.VerticalAxis.Value = relativePitch;

        standartCamera.PreviousStateIsValid = false;

        aimCamera.Priority = 5;

        CinemachineBrain mainCameraBrain = mainCamera.GetComponent<CinemachineBrain>(); 

        yield return new WaitForSeconds(mainCameraBrain.DefaultBlend.BlendTime);

        isAiming = false;

        if (isTakingOff)
        {
            flyingCamera.Priority = 15;
            delayBeforeSwitchingCam = baseDelay;
        }
    }

    IEnumerator canReleaseArrowRoutine()
    {
        yield return new WaitForSeconds(releaseArrowMinDelay);

        canReleaseArrow = true;
    }

    private Vector3 GetCursorTargetPoint()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayerMask))
        {
            return hit.point;
        }
        return ray.GetPoint(maxAimDistance);
    }

    private Vector3 CursorDirection()
    {
        return (GetCursorTargetPoint() - arrowSpawnPoint.position).normalized;
    }

    public void Aiming()
    {
        if (currentSwitchingCamRoutine != null)
        {
            StopCoroutine(currentSwitchingCamRoutine);
        }

        if (!isAiming)
        {
            Vector3 targetPoint = GetCursorTargetPoint();

            Vector3 playerToTarget = targetPoint - transform.position;
            Vector3 flatPlayerToTarget = new Vector3(playerToTarget.x, 0f, playerToTarget.z);
            float flatDistance = flatPlayerToTarget.magnitude;

            if (flatDistance > 0.001f)
            {
                float offsetX = aimCameraTarget.localPosition.x;
                float parallaxAngle = 0f;

                if (flatDistance > Mathf.Abs(offsetX))
                {
                    parallaxAngle = -Mathf.Asin(offsetX / flatDistance) * Mathf.Rad2Deg;
                }

                Quaternion targetYaw = Quaternion.LookRotation(flatPlayerToTarget) * Quaternion.Euler(0f, parallaxAngle, 0f);
                Rigidbody playerRb = GetComponent<Rigidbody>();
                playerRb.MoveRotation(targetYaw);
                transform.rotation = targetYaw;
            }

            Vector3 camToTarget = (targetPoint - aimCameraTarget.position).normalized;
            float targetPitch = -Mathf.Asin(Mathf.Clamp(camToTarget.y, -1f, 1f)) * Mathf.Rad2Deg;
            currentPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            aimCameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }

        aimCamera.Priority = 15;

        isAiming = true;
    }
}
