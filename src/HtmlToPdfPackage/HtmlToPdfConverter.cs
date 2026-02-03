namespace HtmlToPdfPackage;

/// <summary>
/// Convenience wrapper for converting HTML to PDF using the default renderer.
/// </summary>
/// <remarks>
/// This static helper creates a new renderer instance for each call. For better performance
/// when converting multiple documents, create a single PlaywrightHtmlToPdfRenderer instance
/// and reuse it across multiple conversions, then dispose it when done.
/// </remarks>
public static class HtmlToPdfConverter
{
    /// <summary>
    /// Converts the provided HTML into a PDF byte array using Playwright.
    /// </summary>
    /// <param name="html">The HTML markup to render.</param>
    /// <param name="options">Rendering options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PDF data as a byte array.</returns>
    public static async Task<byte[]> ConvertHtmlToPdfAsync(
        string html,
        HtmlToPdfOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var renderer = new PlaywrightHtmlToPdfRenderer();
        return await renderer.ConvertHtmlToPdfAsync(html, options, cancellationToken);
    }
}
