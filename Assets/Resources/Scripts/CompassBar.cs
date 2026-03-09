using UnityEngine;

public class CompassBar : MonoBehaviour
{
    public RectTransform compassBarTransform;
    public RectTransform objectiveMarkerTransform;
    public RectTransform northMarkerTransform;
    public RectTransform southMarkerTransform;
    public Transform objectiveObjectTransform;

    private PlayerMovement playerMovement; // Per accedere a playerModel

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        float playerYAngle = playerMovement.playerModel.eulerAngles.y;

        SetMarkerPosition(objectiveMarkerTransform, objectiveObjectTransform.position, playerYAngle);
        SetMarkerPosition(northMarkerTransform, playerMovement.playerModel.position + Vector3.forward * 1000, playerYAngle);
        SetMarkerPosition(southMarkerTransform, playerMovement.playerModel.position + Vector3.back * 1000, playerYAngle);
    }

    private void SetMarkerPosition(RectTransform markerTransform, Vector3 worldPosition, float playerYAngle)
    {
        Vector3 directionToTarget = worldPosition - playerMovement.playerModel.position;
        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

        // Differenza angolare normalizzata tra -180 e 180
        float angleDiff = Mathf.DeltaAngle(playerYAngle, targetAngle);

        float compassPosition = Mathf.Clamp(angleDiff / Camera.main.fieldOfView, -0.5f, 0.5f);
        markerTransform.anchoredPosition = new Vector2(compassBarTransform.rect.width * compassPosition, 0);
    }
}