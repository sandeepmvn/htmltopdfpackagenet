using Microsoft.Playwright;

namespace HtmlToPdfPackage;

/// <summary>
/// HTML to PDF renderer backed by Microsoft Playwright (Chromium).
/// Create a single instance and reuse it for multiple conversions to avoid the overhead
/// of launching a new browser instance for each conversion (typically hundreds of milliseconds).
/// This class implements IAsyncDisposable and must be properly disposed to release browser resources.
/// </summary>
public sealed class PlaywrightHtmlToPdfRenderer : IHtmlToPdfRenderer, IAsyncDisposable
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private volatile bool _disposed;

    /// <inheritdoc />
    public async Task<byte[]> ConvertHtmlToPdfAsync(
        string html,
        HtmlToPdfOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

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

        await EnsureInitializedAsync(cancellationToken);

        var contextOptions = new BrowserNewContextOptions
        {
            JavaScriptEnabled = !options.DisableJavaScript
        };

        await using var context = await _browser!.NewContextAsync(contextOptions);
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

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        // Check disposed before attempting to acquire lock
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            // Check again after acquiring lock
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_playwright is not null && _browser is not null)
            {
                return;
            }

            IPlaywright? playwright = null;
            try
            {
                playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    });

                // Only set fields after successful initialization
                _playwright = playwright;
                _browser = browser;
            }
            catch
            {
                // Clean up on failure
                playwright?.Dispose();
                throw;
            }
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method will wait for any ongoing initialization to complete before disposing resources.
    /// If disposal must complete quickly, avoid calling from time-sensitive contexts.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        // Quick check to avoid unnecessary lock acquisition
        if (_disposed)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_browser is not null)
            {
                await _browser.DisposeAsync();
            }

            _playwright?.Dispose();
        }
        finally
        {
            _initLock.Release();
        }

        // Dispose the semaphore after releasing the lock and marking as disposed.
        // Note: There's a small theoretical race window where a thread could pass the
        // pre-lock check at line 144 and attempt to acquire the lock while we dispose it.
        // However, this is extremely unlikely because:
        // 1. _disposed is volatile, ensuring visibility
        // 2. The window is microseconds at most
        // 3. If it happens, the thread will get an ObjectDisposedException from the semaphore
        // This is an acceptable tradeoff vs. the complexity of additional synchronization.
        _initLock.Dispose();
    }
}
