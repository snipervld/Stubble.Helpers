using System.Collections.Immutable;

namespace Stubble.Helpers
{
    internal interface IHelperCallInfo
    {
        string Identifier { get; }

        ImmutableArray<HelperArgument> Args { get; }
    }
}
