using System.Collections.Immutable;
using System.Globalization;
using Stubble.Core.Exceptions;
using Stubble.Core.Imported;
using Stubble.Core.Parser;
using Stubble.Core.Parser.Interfaces;
using Stubble.Helpers.Classes;
using Stubble.Helpers.Tokens;
using Stubble.Helpers.Utils;

namespace Stubble.Helpers.Parsers
{
    public class HelperTokenParser : InlineParser
    {
        private readonly ImmutableDictionary<string, HelperRef> _helperMap;

        public HelperTokenParser(ImmutableDictionary<string, HelperRef> helperMap)
        {
            _helperMap = helperMap;
        }

        public override bool Match(Processor processor, ref StringSlice slice)
        {
            if (processor is null)
            {
                throw new System.ArgumentNullException(nameof(processor));
            }

            var tagStart = slice.Start - processor.CurrentTags.StartTag.Length;
            var index = slice.Start;

            while (slice[index].IsWhitespace())
            {
                index++;
            }

            var nameStart = index;

            // Skip whitespace or until end tag
            while (!slice[index].IsWhitespace() && !slice.Match(processor.CurrentTags.EndTag, index - slice.Start))
            {
                index++;
            }

            var name = slice.ToString(nameStart, index);

            // Skip whitespace or until end tag
            while (slice[index].IsWhitespace() && !slice.Match(processor.CurrentTags.EndTag, index - slice.Start))
            {
                index++;
            }

            if (!_helperMap.TryGetValue(name, out var helperRef))
            {
                return false;
            }

            int contentEnd;
            var argsList = ImmutableArray<HelperArgument>.Empty;
            if (helperRef.ArgumentTypes.Length > 1)
            {
                var argsStart = index;
                slice.Start = index;

                while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
                {
                    slice.NextChar();
                }

                var args = new StringSlice(slice.Text, argsStart, slice.Start - 1);
                args.TrimEnd();
                contentEnd = args.End + 1;

                argsList = ParserUtils.ParseArguments(new StringSlice(args.Text, args.Start, args.End));
            }
            else
            {
                while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
                {
                    slice.NextChar();
                }

                contentEnd = slice.Start;
            }

            if (!slice.Match(processor.CurrentTags.EndTag))
            {
                throw new StubbleException($"Unclosed Tag at {slice.Start.ToString(CultureInfo.InvariantCulture)}");
            }

            var tag = new HelperToken
            {
                TagStartPosition = tagStart,
                ContentStartPosition = nameStart,
                Name = name,
                Args = argsList,
                ContentEndPosition = contentEnd,
                TagEndPosition = slice.Start + processor.CurrentTags.EndTag.Length,
                IsClosed = true
            };
            slice.Start += processor.CurrentTags.EndTag.Length;

            processor.CurrentToken = tag;
            processor.HasSeenNonSpaceOnLine = true;

            return true;
        }
    }
}
