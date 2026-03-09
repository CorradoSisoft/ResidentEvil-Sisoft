using UnityEngine;

public class BloodPoolCalibrator : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0.01f, 0f);

    void Update()
    {
        BloodPool.bloodOffset = offset;
    }
}