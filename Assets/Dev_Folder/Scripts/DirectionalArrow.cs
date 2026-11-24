using UnityEngine;

public class DirectionalArrow : MonoBehaviour
{
    private IslandScript islandScript;
    [HideInInspector] public Vector3 direction;

    private void Start()
    {
        islandScript = GetComponentInParent<IslandScript>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Arrow"))
        {
            islandScript.previousPos = islandScript.currentPos;
            islandScript.isBackable = true;
            islandScript.isMoving = true;
            islandScript.direction = transform.up;
            Destroy(collision.gameObject);
        }
    }
}
