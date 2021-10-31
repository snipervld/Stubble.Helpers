using System.Collections.Immutable;
using System.Globalization;
using Stubble.Core.Exceptions;
using Stubble.Core.Imported;
using Stubble.Core.Parser;
using Stubble.Core.Parser.Interfaces;
using Stubble.Core.Tokens;
using Stubble.Helpers.Classes;
using Stubble.Helpers.Tokens;
using Stubble.Helpers.Utils;

namespace Stubble.Helpers.Parsers
{
    public class HelperInvertedSectionTokenParser : BlockParser
    {
        private const char OpeningTagDelimiter = '^';
        private readonly ImmutableDictionary<string, HelperRef> _helperMap;

        public HelperInvertedSectionTokenParser(ImmutableDictionary<string, HelperRef> helperMap)
        {
            _helperMap = helperMap;
        }

        public override ParserState TryOpenBlock(Processor processor, ref StringSlice slice)
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

            var match = slice[index];

            if (match == OpeningTagDelimiter)
            {
                slice.Start = index + 1; // Skip delimiter

                while (slice.CurrentChar.IsWhitespace())
                {
                    slice.NextChar();
                }

                var startIndex = slice.Start;

                // Take characters until whitespace or until closing tag
                while (!slice.IsEmpty && !slice.CurrentChar.IsWhitespace() && !slice.Match(processor.CurrentTags.EndTag))
                {
                    slice.NextChar();
                }

                var sectionName = slice.ToString(startIndex, slice.Start).TrimEnd();

                if (!_helperMap.TryGetValue(sectionName, out var helperRef))
                {
                    return ParserState.Continue;
                }

                var argsList = ImmutableArray<HelperArgument>.Empty;

                if (helperRef.ArgumentTypes.Length > 1)
                {
                    while (!slice.IsEmpty && slice.CurrentChar.IsWhitespace())
                    {
                        slice.NextChar();
                    }

                    var argsStart = slice.Start;

                    while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
                    {
                        slice.NextChar();
                    }

                    var args = new StringSlice(slice.Text, argsStart, slice.Start - 1);
                    args.TrimEnd();

                    argsList = ParserUtils.ParseArguments(new StringSlice(args.Text, args.Start, args.End));
                }
                else
                {
                    while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
                    {
                        slice.NextChar();
                    }
                }

                if (!slice.Match(processor.CurrentTags.EndTag))
                {
                    throw new StubbleException($"Unclosed Tag at {slice.Start.ToString(CultureInfo.InvariantCulture)}");
                }

                var contentStartPosition = slice.Start + processor.CurrentTags.EndTag.Length;

                var sectionTag = new HelperInvertedSectionToken
                {
                    SectionName = sectionName,
                    Args = argsList,
                    StartPosition = tagStart,
                    ContentStartPosition = contentStartPosition,
                    Parser = this,
                    IsClosed = false,
                };

                processor.CurrentToken = sectionTag;
                slice.Start += processor.CurrentTags.EndTag.Length;

                return ParserState.Break;
            }

            return ParserState.Continue;
        }

        public override void EndBlock(Processor processor, BlockToken token, BlockCloseToken closeToken, StringSlice content)
        {
            if (processor is null)
            {
                throw new System.ArgumentNullException(nameof(processor));
            }

            if (token is HelperInvertedSectionToken sectionTag && closeToken is SectionEndToken sectionEndTag)
            {
                if (sectionTag.SectionName == sectionEndTag.SectionName)
                {
                    sectionTag.Tags = processor.CurrentTags;
                    sectionTag.EndPosition = sectionEndTag.EndPosition;
                    sectionTag.ContentEndPosition = sectionEndTag.ContentEndPosition;
                    sectionTag.IsClosed = true;
                    sectionTag.SectionContent = new StringSlice(content.Text, sectionTag.ContentStartPosition, sectionTag.ContentEndPosition - 1);
                }
            }
        }

        public override bool TryClose(Processor processor, ref StringSlice slice, BlockToken token)
        {
            if (processor is null)
            {
                throw new System.ArgumentNullException(nameof(processor));
            }

            if (!(token is HelperInvertedSectionToken sectionTag))
            {
#pragma warning disable CA2208
#pragma warning disable CA1303
                throw new System.ArgumentException(nameof(token));
#pragma warning restore CA1303
#pragma warning restore CA2208
            }

            while (slice.CurrentChar.IsWhitespace())
            {
                slice.NextChar();
            }

            var blockStart = slice.Start - processor.CurrentTags.StartTag.Length;
            slice.Start = slice.Start + 1; // Skip the slash

            while (slice.CurrentChar.IsWhitespace())
            {
                slice.NextChar();
            }

            var startIndex = slice.Start;

            // Take characters until closing tag
            while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
            {
                slice.NextChar();
            }

            var sectionName = slice.ToString(startIndex, slice.Start).TrimEnd();

            if (sectionTag.SectionName == sectionName)
            {
                var tagEnd = slice.Start + processor.CurrentTags.EndTag.Length;

                var endTag =
                    new SectionEndToken
                    {
                        SectionName = sectionName,
                        EndPosition = tagEnd,
                        ContentEndPosition = blockStart,
                        IsClosed = true,
                    };

                processor.CurrentToken = endTag;
                slice.Start += processor.CurrentTags.EndTag.Length;

                return true;
            }

            throw new StubbleException($"Cannot close Block '{sectionName}' at {blockStart}. There is already an unclosed Block '{sectionTag.SectionName}'");
        }
    }
}
