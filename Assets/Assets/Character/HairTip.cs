using UnityEngine;

public class HairTip : MonoBehaviour
{
    [SerializeField] private Vector3 localHairAxis = Vector3.down;
    [SerializeField, Min(0.1f)] private float springStrength = 12f;
    [SerializeField, Range(0f, 90f)] private float maxBend = 18f;
    [SerializeField, Min(0f)] private float movementInfluence = 0.2f;
    private Vector3 previousPosition;

    private Quaternion restLocalRotation;
    private Vector3 springDirection;
    private Vector3 springVelocity;

    private void OnEnable()
    {
        restLocalRotation = transform.localRotation;
        springDirection = transform.rotation * localHairAxis.normalized;
        springVelocity = Vector3.zero;
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (transform.parent == null || localHairAxis.sqrMagnitude < 0.0001f)
            return;

        Quaternion restWorldRotation = transform.parent.rotation * restLocalRotation;
        Vector3 restDirection = restWorldRotation * localHairAxis.normalized;


        float timeStep = Time.deltaTime;
        if (timeStep <= 0f)
            return;

        Vector3 position = transform.position;
        Vector3 velocity = (position - previousPosition) / timeStep;
        previousPosition = position;

        // Use the part of the motion that actually bends this lock.
        Vector3 sidewaysMotion = Vector3.ProjectOnPlane(velocity, restDirection);
        Vector3 lean = Vector3.ClampMagnitude(sidewaysMotion * movementInfluence, 1f);
        Vector3 targetDirection = (restDirection - lean).normalized;

        float decay = Mathf.Exp(-springStrength * timeStep);
        Vector3 offset = springDirection - targetDirection;
        Vector3 step = (springVelocity + springStrength * offset) * timeStep;

        springDirection = targetDirection + (offset + step) * decay;
        springVelocity = (springVelocity - springStrength * step) * decay;
        if (springDirection.sqrMagnitude < 0.0001f)
            springDirection = restDirection;

        Vector3 bentDirection = Vector3.RotateTowards(
            restDirection, springDirection.normalized, maxBend * Mathf.Deg2Rad, 0f);

        transform.rotation =
            Quaternion.FromToRotation(restDirection, bentDirection) * restWorldRotation;
    }

    private void OnDisable()
    {
        transform.localRotation = restLocalRotation;
    }
}