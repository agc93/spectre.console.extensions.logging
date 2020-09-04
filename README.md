# Spectre.Cli.Extensions.DependencyInjection

A highly opioninated logger implementation for [`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging/) that uses [`Spectre.Console`](https://github.com/spectresystems/spectre.console) to write to the console.

## Getting started

Once you've installed both [`Spectre.Console`](https://www.nuget.org/packages/Spectre.Console/) and this package, just add the `SpectreConsoleLogger` using the normal `Add` extension methods. If you're creating loggers manually:

```csharp
using (var factory = LoggerFactory.Create(b => b.AddSpectreConsole())) {
    _logger = factory.CreateLogger("SampleCategory");
}
```

Or if you're using dependency injection:

```csharp
//in your startup code
services.AddLogging(builder => build.AddSpectreConsole());

//in your classes
public MyClass(ILogger<MyClass> logger) {
    _logger = logger;
}

```

Since we're using Spectre.Console underneath, that means you can also include markup in your log messages and they will be rendered on the console:

```csharp
_logger.LogWarning($"No files found at [italic red]{path}[/]!. Continuing with [underline]default files only.[/]");
```

> Obviously your other logging providers may not support this markup!

### Inline Logger

There's also an alternative logger implementation that sacrifices some detail for a terser, more human-readable output: `SpectreInlineLogger`. You can use this version by changing the call to `AddSpectreConsole()` to `AddInlineSpectreConsole()`. This (highly opinionated!) implementation is intended less as an application logging facility and more as a console log/host for command-line apps.

### Configuration

There's also some configuration options available from the `SpectreConsoleLoggerConfiguration` class. You can optionally pass an instance of the configuration, or a configuration action, in the call to `AddSpectreConsole()`.

## Examples

To see Spectre.Console in action, install the [dotnet-example](https://github.com/patriksvensson/dotnet-example) global tool.

```
> dotnet tool install -g dotnet-example
```

Now you can list available examples in this repository:

```
> dotnet example

┌─────────┬────────────────────────────────┬──────────────────────────────────────────────────────────────┐
│ Name    │ Path                           │ Description                                                  │
├─────────┼────────────────────────────────┼──────────────────────────────────────────────────────────────┤
│ Default │ samples/Default/Default.csproj │ Demonstrates the default output of the SpectreConsoleLogger. │
│ Inline  │ samples/Inline/Inline.csproj   │ Demonstrates the terser output of the SpectreInlineLogger.   │
└─────────┴────────────────────────────────┴──────────────────────────────────────────────────────────────┘
```

And to run an example:

```
> dotnet example default
info: SampleCategory[0]
      Sample application starting up...
trce: SampleCategory[1234]
      Use a familiar format for logging messages
...
```