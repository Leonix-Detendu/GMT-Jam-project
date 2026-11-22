using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; 

public class BowController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimCamera;
    [SerializeField] private float maxAimDistance = 150f;
    [SerializeField] private LayerMask aimLayerMask;

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float minArrowSpeed = 25f;
    [SerializeField] private float maxArrowSpeed = 70f;
    [SerializeField] private float chargeRate = 30f;

    private Camera mainCamera;
    private bool isChargingBow;
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
    }

    public void OnAim(InputValue value)
    {
        if (value.isPressed)
        {
            aimCamera.Priority = 15;
            isAiming = true;
        }

        else
        {
            isAiming = false;
            aimCamera.Priority = 5;
        }
        
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            isChargingBow = true;
            currentArrowSpeed = minArrowSpeed;
        }
        else if (isChargingBow)
        {
            ShootArrow();
            isChargingBow = false;
            currentArrowSpeed = minArrowSpeed;
        }
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
}
