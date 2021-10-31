using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;
using Stubble.Helpers.Classes;
using Stubble.Helpers.Contexts;
using Stubble.Helpers.Tokens;
using Stubble.Helpers.Utils;

namespace Stubble.Helpers.Renderers
{
    public class HelperTokenRenderer : StringObjectRenderer<HelperToken>
    {
        private readonly HelperExecutor _helperExecutor;

        public HelperTokenRenderer(ImmutableDictionary<string, HelperRef> helperCache)
        {
            _helperExecutor = new HelperExecutor(helperCache);
        }

        protected override void Write(StringRender renderer, HelperToken obj, Context context)
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

            var helperContext = new HelperContext(context);

            if (_helperExecutor.TryExecuteHelper(obj, helperContext, out var value))
            {
                if (value is string str)
                {
                    renderer.Write(str);
                }
                else if (value is object)
                {
                    renderer.Write(Convert.ToString(value, context.RenderSettings.CultureInfo));
                }
            }
        }

        protected override Task WriteAsync(StringRender renderer, HelperToken obj, Context context)
        {
            Write(renderer, obj, context);

            return Task.CompletedTask;
        }
    }
}
