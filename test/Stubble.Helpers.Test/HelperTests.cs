using System;
using System.Globalization;
using System.Linq;
using McMaster.Extensions.Xunit;
using Stubble.Core.Builders;
using Stubble.Helpers.Builders;
using Stubble.Helpers.Contexts;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class HelperTests
    {
        [Fact]
        [UseCulture("en-GB")]
        public void RegisteredHelpersShouldBeRun()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new HelpersBuilder()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            Assert.Equal("£10.00, £100.26", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToUseRendererContext()
        {
            var helpers = new HelpersBuilder()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", context.RenderSettings.CultureInfo);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency Count}}, {{FormatCurrency Count2}}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m }, new Core.Settings.RenderSettings
            {
                CultureInfo = new CultureInfo("en-GB")
            });

            Assert.Equal("£10.00, £100.26", res);
        }

        [Fact]
        [UseCulture("en-GB")]
        public void StubbleShouldContinueWorkingAsNormal()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new HelpersBuilder()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{Count}}: {{FormatCurrency Count}}, {{Count2}}: {{FormatCurrency Count2}}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            Assert.Equal("10: £10.00, 100.26: £100.26", res);
        }

        [Fact]
        [UseCulture("en-GB")]
        public void StubbleShouldContinueWorkingAsNormalWithWhitespace()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new HelpersBuilder()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{  Count  }}: {{  FormatCurrency Count  }}, {{  Count2  }}: {{  FormatCurrency Count2  }}";

            var res = builder.Render(tmpl, new { Count = 10m, Count2 = 100.26m });

            Assert.Equal("10: £10.00, 100.26: £100.26", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToUseContextLookup()
        {
            var helpers = new HelpersBuilder()
                .Register<int>("PrintWithComma", (context, count) =>
                {
                    var arr = context.Lookup<int[]>("List");
                    var index = Array.IndexOf(arr, count);
                    var comma = index != arr.Length - 1
                        ? ", "
                        : "";

                    return $"{count}{comma}";
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{#List}}{{PrintWithComma .}}{{/List}}";

            var list = Enumerable.Range(1, 10).ToArray();

            var res = builder.Render(tmpl, new { List = list });

            Assert.Equal(string.Join(", ", list), res);
        }

        [Fact]
        [UseCulture("en-GB")]
        public void HelpersShouldBeAbleToHaveOnlyStaticParameters()
        {
            var culture = new CultureInfo("en-GB");
            var helpers = new HelpersBuilder()
                .Register<decimal>("FormatCurrency", (context, count) =>
                {
                    return count.ToString("C", culture);
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{FormatCurrency 10.21}}, {{FormatCurrency Count}}";

            var res = builder.Render(tmpl, new { Count = 100.26m });

            Assert.Equal("£10.21, £100.26", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticAndDynamicParameters()
        {
            var helpers = new HelpersBuilder()
                .Register<decimal, decimal>("Multiply", (context, count, multiplier) =>
                {
                    return $"{count * multiplier}";
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{Multiply 5 5}}, {{Multiply Count 5}}";

            var res = builder.Render(tmpl, new { Count = 2 });

            Assert.Equal("25, 10", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticParameterWithSpaces()
        {
            var helpers = new HelpersBuilder()
                .Register<string, string>("DefaultMe", (context, str, @default) =>
                {
                    return string.IsNullOrEmpty(str) ? @default : str;
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{DefaultMe Value ""I'm Defaulted""}}";

            var res = builder.Render(tmpl, new { Value = "" });

            Assert.Equal("I'm Defaulted", res);
        }

        [Fact]
        public void HelpersShouldBeAbleToHaveStaticParameterWithEscapedQuotes()
        {
            var helpers = new HelpersBuilder()
                .Register<string, string>("DefaultMe", (context, str, @default) =>
                {
                    return string.IsNullOrEmpty(str) ? @default : str;
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{DefaultMe Value 'I\'m Defaulted'}}";

            var res = builder.Render(tmpl, new { Value = "" });

            Assert.Equal("I'm Defaulted", res);
        }

        [Theory]
        [InlineData("'Count'")]
        [InlineData("\"Count\"")]
        public void ItShouldCallHelperWhenExistsStaticAndDynamicVariable(string staticValue)
        {
            var helpers = new HelpersBuilder()
                .Register<string, int>("MyHelper", (context, staticVariable, dynamicVariable) =>
                {
                    return $"<{staticVariable}#{dynamicVariable}>";
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{MyHelper " + staticValue + " Count }}";

            var res = builder.Render(tmpl, new { Count = 10 });

            Assert.Equal($"<Count#10>", res);
        }

        [Fact]
        public void ItShouldAllowRegisteredHelpersWithoutArguments()
        {
            var helpers = new HelpersBuilder()
                .Register("PrintListWithComma", (context) => string.Join(", ", context.Lookup<int[]>("List")));

            var builder = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = builder.Render("List: {{PrintListWithComma}}", new { List = new[] { 1, 2, 3 } });

            Assert.Equal("List: 1, 2, 3", res);
        }

        [Fact]
        public void ItShouldNotRenderHelperWithMissingLookedUpArgumentThatIsntValueType()
        {
            var helpers = new HelpersBuilder()
                .Register<string>("ToCapitalLetters", (context, arg) => arg.ToUpperInvariant());

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = renderer.Render("User name is '{{Name}}' and nickname is '{{Nickname}}'. In capital letters name is '{{ToCapitalLetters Name}}' and nickname is '{{ToCapitalLetters Nickname}}'", new
            {
                Name = "John"
            });

            Assert.Equal("User name is 'John' and nickname is ''. In capital letters name is 'JOHN' and nickname is ''", res);
        }

        [Fact]
        public void ItShouldRenderHelperWithConstantQuotedStringArgument()
        {
            var helpers = new HelpersBuilder()
                .Register<string>("ToCapitalLetters", (context, arg) => arg.ToUpperInvariant());

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var res = renderer.Render("User name is '{{Name}}' and nickname is '{{Nickname}}'. In capital letters name is '{{ToCapitalLetters Name}}' and nickname is '{{ToCapitalLetters 'Nickname'}}'", new
            {
                Name = "John"
            });

            Assert.Equal("User name is 'John' and nickname is ''. In capital letters name is 'JOHN' and nickname is 'NICKNAME'", res);
        }

        [Fact]
        public void ItShouldRenderHelperWithTwoConstantArguments()
        {
            var helpers = new HelpersBuilder()
                .Register("ReplaceString", (HelperContext context, string searchString, string oldString, string newString) => searchString?.Replace(oldString, newString, StringComparison.InvariantCulture));

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddHelpers(helpers))
                .Build();

            var result = renderer.Render("Name: {{ReplaceString Name 'XXX' ' '}}", new
            {
                Name = "JohnXXXSmith"
            });

            Assert.Equal("Name: John Smith", result);
        }

        [Fact]
        public void RegisteredSectionHelpersShouldBeRun()
        {
            var helpers = new SectionHelpersBuilder()
                .Register("RenderInnerContent", (context) =>
                {
                    return true;
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddSectionHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{#RenderInnerContent}}Count 1 is {{Count}}{{/RenderInnerContent}}, {{#RenderInnerContent}}Count 2 is {{Count2}}{{/RenderInnerContent}}";

            var res = builder.Render(tmpl, new { Count = 1, Count2 = 2 });

            Assert.Equal("Count 1 is 1, Count 2 is 2", res);
        }

        [Fact]
        public void ItShouldRenderSectionContentWhenHelperReturnsTrue()
        {
            var helpers = new SectionHelpersBuilder()
                .Register("IfEquals", (HelperSectionContext context, int value1, int value2) => value1 == value2);

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddSectionHelpers(helpers))
                .Build();

            var result = renderer.Render(@"{{#IfEquals '5' '5'}}5 equals to 5{{/IfEquals}}
{{#IfEquals NumberFive '5'}}{{NumberFive}} equals to 5{{/IfEquals}}
{{#IfEquals NumberSix '5'}}{{NumberSix}} don't equal to 5{{/IfEquals}}", new
            {
                NumberFive = 5,
                NumberSix = 6,
            });

            Assert.Equal(@"5 equals to 5
5 equals to 5
", result);
        }

        [Fact]
        public void ItShouldRenderTextWhenSectionHelperReturnsString()
        {
            var helpers = new SectionHelpersBuilder()
                .Register("RenderAnotherTemplate", (HelperSectionContext context) => "another template");

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddSectionHelpers(helpers))
                .Build();

            var result = renderer.Render("{{#RenderAnotherTemplate}}template{{/RenderAnotherTemplate}}", null);

            Assert.Equal("another template", result);
        }

        [Fact]
        public void ItShouldRenderSectionHelperWithInnerRegularSection()
        {
            var helpers = new SectionHelpersBuilder()
                .Register("Helper", (HelperSectionContext context) => true);

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddSectionHelpers(helpers))
                .Build();

            var result = renderer.Render("{{#Helper}}{{#Array}}{{.}}{{/Array}}{{/Helper}}", new { Array = new[] { 1, 2, 3 } });

            Assert.Equal("123", result);
        }

        [Fact]
        public void ItShouldRenderSectionHelperWithInnerRegularSectionWithInnerSectionSection()
        {
            var helpers = new SectionHelpersBuilder()
                .Register("Helper", (HelperSectionContext context) => true)
                .Register("InnerHelper", (HelperSectionContext context) => context.Lookup("."));

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddSectionHelpers(helpers))
                .Build();

            var result = renderer.Render("{{#Helper}}{{#Array}}{{#InnerHelper}}{{.}}{{/InnerHelper}}{{/Array}}{{/Helper}}", new { Array = new[] { 1, 2, 3 } });

            Assert.Equal("123", result);
        }

        [Fact]
        public void RegisteredInvertedSectionHelpersShouldBeRun()
        {
            var helpers = new SectionHelpersBuilder()
                .Register<int>("False", (context, count) =>
                {
                    return false;
                });

            var builder = new StubbleBuilder()
                .Configure(conf =>
                {
                    conf.AddSectionHelpers(helpers);
                })
                .Build();

            var tmpl = @"{{^False Count}}Count 1 is {{Count}}{{/False}}";

            var res = builder.Render(tmpl, new { Count = 1 });

            Assert.Equal("Count 1 is 1", res);
        }

        [Fact]
        public void ItShouldRenderInvertedSectionContentWhenHelperReturnsNonTruthyValue()
        {
            var helpers = new SectionHelpersBuilder()
                .Register("IfEquals", (HelperSectionContext context, int value1, int value2) => value1 == value2);

            var renderer = new StubbleBuilder()
                .Configure(conf => conf.AddSectionHelpers(helpers))
                .Build();

            var result = renderer.Render(@"{{^IfEquals '5' '6'}}5 don't equal to 6{{/IfEquals}}
{{^IfEquals NumberSix '5'}}{{NumberSix}} don't equal to 5{{/IfEquals}}
{{^IfEquals NumberFive '5'}}{{NumberFive}} equals to 5{{/IfEquals}}", new
            {
                NumberSix = 6,
                NumberFive = 5,
            });

            Assert.Equal(@"5 don't equal to 6
6 don't equal to 5
", result);
        }
    }
}
