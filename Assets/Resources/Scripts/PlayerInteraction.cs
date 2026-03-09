using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 2.5f;   // Distanza massima raycast
    public Transform cameraTransform;        // Trascina qui la Camera figlia
    public TextMeshProUGUI promptText;       // Testo "Premi E per raccogliere"
    public TextMeshProUGUI notificationText; // Testo "Hai raccolto X"
    public float notificationDuration = 2f;

    private float notificationTimer = 0f;

    void Update()
    {
        CheckInteraction();

        // Timer notifica
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
                notificationText.text = "";
        }
    }

    void CheckInteraction()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            PickupItem item = hit.collider.GetComponent<PickupItem>();

            if (item != null)
            {
                promptText.text = $"Premi E per raccogliere: {item.itemName}";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    ShowNotification($"Hai raccolto: {item.itemName}");
                    Destroy(item.gameObject);
                }
                return;
            }
        }

        promptText.text = ""; // Nessun oggetto nel mirino
    }

    void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationTimer = notificationDuration;
    }
}