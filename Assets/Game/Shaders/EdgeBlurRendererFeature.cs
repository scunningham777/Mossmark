using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Mossmark.Visuals
{
    public class EdgeBlurRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
            [Range(0f, 1f)] public float falloffStart = 0.4f;
            [Range(0f, 1f)] public float falloffEnd = 0.9f;
            [Min(0f)] public float maxBlurRadius = 12f;
            [Range(4, 32)] public int sampleCount = 24;
        }

        public Settings settings = new Settings();

        private EdgeBlurPass _pass;
        private Material _material;

        public override void Create()
        {
            Shader shader = Shader.Find("Mossmark/EdgeBlur");
            if (shader == null)
            {
                Debug.LogWarning("[EdgeBlur] Shader 'Mossmark/EdgeBlur' not found.");
                return;
            }
            _material = CoreUtils.CreateEngineMaterial(shader);
            _pass = new EdgeBlurPass(_material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null || _pass == null) return;
            _pass.Setup(settings);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);
            _material = null;
        }

        class EdgeBlurPass : ScriptableRenderPass
        {
            readonly Material _material;
            Settings _settings;

            static readonly int FalloffStartId  = Shader.PropertyToID("_FalloffStart");
            static readonly int FalloffEndId    = Shader.PropertyToID("_FalloffEnd");
            static readonly int MaxBlurRadiusId = Shader.PropertyToID("_MaxBlurRadius");
            static readonly int SampleCountId   = Shader.PropertyToID("_SampleCount");

            internal EdgeBlurPass(Material material)
            {
                _material = material;
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            internal void Setup(Settings settings)
            {
                _settings = settings;
                renderPassEvent = settings.passEvent;
                _material.SetFloat(FalloffStartId,  settings.falloffStart);
                _material.SetFloat(FalloffEndId,    settings.falloffEnd);
                _material.SetFloat(MaxBlurRadiusId, settings.maxBlurRadius);
                _material.SetFloat(SampleCountId,   settings.sampleCount);
            }

            class PassData
            {
                public TextureHandle source;
                public Material material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                    return;

                var cameraData = frameData.Get<UniversalCameraData>();
                TextureHandle source = resourceData.activeColorTexture;

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                TextureHandle tempTex = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph, desc, "EdgeBlurTemp", false);

                // Pass 1: apply blur from source → temp
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("EdgeBlur.Effect", out var passData))
                {
                    passData.source   = source;
                    passData.material = _material;
                    builder.UseTexture(source);
                    builder.SetRenderAttachment(tempTex, 0);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (PassData d, RasterGraphContext ctx) =>
                        Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), d.material, 0));
                }

                // Pass 2: copy temp back to source
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("EdgeBlur.CopyBack", out var passData))
                {
                    passData.source   = tempTex;
                    passData.material = null;
                    builder.UseTexture(tempTex);
                    builder.SetRenderAttachment(source, 0);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (PassData d, RasterGraphContext ctx) =>
                        Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), 0, false));
                }
            }
        }
    }
}
