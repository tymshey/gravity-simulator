using UnityEngine;
using UnityEngine.EventSystems;

public class MassSelector : MonoBehaviour
{
    public float handleSize = 1f;
    public Color highlightColor = Color.yellow;
    public float scaleDistanceFactor = 0.1f;
    public Material arrowMaterial;

    private Camera cam;
    private ObjectSpawnerUI ui;
    private MassObject selectedObject;
    private Color originalColor;

    private GameObject arrowX, arrowY, arrowZ;
    private enum Axis { None, X, Y, Z }
    private Axis draggingAxis = Axis.None;

    private Vector3 dragStartPos;
    private Vector3 mouseStartWorld;

    void Start()
    {
        cam = Camera.main;
        ui = FindObjectOfType<ObjectSpawnerUI>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (draggingAxis == Axis.None && selectedObject != null)
            {
                if (arrowX != null && Physics.Raycast(ray, out RaycastHit hitX) && hitX.collider.gameObject == arrowX)
                    BeginDrag(Axis.X);
                else if (arrowY != null && Physics.Raycast(ray, out RaycastHit hitY) && hitY.collider.gameObject == arrowY)
                    BeginDrag(Axis.Y);
                else if (arrowZ != null && Physics.Raycast(ray, out RaycastHit hitZ) && hitZ.collider.gameObject == arrowZ)
                    BeginDrag(Axis.Z);
                else
                    TrySelectObject(ray);
            }
            else
            {
                TrySelectObject(ray);
            }
        }

        if (Input.GetMouseButton(0) && draggingAxis != Axis.None && selectedObject != null)
        {
            Vector3 currentMouseWorld = GetMouseWorldOnAxis(draggingAxis);
            if (currentMouseWorld != Vector3.zero)
            {
                Vector3 offset = currentMouseWorld - mouseStartWorld;
                selectedObject.transform.position = dragStartPos + offset;
                UpdateArrows();
            }
        }

        if (Input.GetMouseButtonUp(0))
            draggingAxis = Axis.None;

        if (selectedObject != null)
            UpdateArrows();
    }

    private void BeginDrag(Axis axis)
    {
        draggingAxis = axis;
        dragStartPos = selectedObject.transform.position;
        mouseStartWorld = GetMouseWorldOnAxis(axis);
    }

    private void TrySelectObject(Ray ray)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var m = hit.collider.GetComponent<MassObject>();
            if (m != null)
            {
                SelectObject(m);
                return;
            }
        }

        DeselectObject();
    }

    private void DeselectObject()
    {
        if (selectedObject != null)
        {
            var rendPrev = selectedObject.GetComponent<Renderer>();
            if (rendPrev != null)
                rendPrev.material.color = originalColor;

            ClearArrows();
            selectedObject = null;

            if (ui != null)
                ui.SelectObject(null);
        }
    }

    public void SelectObject(MassObject obj)
    {
        ClearArrows();

        if (selectedObject != null)
        {
            var rendPrev = selectedObject.GetComponent<Renderer>();
            if (rendPrev != null)
                rendPrev.material.color = originalColor;
        }

        selectedObject = obj;
        if (selectedObject != null)
        {
            var rend = selectedObject.GetComponent<Renderer>();
            if (rend != null)
            {
                originalColor = rend.material.color;
                rend.material.color = highlightColor;
            }

            ui.SelectObject(obj);
            CreateArrows();
        }
    }

    private void CreateArrows()
    {
        if (selectedObject == null) return;

        arrowX = CreateArrow(Vector3.right, Color.red);
        arrowY = CreateArrow(Vector3.up, Color.green);
        arrowZ = CreateArrow(Vector3.forward, Color.blue);
    }

    private GameObject CreateArrow(Vector3 direction, Color color)
    {
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.transform.SetParent(selectedObject.transform);
        arrow.transform.localScale = Vector3.one * handleSize;
        arrow.transform.localPosition = direction * handleSize;

        var rend = arrow.GetComponent<Renderer>();
        rend.material = new Material(arrowMaterial);
        rend.material.color = color;

        arrow.GetComponent<Collider>().isTrigger = true;
        return arrow;
    }

    private void UpdateArrows()
    {
        if (selectedObject == null || cam == null) return;

        float distance = Vector3.Distance(cam.transform.position, selectedObject.transform.position);
        float scale = handleSize + distance * scaleDistanceFactor;

        if (arrowX != null)
        {
            arrowX.transform.localPosition = Vector3.right * scale;
            arrowX.transform.localScale = Vector3.one * scale * 0.2f;
        }
        if (arrowY != null)
        {
            arrowY.transform.localPosition = Vector3.up * scale;
            arrowY.transform.localScale = Vector3.one * scale * 0.2f;
        }
        if (arrowZ != null)
        {
            arrowZ.transform.localPosition = Vector3.forward * scale;
            arrowZ.transform.localScale = Vector3.one * scale * 0.2f;
        }
    }

    private Vector3 GetMouseWorldOnAxis(Axis axis)
    {
        if (selectedObject == null) return Vector3.zero;

        Vector3 pos = selectedObject.transform.position;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Plane plane = axis switch
        {
            Axis.X => new Plane(Vector3.up, pos),
            Axis.Y => new Plane(Vector3.forward, pos),
            Axis.Z => new Plane(Vector3.up, pos),
            _ => new Plane(Vector3.up, pos)
        };

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);
            switch (axis)
            {
                case Axis.X: point.y = pos.y; point.z = pos.z; break;
                case Axis.Y: point.x = pos.x; point.z = pos.z; break;
                case Axis.Z: point.x = pos.x; point.y = pos.y; break;
            }
            return point;
        }

        return Vector3.zero;
    }

    private void ClearArrows()
    {
        if (arrowX != null) Destroy(arrowX);
        if (arrowY != null) Destroy(arrowY);
        if (arrowZ != null) Destroy(arrowZ);
    }
}
