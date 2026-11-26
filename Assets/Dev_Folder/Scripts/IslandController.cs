using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class IslandController : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject focusedIsland;
    private IslandScript focusedIslandScript;

    [SerializeField] private float maxRangeOfRay = 100f;
    [SerializeField] private LayerMask islandLayerMask;
    [HideInInspector] public bool isFocusingIsland => focusedIsland != null;
    [SerializeField] private CinemachineInputAxisController defaultCamera;

    private float rotateInput;

    private BowAndAimController bowControllerScript;

    private void Start()
    {
        mainCamera = Camera.main;
        bowControllerScript = GetComponent<BowAndAimController>();
    }

    public void OnFocusIsland(InputValue value)
    {
        if (value.isPressed && !bowControllerScript.isChargingBow)
        {
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRangeOfRay, islandLayerMask))
            {
                IslandScript hitIsland = hit.collider.GetComponentInParent<IslandScript>();

                if (hitIsland != null && hitIsland.isControllable)
                {
                    focusedIslandScript = hit.collider.GetComponentInParent<IslandScript>();
                }
                
                if( focusedIslandScript != null ) focusedIsland = focusedIslandScript.gameObject;

            }
        }
        else
        {
            focusedIsland = null;
            focusedIslandScript = null;
            rotateInput = 0f;
        }
    }

    public void OnRecallIsland(InputValue value)
    {
        if (focusedIsland != null && focusedIslandScript.isBackable)
        {
            focusedIslandScript.shouldMoveBack = true;
        }
    }

    public void OnRotateIsland(InputValue value)
    {
        rotateInput = value.Get<float>();
    }

    private void Update()
    {
        if (isFocusingIsland)
        {
            defaultCamera.enabled = false;
        }
        else
        {
            defaultCamera.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (focusedIsland != null && focusedIslandScript.isRotatable )
        {
            focusedIslandScript.RotateIsland(rotateInput);
        }
    }

}
