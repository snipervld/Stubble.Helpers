using Stubble.Core.Parser.TokenParsers;
using Stubble.Core.Settings;
using Stubble.Helpers.Builders;
using Stubble.Helpers.Parsers;
using Stubble.Helpers.Renderers;

namespace Stubble.Helpers
{
    public static class HelperExtensions
    {
        public static RendererSettingsBuilder AddHelpers(this RendererSettingsBuilder builder, HelpersBuilder helpersBuilder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            if (helpersBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(helpersBuilder));
            }

            builder.ConfigureParserPipeline(pipelineBuilder => pipelineBuilder
                .AddBefore<InterpolationTagParser>(new HelperTokenParser(helpersBuilder.HelperMap)));

            builder.TokenRenderers.Add(new HelperTokenRenderer(helpersBuilder.HelperMap));

            return builder;
        }

        public static RendererSettingsBuilder AddSectionHelpers(this RendererSettingsBuilder builder, SectionHelpersBuilder helpersBuilder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            if (helpersBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(helpersBuilder));
            }

            builder.ConfigureParserPipeline(pipelineBuilder => pipelineBuilder
                .AddBefore<SectionTagParser>(new HelperSectionTokenParser(helpersBuilder.HelperMap))
                .AddBefore<InvertedSectionParser>(new HelperInvertedSectionTokenParser(helpersBuilder.HelperMap)));

            builder.TokenRenderers.Add(new HelperSectionTokenRenderer(helpersBuilder.HelperMap));
            builder.TokenRenderers.Add(new HelperInvertedSectionTokenRenderer(helpersBuilder.HelperMap));

            return builder;
        }
    }
}
