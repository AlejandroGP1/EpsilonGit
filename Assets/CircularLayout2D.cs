using UnityEngine;

public class CircularLayout2D : MonoBehaviour
{
    public float radius = 3.5f;
    public float startAngleDeg = 90f;
    public bool clockwise = true;

    void Start() => Reorganize();

    public void Reorganize()
    {
        Debug.Log("FUNCIONA PORFAVOR");
        int count = transform.childCount;
        if (count == 0) return;

        float angleStep = 360f / count;
        float direction = clockwise ? -1f : 1f;

        for (int i = 0; i < count; i++)
        {
            float angleDeg = startAngleDeg + direction * angleStep * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius,
                0f
            );
            transform.GetChild(i).position = transform.position + offset;
        }
    }
}