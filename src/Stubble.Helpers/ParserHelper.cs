using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Stubble.Core.Exceptions;
using Stubble.Core.Imported;

namespace Stubble.Helpers
{
    internal static class ParserHelper
    {
        public static ImmutableArray<HelperArgument> ParseArguments(StringSlice slice)
        {
            slice.TrimStart();
            var args = ImmutableArray.CreateBuilder<HelperArgument>();

            while (!slice.IsEmpty)
            {
                if (slice.CurrentChar == '"' || slice.CurrentChar == '\'')
                {
                    args.Add(ParseQuotedString(ref slice));
                }
                else
                {
                    while (slice.CurrentChar.IsWhitespace())
                    {
                        slice.NextChar();
                    }

                    var start = slice.Start;

                    while (!slice.CurrentChar.IsWhitespace() && !slice.IsEmpty)
                    {
                        slice.NextChar();
                    }

                    args.Add(new HelperArgument(slice.Text.Substring(start, slice.Start - start)));
                }

                while (slice.CurrentChar.IsWhitespace())
                {
                    slice.NextChar();
                }
            }

            return args.ToImmutable();
        }

        private static HelperArgument ParseQuotedString(ref StringSlice slice)
        {
            var startQuote = slice.CurrentChar;
            slice.NextChar();
            var st = slice.Start;

            while (!(slice.CurrentChar == startQuote && slice[slice.Start - 1] != '\\'))
            {
                if (slice.IsEmpty)
                {
                    throw new StubbleException($"Unclosed string at {slice.Start}");
                }

                slice.NextChar();
            }

            var end = slice.Start;
            slice.NextChar();

            return new HelperArgument(Regex.Unescape(slice.Text.Substring(st, end - st)), false);
        }
    }
}
