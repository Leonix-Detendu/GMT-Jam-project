using UnityEngine;

[ExecuteAlways]
public class BowDrawControl : MonoBehaviour
{
    [SerializeField] private Transform stringTarget;

    [SerializeField, Range(0f, 1f)]
    private float drawAmount;

    [SerializeField] private Vector3 restLocalPosition;
    [SerializeField] private Vector3 maxDrawLocalPosition;

    private void Update()
    {
        ApplyDraw();
    }

    private void OnDidApplyAnimationProperties()
    {
        ApplyDraw();
    }

    private void ApplyDraw()
    {
        if (stringTarget == null)
            return;

        Vector3 position = Vector3.Lerp(
            restLocalPosition,
            maxDrawLocalPosition,
            drawAmount
        );

        if (stringTarget.localPosition != position)
            stringTarget.localPosition = position;
    }
}