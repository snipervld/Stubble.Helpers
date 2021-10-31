using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;

namespace Stubble.Helpers
{
    public class HelperInvertedSectionTokenRenderer : StringObjectRenderer<HelperInvertedSectionToken>
    {
        private readonly HelperExecutor _helperExecutor;

        public HelperInvertedSectionTokenRenderer(ImmutableDictionary<string, HelperRef> helperCache)
        {
            _helperExecutor = new HelperExecutor(helperCache);
        }

        protected override void Write(StringRender renderer, HelperInvertedSectionToken obj, Context context)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var helperContext = new HelperSectionContext(context, obj.SectionContent.ToString());

            if (!_helperExecutor.TryExecuteHelper(obj, helperContext, out var value)
                || !context.IsTruthyValue(value))
            {
                renderer.Render(obj, context);
            }
        }

        protected override async Task WriteAsync(StringRender renderer, HelperInvertedSectionToken obj, Context context)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var helperContext = new HelperSectionContext(context, obj.SectionContent.ToString());

            if (!_helperExecutor.TryExecuteHelper(obj, helperContext, out var value)
                || !context.IsTruthyValue(value))
            {
                await renderer.RenderAsync(obj, context);
            }
        }
    }
}
