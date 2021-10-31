using System.Collections.Immutable;
using Stubble.Helpers.Classes;

namespace Stubble.Helpers.Tokens
{
    internal interface IHelperCallInfo
    {
        string Identifier { get; }

        ImmutableArray<HelperArgument> Args { get; }
    }
}
