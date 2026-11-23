using UnityEngine;

public class IslandScript : MonoBehaviour
{
    private GameObject directionalArrows;
    [HideInInspector] public bool shouldMove;
    [SerializeField] private GameObject sisterIsland;
    [SerializeField] private GameObject goalIsland;
    [HideInInspector] public Vector3 direction;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Transform directionalArrowsTransform = transform.Find("Directional Arrows");
        
        directionalArrows = directionalArrowsTransform.gameObject;
    }

    private void FixedUpdate()
    {
        if (shouldMove)
        {
            rb.MovePosition(rb.position + direction * 100f);
            directionalArrows.SetActive(false);
            shouldMove = false;
        }
    }
}
