// CameraController.cs — su Main Camera standalone
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Posizione")]
    public float height       = 1.6f;
    public float distance     = 4.5f;
    public float followSmooth = 8f;

    [Header("Rotazione")]
    public float sensitivity = 3f;
    public float pitchMin    = -15f;
    public float pitchMax    = 50f;

    private float yaw   = 0f;
    private float pitch = 15f;

    void Start()
    {
        yaw = player.eulerAngles.y;
        ApplyTransform();
    }

    void LateUpdate()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw   += Input.GetAxisRaw("Mouse X") * sensitivity;
            pitch -= Input.GetAxisRaw("Mouse Y") * sensitivity;
            pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        ApplyTransform();
    }

    void ApplyTransform()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot  = player.position + Vector3.up * height;
        Vector3 offset = rot * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.Lerp(
            transform.position,
            pivot + offset,
            followSmooth * Time.deltaTime
        );
        transform.rotation = rot;
    }
}