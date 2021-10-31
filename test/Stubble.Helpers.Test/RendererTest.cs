using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Stubble.Core.Contexts;
using Stubble.Core.Renderers.StringRenderer;
using Stubble.Core.Settings;
using Stubble.Helpers.Classes;
using Stubble.Helpers.Contexts;
using Stubble.Helpers.Renderers;
using Stubble.Helpers.Tokens;
using Xunit;

namespace Stubble.Helpers.Test
{
    public class RendererTests
    {
        [Fact]
        public void ItShouldRenderNothingWhenHelperDoesntExist()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.Create<string, HelperRef>();

            var tagRenderer = new HelperTokenRenderer(helpers);

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }

        [Fact]
        public void ItShouldCallHelperWhenExists()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<10>", res);
        }

        [Fact]
        public void ItShouldCallHelperWhenExistsWithArgumentFromParent()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, int, string>((helperContext, count, count2) =>
            {
                return $"<{count}-{count2}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"), new HelperArgument("Count2"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings)
                .Push(new { Count2 = 20 });

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<10-20>", res);
        }

        [Fact]
        public void ItShouldCallHelperWhenExistsWithArgumentOverrideFromParent()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, int, string>((helperContext, count, count2) =>
            {
                return $"<{count}-{count2}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"), new HelperArgument("Count2"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings)
                .Push(new { Count = 20, Count2 = 20 });

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<20-20>", res);
        }

        [Fact]
        public void ItShouldRenderNothingWhenValueDoesntExist()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count1"))
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }

        [Fact]
        public void ItShouldRenderNothingWhenTypesDoNotMatch()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = "wrong-type" }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }

        [Fact]
        public void ItShouldRenderWhenTypesNotMatchCanBeConverted()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = "10" }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<10>", res);
        }

        [Fact]
        public void ItShouldRenderWhenTypesMatchBaseType()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, object, string>((helperContext, src) =>
            {
                if (!(src is IDictionary<object, object> dic))
                {
                    return string.Empty;
                }
                return $"<{dic["value"]}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count"))
            };

            var context = new Context(new { Count = new Dictionary<object, object> { { "value", "10" } } }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<10>", res);
        }

        [Fact]
        public void ItShouldRenderAllowHelpersWithNoArguments()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, string>((helperContext) =>
            {
                return $"<{helperContext.Lookup<int>("Count")}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<10>", res);
        }

        [Fact]
        public void ItShouldApplyTheCultureOfTheRenderer()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings
            {
                CultureInfo = new CultureInfo("ru-RU")
            };
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, decimal>((helperContext) =>
            {
                return helperContext.Lookup<decimal>("Count");
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 1.21m }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("1,21", res);
        }

        [Fact]
        public void ItShouldHandleNullReturnFromHelper()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, object>((helperContext) =>
            {
                return null;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperTokenRenderer(helpers.ToImmutable());

            var token = new HelperToken
            {
                Name = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty
            };

            var context = new Context(new { Count = 1.21m }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }

        [Fact]
        public void ItShouldRenderNothingWhenSectionHelperDoesntExist()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.Create<string, HelperRef>();

            var tagRenderer = new HelperSectionTokenRenderer(helpers);

            var token = new HelperSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }

        [Fact]
        public void ItShouldCallSectionHelperWhenExistsAndReturnsTruthyValue()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, bool>((helperContext) =>
            {
                return true;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(null, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("text", res);
        }

        [Fact]
        public void ItShouldRenderNothingWhenHelperExistsAndReturnsNonTruthyValue()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, bool>((helperContext) =>
            {
                return false;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(null, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }

        [Fact]
        public void ItShouldCallSectionHelperWhenExistsAndReturnsStringValue()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, string>((helperContext, count) =>
            {
                return $"<{count}>";
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Count")),
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("<10>", res);
        }

        [Fact]
        public void ItShouldRenderSectionContentPerItemWhenSectionHelperReturnsEnumerable()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, int, int, IEnumerable<int>>((helperContext, start, count) =>
            {
                return Enumerable.Range(start, count);
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray.Create(new HelperArgument("Start"), new HelperArgument("Count")),
                SectionContent = new Core.Imported.StringSlice("{{.}},"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.InterpolationToken
                        {
                            Content = new Core.Imported.StringSlice("."),
                        },
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice(",") },
                        },
                    },
            };

            var context = new Context(new { Start = 2, Count = 3 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("2,3,4,", res);
        }

        [Fact]
        public void ItShouldRenderSectionContentPerItemWhenSectionHelperReturnsEnumerator()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();
            var rangeEnumerator = Enumerable.Range(2, 3).GetEnumerator();

            var helper = new Func<HelperContext, IEnumerator<int>>((helperContext) =>
            {
                return rangeEnumerator;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("{{.}},"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.InterpolationToken
                        {
                            Content = new Core.Imported.StringSlice("."),
                        },
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice(",") },
                        },
                    },
            };

            var context = new Context(null, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("2,3,4,", res);
        }

        [Fact]
        public void ItShouldRenderContentWhenInvertedSectionHelperDoesntExist()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.Create<string, HelperRef>();

            var tagRenderer = new HelperInvertedSectionTokenRenderer(helpers);

            var token = new HelperInvertedSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(new { Count = 10 }, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("text", res);
        }

        [Fact]
        public void ItShouldRenderContentWhenInvertedSectionHelperExistsAndReturnsNonTruthyValue()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, bool>((helperContext) =>
            {
                return false;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperInvertedSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperInvertedSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(null, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("text", res);
        }

        [Fact]
        public void ItShouldRenderNothingWhenInvertedSectionHelperExistsAndReturnsTruthyValue()
        {
            var writer = new StringWriter();
            var settings = new RendererSettingsBuilder().BuildSettings();
            var renderSettings = new RenderSettings();
            var stringRenderer = new StringRender(writer, settings.RendererPipeline);

            var helpers = ImmutableDictionary.CreateBuilder<string, HelperRef>();

            var helper = new Func<HelperContext, bool>((helperContext) =>
            {
                return true;
            });

            helpers.Add("MyHelper", new HelperRef(helper));

            var tagRenderer = new HelperInvertedSectionTokenRenderer(helpers.ToImmutable());

            var token = new HelperInvertedSectionToken
            {
                SectionName = "MyHelper",
                Args = ImmutableArray<HelperArgument>.Empty,
                SectionContent = new Core.Imported.StringSlice("text"),
                Children =
                    new List<Core.Tokens.MustacheToken>
                    {
                        new Core.Tokens.LiteralToken
                        {
                            Content = new[] { new Core.Imported.StringSlice("text") },
                        },
                    },
            };

            var context = new Context(null, settings, renderSettings);

            tagRenderer.Write(stringRenderer, token, context);

            var res = writer.ToString();

            Assert.Equal("", res);
        }
    }
}
