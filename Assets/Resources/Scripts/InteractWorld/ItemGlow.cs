using UnityEngine;

public class ItemGlow : MonoBehaviour
{
    Transform cameraTransform;
    SpriteRenderer sparkleSprite;

    [Header("Rotazione")]
    public float rotationSpeed = 180f;

    [Header("Scala")]
    public float minScale = 0.3f;
    public float maxScale = 1f;
    public float scaleSpeed = 3f;

    [Header("Alpha")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;
    public float alphaSpeed = 3f;
    private float zRotation = 0f;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
            
        if (sparkleSprite == null && transform.childCount > 0)
            sparkleSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Billboard
        sparkleSprite.transform.LookAt(cameraTransform);
        
        // Rotazione Z accumulata separatamente
        zRotation += rotationSpeed * Time.deltaTime;
        sparkleSprite.transform.rotation = Quaternion.LookRotation(cameraTransform.position - sparkleSprite.transform.position) * Quaternion.Euler(0f, 0f, zRotation);

        // Scala
        float t = (Mathf.Sin(Time.time * scaleSpeed) + 1f) / 2f;
        float scale = Mathf.Lerp(minScale, maxScale, t);
        sparkleSprite.transform.localScale = new Vector3(scale, scale, scale);

        // Alpha
        float a = (Mathf.Sin(Time.time * alphaSpeed) + 1f) / 2f;
        Color c = sparkleSprite.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, a);
        sparkleSprite.color = c;
    }
}