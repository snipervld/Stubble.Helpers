# Stubble Extensions - Helpers (+ Section Helpers) [![Nuget](https://img.shields.io/nuget/vpre/snipervld.Stubble.Helpers.svg?label=Nuget&style=flat-square)](https://www.nuget.org/packages/snipervld.Stubble.Helpers/)


Stubble helpers is an opinionated method of registering helpers with the Stubble renderer to call certain methods with arguments while rendering your templates much like [Handlebars.js helpers](https://handlebarsjs.com/guide/#custom-helpers) and [block helpers](https://handlebarsjs.com/guide/#block-helpers).

To get started with helpers, include the package from nuget and register your helpers like so.
```csharp
var culture = new CultureInfo("en-GB");
var helpers = new Helpers()
    .Register("FormatCurrency", (HelperContext context, decimal count) => count.ToString("C", culture));
var sectionHelpers = new SectionHelpers()
    .Register("IfEqualsTo5", (HelperSectionContext context, decimal count) => count == 5);

var stubble = new StubbleBuilder()
    .Configure(conf =>
        conf
            .AddHelpers(helpers)
            .AddSectionHelpers(sectionHelpers))
    .Build();

var result = stubble.Render("{{FormatCurrency Count}}", new { Count = 100.26m });

Assert.Equal("£100.26", result);

result = stubble.Render("{{#IfEqualsTo5 Count}}{{Count}} equals to 5{{/IfEqualsTo5}}", new { Count = 5 });

Assert.Equal("5 equals to 5", result);

result = stubble.Render("{{^IfEqualsTo5 Count}}{{Count}} doesn't equal to 5{{/IfEqualsTo5}}", new { Count = 6 });

Assert.Equal("6 doesn't equal to 5", result);
```

For more advanced cases you can use the `HelperContext` or the `HelperSectionContext` to get access to values in your current context in a strongly typed manner like the following:
```csharp
var helpers = new Helpers()
    .Register("PrintListWithComma", (context) => string.Join(", ", context.Lookup<int[]>("List")));

var builder = new StubbleBuilder()
    .Configure(conf => conf.AddHelpers(helpers))
    .Build();

var res = builder.Render("List: {{PrintListWithComma}}", new { List = new[] { 1, 2, 3 } });

Assert.Equal("List: 1, 2, 3", res);
```

You can also have static arguments in your template that will be parsed into your helper. There are some caveats to this which i'll note below the example:
```csharp
var culture = new CultureInfo("en-GB");
var helpers = new Helpers()
    .Register("FormatCurrency", (HelperContext context, decimal count) => count.ToString("C", culture));

var stubble = new StubbleBuilder()
    .Configure(conf => conf.AddHelpers(helpers))
    .Build();

var result = stubble.Render("{{FormatCurrency 10}}", new { });

Assert.Equal("£10.00", result);
```

Cavets:
- The type should match or be convertable to the argument type
- If you're writing a constant string as an argument then it should be escaped with quotes either `"` or `'`.
Quoted strings are treated as verbatim and will not be attempted to be looked up in the context however their type will still be converted
- If you have a quote in your string for example `It's` then you can escape it with a `/` like so: `It/'s`

## Argument Type Converting
The helpers will try to be as smart and convert the parameters types if you're convertable or able to be used as that value. For example `string->int`.

## Limitations
This uses the C# `Func` delegates for registering these functions and so you're limited to 15 parameters but we feel this is a pretty fair limitation and anything more and you should be preprocessing your data before rendering.

There is also no async support inside your helpers for the same reasons since you should be preprocessing your data before rendering in this case.

## Differences From [Stubble.Helpers](https://github.com/StubbleOrg/Stubble.Helpers)
- Added section helpers ("block helpers" in Handlebars.js)
