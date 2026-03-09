using UnityEngine;

public class RotateY : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f; // gradi al secondo

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}