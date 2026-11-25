using TMPro;
using UnityEngine;

public class IslandScript : MonoBehaviour
{
    private GameObject directionalArrows;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool shouldMoveBack;

    [SerializeField] private GameObject[] flyingIslands;

    [SerializeField] private bool startingIsland;

    [SerializeField] private IslandScript sisterIslandScript;

    public bool isGoalIsland;

    [HideInInspector] public bool lastIslandWherePWasStanding;

    [HideInInspector] public Vector3 direction;

    [HideInInspector] public Vector3 currentPos;
    [HideInInspector] public Vector3 previousPos;
    [HideInInspector] public Vector3 targetPos;

    [SerializeField] private float baseMoveSpeed;
    [HideInInspector] public float moveSpeed;

    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    [SerializeField] private float baseRange;
    [HideInInspector] public float range;

    [SerializeField] private float rotateSpeed;

    [HideInInspector] public bool isControllable;
    [HideInInspector] public bool isBackable;
    [HideInInspector] public bool isRotatable;

    public GameObject respawnPoint;

    [HideInInspector] public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Transform directionalArrowsTransform = transform.Find("Directional Arrows");
        
        directionalArrows = directionalArrowsTransform.gameObject;

        moveSpeed = baseMoveSpeed;

        range = baseRange;

        isControllable = true;

        isRotatable = true;

        isBackable = false;

        foreach(GameObject island in flyingIslands)
        {
            IslandScript islandScript = island.GetComponent<IslandScript>();
            if (islandScript != null)
            {
                if (!islandScript.isGoalIsland && islandScript != this)
                {
                    sisterIslandScript = islandScript;
                    break;
                }
            }
        }

        for (int i = 0; i < flyingIslands.Length; i++)
        {
            if (flyingIslands[i] == gameObject)
            {
                rb.MovePosition(GameManager.instance.startingIslandsPos[i]);
            }
        }

        if (isGoalIsland)
        {
            isControllable = false;
            sisterIslandScript = null;
            MakeDirArrowsDisappear();
        }

        if (startingIsland)
        {
            MakeDirArrowsDisappear();
            isControllable = false;
            lastIslandWherePWasStanding = true;
        }
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

            moveSpeed = baseMoveSpeed;
            range = baseRange;
            GameManager.instance.RecalculateDeathHeightThreshold();
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

                if (isGoalIsland)
                {
                    NewGoalSetup();
                }

                lastIslandWherePWasStanding = true;
                sisterIslandScript.lastIslandWherePWasStanding = false;

                isControllable = false;
                sisterIslandScript.isControllable = true;

                MakeDirArrowsDisappear();
                sisterIslandScript.MakeDirArrowsReappear();

                currentPos = rb.position; 
                previousPos = rb.position; 
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

    private void NewGoalSetup()
    {
        IslandScript jumpedFrom = null;
        IslandScript newGoal = null;

        foreach (GameObject island in flyingIslands)
        {
            IslandScript script = island.GetComponent<IslandScript>();
            if (script == this) continue;

            if (script.lastIslandWherePWasStanding)
                jumpedFrom = script;
            else
                newGoal = script;
        }

        isGoalIsland = false;
        sisterIslandScript = jumpedFrom;

        jumpedFrom.sisterIslandScript = this;

        newGoal.isGoalIsland = true;
        newGoal.isControllable = false;
        newGoal.lastIslandWherePWasStanding = false;
        newGoal.sisterIslandScript = null;
        newGoal.MakeDirArrowsDisappear();

        GameManager.instance.MoveNewGoalIslandToNextPos();
    }

    public void SyncInitialPosition(Vector3 startPos)
    {
        currentPos = startPos;
        previousPos = startPos;
        targetPos = startPos;
    }

}
