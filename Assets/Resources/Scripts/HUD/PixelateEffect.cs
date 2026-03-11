using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelateEffect : MonoBehaviour
{
    [Header("Pixelation PSX")]
    [Range(0.05f, 1f)] public float resolutionScale = 0.2f;
    public bool enablePixelation = true;

    void Start() => Apply();

    void Apply()
    {
        var urpAsset = UniversalRenderPipeline.asset;
        if (urpAsset == null) return;
        urpAsset.renderScale = enablePixelation ? resolutionScale : 1f;
    }

    void OnValidate() => Apply();

    void OnDisable()
    {
        var urpAsset = UniversalRenderPipeline.asset;
        if (urpAsset != null) urpAsset.renderScale = 1f;
    }
}