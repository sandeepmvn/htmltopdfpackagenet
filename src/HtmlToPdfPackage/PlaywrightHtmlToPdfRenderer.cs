using Microsoft.Playwright;

namespace HtmlToPdfPackage;

/// <summary>
/// HTML to PDF renderer backed by Microsoft Playwright (Chromium).
/// </summary>
public sealed class PlaywrightHtmlToPdfRenderer : IHtmlToPdfRenderer
{
    /// <inheritdoc />
    public async Task<byte[]> ConvertHtmlToPdfAsync(
        string html,
        HtmlToPdfOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("HTML content must be provided.", nameof(html));
        }

        options ??= new HtmlToPdfOptions();

        if (options.MaxHtmlLength > 0 && html.Length > options.MaxHtmlLength)
        {
            throw new ArgumentException(
                $"HTML content exceeds the maximum length of {options.MaxHtmlLength} characters.",
                nameof(html));
        }

        var baseUri = TryGetBaseUri(options.BaseUrl);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true
            });

        var contextOptions = new BrowserNewContextOptions
        {
            JavaScriptEnabled = !options.DisableJavaScript
        };

        await using var context = await browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();

        if (!options.AllowRemoteResources)
        {
            await page.RouteAsync("**/*", async route =>
            {
                if (IsRequestAllowed(route.Request.Url, baseUri))
                {
                    await route.ContinueAsync();
                }
                else
                {
                    await route.AbortAsync();
                }
            });
        }

        await page.SetContentAsync(
            html,
            new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = options.TimeoutMs
            });

        var pdfOptions = new PagePdfOptions
        {
            Format = options.PaperFormat,
            Scale = options.Scale,
            PrintBackground = options.PrintBackground,
            Timeout = options.TimeoutMs,
            Margin = BuildMargins(options)
        };

        return await page.PdfAsync(pdfOptions);
    }

    private static Margin? BuildMargins(HtmlToPdfOptions options)
    {
        if (options.MarginTop is null && options.MarginBottom is null &&
            options.MarginLeft is null && options.MarginRight is null)
        {
            return null;
        }

        return new Margin
        {
            Top = options.MarginTop,
            Bottom = options.MarginBottom,
            Left = options.MarginLeft,
            Right = options.MarginRight
        };        
    }

    private static Uri? TryGetBaseUri(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        throw new ArgumentException("BaseUrl must be an absolute URL when provided.", nameof(baseUrl));
    }

    private static bool IsRequestAllowed(string requestUrl, Uri? baseUri)
    {
        if (requestUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            requestUrl.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out var requestUri))
        {
            return true;
        }

        if (baseUri is null)
        {
            return false;
        }

        return string.Equals(requestUri.Scheme, baseUri.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(requestUri.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase)
            && requestUri.Port == baseUri.Port;
    }
}
