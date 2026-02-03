namespace HtmlToPdfPackage;

/// <summary>
/// Convenience wrapper for converting HTML to PDF using the default renderer.
/// </summary>
public static class HtmlToPdfConverter
{
    /// <summary>
    /// Converts the provided HTML into a PDF byte array using Playwright.
    /// </summary>
    /// <param name="html">The HTML markup to render.</param>
    /// <param name="options">Rendering options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PDF data as a byte array.</returns>
    /// <remarks>
    /// This convenience method creates a new Playwright instance and browser for each call,
    /// which is inefficient for frequent conversions. For applications performing multiple
    /// conversions, create and reuse a <see cref="PlaywrightHtmlToPdfRenderer"/> instance
    /// instead to avoid the overhead of launching Playwright and a browser repeatedly.
    /// </remarks>
    public static Task<byte[]> ConvertHtmlToPdfAsync(
        string html,
        HtmlToPdfOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var renderer = new PlaywrightHtmlToPdfRenderer();
        return renderer.ConvertHtmlToPdfAsync(html, options, cancellationToken);
    }
}
