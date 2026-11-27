using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UILogic
{
    /// <summary>
    /// 这一帧要描边的渲染器。颜色由预览逻辑填好，Renderer Feature 统一画。
    /// </summary>
    public static class PreviewOutline
    {
        public const int Yellow = 0;
        public const int Green = 1;
        public const int Red = 2;

        public const uint YellowLayer = 1u << 16;
        public const uint GreenLayer = 1u << 17;
        public const uint RedLayer = 1u << 18;
        public const uint LayerMask = YellowLayer | GreenLayer | RedLayer;

        public struct Entry
        {
            public Renderer Renderer;
            public int Color;
        }

        static readonly List<Entry> Entries = new List<Entry>();

        public static int Count
        {
            get { return Entries.Count; }
        }

        public static void Clear()
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                var renderer = Entries[i].Renderer;
                if (renderer != null)
                    renderer.renderingLayerMask &= ~LayerMask;
            }

            Entries.Clear();
        }

        public static void Add(Renderer renderer, int color)
        {
            if (renderer == null)
                return;
            renderer.renderingLayerMask = (renderer.renderingLayerMask & ~LayerMask) | LayerOf(color);
            Entries.Add(new Entry { Renderer = renderer, Color = color });
        }

        public static uint LayerOf(int color)
        {
            if (color == Green)
                return GreenLayer;
            if (color == Red)
                return RedLayer;
            return YellowLayer;
        }

        public static Color Tint(int color)
        {
            if (color == Green)
                return new Color(0.25f, 0.82f, 0.38f, 1f);
            if (color == Red)
                return new Color(0.86f, 0.22f, 0.20f, 1f);
            return new Color(0.91f, 0.72f, 0.28f, 1f);
        }
    }

    /// <summary>
    /// 用角色自己的材质贴图裁出轮廓，再沿遮罩外缘描一圈。
    /// </summary>
    public class PreviewOutlineFeature : ScriptableRendererFeature
    {
        [SerializeField] Shader _maskShader;
        [SerializeField] Shader _edgeShader;

        PreviewOutlinePass _pass;

        public override void Create()
        {
            _pass = new PreviewOutlinePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (PreviewOutline.Count == 0)
                return;
            if (!_pass.Prepare(_maskShader, _edgeShader))
                return;
            renderer.EnqueuePass(_pass);
        }

        sealed class PreviewOutlinePass : ScriptableRenderPass
        {
            static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");

            readonly List<ShaderTagId> _passes = new List<ShaderTagId>(3);
            Material _edge;
            Shader _mask;

            public PreviewOutlinePass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
                _passes.Add(new ShaderTagId("SRPDefaultUnlit"));
                _passes.Add(new ShaderTagId("UniversalForward"));
                _passes.Add(new ShaderTagId("UniversalForwardOnly"));
            }

            public bool Prepare(Shader mask, Shader edge)
            {
                if (mask == null)
                    mask = Shader.Find("GameContentEditor/PreviewOutlineMask");
                if (edge == null)
                    edge = Shader.Find("GameContentEditor/PreviewOutlineEdge");
                if (mask == null || edge == null)
                    return false;
                _mask = mask;
                if (_edge != null)
                    return true;

                _edge = new Material(edge);
                _edge.hideFlags = HideFlags.HideAndDontSave;
                return true;
            }

            class MaskData
            {
                public RendererListHandle Yellow;
                public RendererListHandle Green;
                public RendererListHandle Red;
            }

            class EdgeData
            {
                public TextureHandle Mask;
                public Material Edge;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                var rendering = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                if (cameraData.isPreviewCamera || _mask == null || _edge == null || PreviewOutline.Count == 0)
                    return;

                var desc = cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;
                desc.depthStencilFormat = GraphicsFormat.None;
                desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                var mask = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "PreviewOutlineMask", true);

                using (var builder = renderGraph.AddRasterRenderPass<MaskData>("Preview Outline Mask", out var data))
                {
                    builder.SetRenderAttachment(mask, 0, AccessFlags.Write);
                    data.Yellow = ListFor(renderGraph, rendering, cameraData, lightData, PreviewOutline.YellowLayer);
                    data.Green = ListFor(renderGraph, rendering, cameraData, lightData, PreviewOutline.GreenLayer);
                    data.Red = ListFor(renderGraph, rendering, cameraData, lightData, PreviewOutline.RedLayer);
                    builder.UseRendererList(data.Yellow);
                    builder.UseRendererList(data.Green);
                    builder.UseRendererList(data.Red);
                    builder.AllowGlobalStateModification(true);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (MaskData pass, RasterGraphContext context) =>
                    {
                        var cmd = context.cmd;
                        cmd.SetGlobalColor(OutlineColorId, PreviewOutline.Tint(PreviewOutline.Yellow));
                        cmd.DrawRendererList(pass.Yellow);
                        cmd.SetGlobalColor(OutlineColorId, PreviewOutline.Tint(PreviewOutline.Green));
                        cmd.DrawRendererList(pass.Green);
                        cmd.SetGlobalColor(OutlineColorId, PreviewOutline.Tint(PreviewOutline.Red));
                        cmd.DrawRendererList(pass.Red);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<EdgeData>("Preview Outline Edge", out var data))
                {
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                    builder.UseTexture(mask, AccessFlags.Read);
                    data.Mask = mask;
                    data.Edge = _edge;
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (EdgeData pass, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, pass.Mask, new Vector4(1f, 1f, 0f, 0f), pass.Edge, 0);
                    });
                }
            }

            RendererListHandle ListFor(
                RenderGraph renderGraph,
                UniversalRenderingData rendering,
                UniversalCameraData cameraData,
                UniversalLightData lightData,
                uint layer)
            {
                var draw = RenderingUtils.CreateDrawingSettings(_passes, rendering, cameraData, lightData, SortingCriteria.None);
                draw.overrideShader = _mask;
                draw.overrideShaderPassIndex = 0;
                draw.perObjectData = PerObjectData.None;
                var filter = new FilteringSettings(RenderQueueRange.all)
                {
                    renderingLayerMask = layer,
                };
                return renderGraph.CreateRendererList(new RendererListParams(rendering.cullResults, draw, filter));
            }
        }
    }
}
