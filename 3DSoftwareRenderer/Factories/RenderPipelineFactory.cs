using SoftwareRenderer3D.Enums;
using SoftwareRenderer3D.RenderingPipelines;
using System.Collections.Generic;

namespace SoftwareRenderer3D.Factories
{
    public static class RenderPipelineFactory
    {
        private static Dictionary<RenderType, IRenderPipeline> _renderPipelineMapping = new Dictionary<RenderType, IRenderPipeline>
        {
            { RenderType.None, new SimplePipeline() },
            { RenderType.Simple, new SimplePipeline() },
            { RenderType.Transparent, new OrderIndependentTransparencyPipeline() },
            { RenderType.SubsurfaceScattering, new SubsurfaceScatteringPipeline() },
        };

        public static IRenderPipeline GetRenderPipeline(RenderType renderType)
        {
            return _renderPipelineMapping[renderType];
        }
    }
}
