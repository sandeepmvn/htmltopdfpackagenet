namespace HtmlToPdfPackage;

/// <summary>
/// Options controlling HTML to PDF rendering.
/// </summary>
public sealed class HtmlToPdfOptions
{
    /// <summary>
    /// Optional base URL for resolving relative resources in the HTML.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Allow external network requests when rendering HTML. Default is false.
    /// </summary>
    public bool AllowRemoteResources { get; set; }

    /// <summary>
    /// Disables JavaScript execution in the page context. Default is true.
    /// </summary>
    public bool DisableJavaScript { get; set; } = true;

    /// <summary>
    /// Maximum HTML length allowed for rendering. Default is 1,000,000 characters.
    /// </summary>
    public int MaxHtmlLength { get; set; } = 1_000_000;

    /// <summary>
    /// Timeout for HTML rendering and PDF generation in milliseconds. Default is 30 seconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 30_000;

    /// <summary>
    /// Page format to use (e.g., "A4", "Letter"). Defaults to "A4".
    /// </summary>
    public string PaperFormat { get; set; } = "A4";

    /// <summary>
    /// Scale of the webpage rendering. Defaults to 1.0.
    /// </summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>
    /// Whether to print background graphics. Defaults to true.
    /// </summary>
    public bool PrintBackground { get; set; } = true;

    /// <summary>
    /// Top margin in CSS units (e.g., "1in", "20mm").
    /// </summary>
    public string? MarginTop { get; set; }

    /// <summary>
    /// Bottom margin in CSS units (e.g., "1in", "20mm").
    /// </summary>
    public string? MarginBottom { get; set; }

    /// <summary>
    /// Left margin in CSS units (e.g., "1in", "20mm").
    /// </summary>
    public string? MarginLeft { get; set; }

    /// <summary>
    /// Right margin in CSS units (e.g., "1in", "20mm").
    /// </summary>
    public string? MarginRight { get; set; }
}
