namespace HtmlToPdfPackage;

/// <summary>
/// Defines a renderer that converts HTML to a PDF document.
/// </summary>
public interface IHtmlToPdfRenderer
{
    /// <summary>
    /// Converts the provided HTML into a PDF byte array.
    /// </summary>
    /// <param name="html">The HTML markup to render.</param>
    /// <param name="options">Rendering options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PDF data as a byte array.</returns>
    Task<byte[]> ConvertHtmlToPdfAsync(
        string html,
        HtmlToPdfOptions? options = null,
        CancellationToken cancellationToken = default);
}
