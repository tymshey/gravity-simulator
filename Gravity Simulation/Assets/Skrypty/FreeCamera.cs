using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public float boostMultiplier = 2f;
    public bool invertY = false;

    private float yaw = 0f;
    private float pitch = 0f;
    private bool cursorLocked = true;

    void Start()
    {
        LockCursor(true);

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleMouseLockToggle();

        if (cursorLocked)
            HandleLook();

        HandleMovement();
    }

    void HandleMouseLockToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            cursorLocked = !cursorLocked;
            LockCursor(cursorLocked);
        }
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * (invertY ? 1 : -1);

        yaw += mouseX;
        pitch += mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);

        Vector3 move = new Vector3(
            Input.GetAxis("Horizontal"),
            (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f),
            Input.GetAxis("Vertical")
        );

        transform.Translate(move * speed * Time.deltaTime, Space.Self);
    }
}
