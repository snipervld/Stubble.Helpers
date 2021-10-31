using Stubble.Core.Contexts;

namespace Stubble.Helpers.Contexts
{
    public class HelperSectionContext : HelperContext
    {
        public HelperSectionContext(Context context, string content)
            : base(context)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
