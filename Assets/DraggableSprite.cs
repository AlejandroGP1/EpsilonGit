using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableSprite : MonoBehaviour
{
    private Camera mainCamera;
    private CircularLayout2D layout;
    private Transform layoutParent;
    private SpriteRenderer sr;
    private Vector3 dragOffset;
    private bool isDragging;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        sr           = GetComponent<SpriteRenderer>();
        layoutParent = transform.parent;
        layout       = layoutParent != null
                       ? layoutParent.GetComponent<CircularLayout2D>()
                       : FindObjectOfType<CircularLayout2D>();
        if (layout == null)
            layout = FindObjectOfType<CircularLayout2D>();
    }

    void Update()
{
    var mouse = Mouse.current;
    if (mouse == null) return;

    if (mouse.leftButton.wasPressedThisFrame)
    {
        Vector2 worldPos2D = MouseWorld();
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos2D, Vector2.zero);

        bool clickedMe = false;
        foreach (var h in hits)
        {
            if (h.collider.gameObject == gameObject)
            {
                clickedMe = true;
                break;
            }
        }

        if (clickedMe)
        {
            isDragging = true;
            dragOffset = transform.position - (Vector3)worldPos2D;
            sr.sortingOrder = 99;
            sr.color = new Color(1, 1, 1, 0.7f);
            Debug.Log($"Drag iniciado en {gameObject.name}");
        }
    }

    if (mouse.leftButton.isPressed && isDragging)
        transform.position = (Vector3)MouseWorld() + dragOffset;

    if (mouse.leftButton.wasReleasedThisFrame && isDragging)
    {
        isDragging = false;
        sr.color = Color.white;
        sr.sortingOrder = 2;

        if (layout != null)
        {
            transform.SetSiblingIndex(GetClosestIndex());
            layout.Reorganize();
        }
    }
}

    // ← MouseWorld() va aquí, dentro de la clase
    Vector3 MouseWorld()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 pos3D = new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(pos3D);
    }

    int GetClosestIndex()
    {
        var local   = transform.position - layout.transform.position;
        float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        int   count = layoutParent.childCount;
        float step  = 360f / count;
        float dir   = layout.clockwise ? -1f : 1f;
        int   best  = 0;
        float bestD = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            float slot = layout.startAngleDeg + dir * step * i;
            float d    = Mathf.Abs(Mathf.DeltaAngle(angle, slot));
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }
}