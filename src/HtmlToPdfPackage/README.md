# HtmlToPdfPackage

Cross-platform HTML-to-PDF conversion for .NET using Microsoft Playwright. The library targets enterprise defaults by
blocking remote network access and disabling JavaScript by default.

## Features

- Linux and Windows support via Playwright (Chromium engine).
- Secure defaults: JavaScript disabled and remote network requests blocked.
- Configurable page format, margins, timeouts, and rendering scale.
- Simple API with async support.

## Installation

```bash
# Install the NuGet package once published
# dotnet add package HtmlToPdfPackage
```

Playwright requires browser binaries. Install them once per environment:

```bash
playwright install chromium
```

## Azure App Service & Azure Functions

Playwright can run on Azure App Service and Azure Functions when the Chromium browser binaries and required
system dependencies are present. Use one of the following approaches:

### Option A: Build-time install (recommended for App Service)

Install the browser binaries as part of your build or deploy pipeline and copy the Playwright cache into your
deployment artifact (for example, under `/home/site/wwwroot/.playwright` on Linux).

```bash
playwright install chromium
```

Ensure the environment variable below points to the directory where the binaries are deployed:

```bash
PLAYWRIGHT_BROWSERS_PATH=/home/site/wwwroot/.playwright
```

### Option B: Startup install (Functions or App Service)

Run the Playwright install command at startup (for example, in a Functions startup script) and cache the
result in a writable path. Example for Linux:

```bash
PLAYWRIGHT_BROWSERS_PATH=/home/site/wwwroot/.playwright
playwright install chromium
```

> Note: Consuming plan Functions may have limited writable storage and cold-start constraints. Prefer Premium
> or Dedicated plans for predictable rendering performance.

### Required system dependencies

Playwright needs Chromium dependencies on Linux. On Azure App Service (Linux), use a custom container or
startup script to install the dependencies listed in the Playwright documentation.

## Usage

## Usage

```csharp
using HtmlToPdfPackage;

var html = "<html><body><h1>Hello PDF</h1></body></html>";
var options = new HtmlToPdfOptions
{
    PaperFormat = "A4",
    PrintBackground = true,
    BaseUrl = "https://example.com",
    AllowRemoteResources = false,
    DisableJavaScript = true,
    MaxHtmlLength = 1_000_000 // example limit in characters/bytes
};

byte[] pdf = await HtmlToPdfConverter.ConvertHtmlToPdfAsync(html, options);
File.WriteAllBytes("output.pdf", pdf);
```

## Security Notes

- Remote network requests are blocked by default. Enable `AllowRemoteResources` only if you trust the content.
- JavaScript is disabled by default to reduce attack surface.
- Use the `MaxHtmlLength` option to bound payload size for multi-tenant environments.

## License

MIT
