using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateRenderFeature : ScriptableRendererFeature
{
    class PixelatePass : ScriptableRenderPass
    {
        public float resolutionScale = 0.25f;

        RTHandle lowResTexture;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;

            desc.depthBufferBits = 0;

            desc.width = Mathf.Max(1, Mathf.RoundToInt(desc.width * resolutionScale));
            desc.height = Mathf.Max(1, Mathf.RoundToInt(desc.height * resolutionScale));

            RenderingUtils.ReAllocateIfNeeded(
                ref lowResTexture,
                desc,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: "_PixelatedTexture"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Pixelate Pass");

            var renderer = renderingData.cameraData.renderer;
            RTHandle cameraTarget = renderer.cameraColorTargetHandle;

            // Downscale
            Blitter.BlitCameraTexture(cmd, cameraTarget, lowResTexture);

            // Upscale (pixel effect)
            Blitter.BlitCameraTexture(cmd, lowResTexture, cameraTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {}
    }

    PixelatePass pass;

    [Range(0.05f,1f)]
    public float resolutionScale = 0.25f;

    public override void Create()
    {
        pass = new PixelatePass();
        pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.resolutionScale = resolutionScale;
        renderer.EnqueuePass(pass);
    }
}