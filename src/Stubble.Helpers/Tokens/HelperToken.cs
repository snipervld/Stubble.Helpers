using System.Collections.Immutable;
using Stubble.Core.Tokens;
using Stubble.Helpers.Classes;
using Stubble.Helpers.Utils;

namespace Stubble.Helpers.Tokens
{
    public class HelperToken : InlineToken<HelperToken>, INonSpace, IHelperCallInfo
    {
        public string Name { get; set; }

        public ImmutableArray<HelperArgument> Args { get; set; }

        public override bool Equals(HelperToken other) =>
            other is object
            && (other.TagStartPosition, other.TagEndPosition, other.ContentStartPosition, other.ContentEndPosition, other.IsClosed)
                == (TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, IsClosed)
            && other.Content.Equals(Content)
            && other.Name.Equals(Name, System.StringComparison.OrdinalIgnoreCase)
            && CompareUtils.CompareImmutableArraysWithEquatable(Args, other.Args);

        public override bool Equals(object obj)
            => obj is HelperToken a && Equals(a);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => (Name, Args, TagStartPosition, TagEndPosition, ContentStartPosition, ContentEndPosition, Content, IsClosed).GetHashCode();

        string IHelperCallInfo.Identifier => Name;
    }
}
