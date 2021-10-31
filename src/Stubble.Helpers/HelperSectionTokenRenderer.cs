using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;

namespace Stubble.Helpers
{
    public class HelperSectionTokenRenderer : StringObjectRenderer<HelperSectionToken>
    {
        private readonly HelperExecutor _helperExecutor;

        public HelperSectionTokenRenderer(ImmutableDictionary<string, HelperRef> helperCache)
        {
            _helperExecutor = new HelperExecutor(helperCache);
        }

        protected override void Write(StringRender renderer, HelperSectionToken obj, Context context)
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

            if (!_helperExecutor.TryExecuteHelper(obj, helperContext, out var value))
            {
                return;
            }

            if (!context.IsTruthyValue(value))
            {
                return;
            }

            if (value is string stringValue)
            {
                renderer.Render(context.RendererSettings.Parser.Parse(stringValue, obj.Tags), context);
            }
            else if (value is IEnumerable arrayValue
                && !context.RendererSettings.SectionBlacklistTypes.Any(x => x.IsInstanceOfType(value)))
            {
                foreach (var v in arrayValue)
                {
                    renderer.Render(obj, context.Push(v));
                }
            }
            else if (value is IEnumerator enumeratorValue)
            {
                while (enumeratorValue.MoveNext())
                {
                    renderer.Render(obj, context.Push(enumeratorValue.Current));
                }

                enumeratorValue.Reset();
            }
            else if (value != null)
            {
                renderer.Render(obj, context.Push(value));
            }
        }

        protected override async Task WriteAsync(StringRender renderer, HelperSectionToken obj, Context context)
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

            if (!_helperExecutor.TryExecuteHelper(obj, helperContext, out var value))
            {
                return;
            }

            if (!context.IsTruthyValue(value))
            {
                return;
            }

            if (value is string stringValue)
            {
                await renderer.RenderAsync(context.RendererSettings.Parser.Parse(stringValue, obj.Tags), context);
            }
            else if (value is IEnumerable arrayValue
                && !context.RendererSettings.SectionBlacklistTypes.Any(x => x.IsInstanceOfType(value)))
            {
                foreach (var v in arrayValue)
                {
                    await renderer.RenderAsync(obj, context.Push(v));
                }
            }
            else if (value is IEnumerator enumeratorValue)
            {
                while (enumeratorValue.MoveNext())
                {
                    await renderer.RenderAsync(obj, context.Push(enumeratorValue.Current));
                }

                enumeratorValue.Reset();
            }
            else if (value != null)
            {
                await renderer.RenderAsync(obj, context.Push(value));
            }
        }
    }
}
