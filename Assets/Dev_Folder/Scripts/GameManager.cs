using UnityEngine;

public class GameManager : MonoBehaviour
{

    [HideInInspector] public static GameManager instance;

    public Vector3[] startingIslandsPos;
    public Vector3 startingPlayerPos;

    public Vector2[] xZGoalPos;
    private Vector2 currentXZGoalPos;

    [SerializeField] private float elevationStep;
    private float elevationValue;
    [SerializeField] private float nextGoalMoveSpeed;

    [SerializeField] private GameObject[] flyingIslands;
    private Rigidbody[] rbIslands;

    [HideInInspector] public float deathHeightThreshold;
    [SerializeField] private float deathThresholdOffset;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        rbIslands = new Rigidbody[flyingIslands.Length];

        for ( int i = 0; i < flyingIslands.Length; i++)
        {
            rbIslands[i] = flyingIslands[i].GetComponent<Rigidbody>();
            flyingIslands[i].transform.position = startingIslandsPos[i];
            flyingIslands[i].GetComponent<IslandScript>().SyncInitialPosition(startingIslandsPos[i]);
        }

        elevationValue = elevationStep;

        RecalculateDeathHeightThreshold();
    }

    public void MoveNewGoalIslandToNextPos()
    {
        foreach (GameObject island in flyingIslands)
        {
            IslandScript islandScript = island.GetComponent<IslandScript>();

            if (islandScript.isGoalIsland)
            {
                islandScript.isMoving = true;

                int indexOfNextPos = Random.Range(0, xZGoalPos.Length);

                Vector2 nextXZGoalPos = xZGoalPos[indexOfNextPos];

                if (xZGoalPos.Length > 1)
                {
                    while (nextXZGoalPos == currentXZGoalPos)
                    {
                        indexOfNextPos = Random.Range(0, xZGoalPos.Length);
                        nextXZGoalPos = xZGoalPos[indexOfNextPos];
                    }
                }

                Vector3 nextGoalPos = new Vector3(nextXZGoalPos.x, 0f, nextXZGoalPos.y) + Vector3.up * elevationValue;

                islandScript.direction = (nextGoalPos - islandScript.currentPos).normalized;
                islandScript.range = (nextGoalPos - islandScript.currentPos).magnitude;
                islandScript.moveSpeed = nextGoalMoveSpeed;


                elevationValue += elevationStep;
                currentXZGoalPos = nextXZGoalPos;
            }
        }
    }

    public void RecalculateDeathHeightThreshold()
    {
        float leastHighIslandValue = Mathf.Infinity;
        foreach (GameObject island in flyingIslands)
        {
            if (island.transform.position.y < leastHighIslandValue)
            {
                leastHighIslandValue = island.transform.position.y;
            }
        }

        deathHeightThreshold = leastHighIslandValue - deathThresholdOffset;
    }

    public void Respawn(GameObject player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();

        foreach (GameObject island in flyingIslands)
        {
            IslandScript islandScript = island.GetComponent<IslandScript>();
            
            if (islandScript.lastIslandWherePWasStanding)
            {
                if (playerRb != null)
                {
                    playerRb.MovePosition(islandScript.respawnPoint.transform.position);
                }
            }
        }

    }

}
