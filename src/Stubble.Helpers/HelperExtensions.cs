using Stubble.Core.Parser.TokenParsers;
using Stubble.Core.Renderers.StringRenderer;
using Stubble.Core.Settings;

namespace Stubble.Helpers
{
    public delegate string HelperDelegate(StringRender renderer, HelperContext context);

    public static class HelperExtensions
    {
        public static RendererSettingsBuilder AddHelpers(this RendererSettingsBuilder builder, Helpers helpers)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            if (helpers is null)
            {
                throw new System.ArgumentNullException(nameof(helpers));
            }

            builder.ConfigureParserPipeline(pipelineBuilder => pipelineBuilder
                .AddBefore<InterpolationTagParser>(new HelperTagParser(helpers.HelperMap)));

            builder.TokenRenderers.Add(new HelperTagRenderer(helpers.HelperMap));

            return builder;
        }

        public static RendererSettingsBuilder AddSectionHelpers(this RendererSettingsBuilder builder, SectionHelpers helpers)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            if (helpers is null)
            {
                throw new System.ArgumentNullException(nameof(helpers));
            }

            builder.ConfigureParserPipeline(pipelineBuilder => pipelineBuilder
                .AddBefore<SectionTagParser>(new HelperSectionTokenParser(helpers.HelperMap))
                .AddBefore<InvertedSectionParser>(new HelperInvertedSectionTokenParser(helpers.HelperMap)));

            builder.TokenRenderers.Add(new HelperSectionTokenRenderer(helpers.HelperMap));
            builder.TokenRenderers.Add(new HelperInvertedSectionTokenRenderer(helpers.HelperMap));

            return builder;
        }
    }
}
