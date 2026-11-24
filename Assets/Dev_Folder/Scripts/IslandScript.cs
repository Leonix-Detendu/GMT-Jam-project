using TMPro;
using UnityEngine;

public class IslandScript : MonoBehaviour
{
    private GameObject directionalArrows;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool shouldMoveBack;

    [SerializeField] private GameObject sisterIsland;
    private IslandScript sisterIslandScript;
    [SerializeField] private GameObject goalIsland;
    private IslandScript goalIslandScript;

    private bool isGoalIsland;

    [HideInInspector] public bool playerStandingHere;

    [HideInInspector] public Vector3 direction;

    [HideInInspector] public Vector3 currentPos;
    [HideInInspector] public Vector3 previousPos;
    private Vector3 targetPos;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;
    [HideInInspector] public float range;

    [SerializeField] private float rotateSpeed;

    [HideInInspector] public bool isControllable;
    [HideInInspector] public bool isBackable;
    [HideInInspector] public bool isRotatable;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Transform directionalArrowsTransform = transform.Find("Directional Arrows");
        
        directionalArrows = directionalArrowsTransform.gameObject;

        targetPos = rb.position;

        currentPos = rb.position;

        previousPos = rb.position; 

        range = 50f;

        isControllable = true;

        isRotatable = true;

        isBackable = false;

        sisterIslandScript = sisterIsland.GetComponent<IslandScript>();

        // goalIslandScript = goalIsland.GetComponent<IslandScript>();
    }

    private void FixedUpdate()
    {
        if (shouldMoveBack)
        {
            isMoving = true;
            isBackable = false;
            direction = (previousPos - currentPos).normalized;
        }

        if (isMoving)
        {
            MovingToTargetPos();
        }
    }

    private void MovingToTargetPos()
    {
        isRotatable = false;
        targetPos = currentPos + direction * range;
        rb.MovePosition(Vector3.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime));
        MakeDirArrowsDisappear();

        if ((rb.position - targetPos).sqrMagnitude <= 0.01f)
        {
            currentPos = rb.position;
            isMoving = false;
            if (shouldMoveBack)
            {
                MakeDirArrowsReappear();
            }
            shouldMoveBack = false;
            isRotatable = true;
        }
    }

    public void RotateIsland(float rotateInput)
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, - rotateInput * rotateSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerControllerScript = collision.gameObject.GetComponent<PlayerController>();
            
            if (playerControllerScript != null  && collision.GetContact(0).normal.y < -0.5f)
            {
                isControllable = false;
                sisterIslandScript.isControllable = true;

                MakeDirArrowsDisappear();
                sisterIslandScript.MakeDirArrowsReappear();
            }
        }
    }

    public void MakeDirArrowsReappear()
    {
        directionalArrows.SetActive(true);
    }

    public void MakeDirArrowsDisappear()
    {
        directionalArrows.SetActive(false);
    }
}
