namespace SunamoWpf.Cef.Internal;

/// <summary>
/// Interface for browser control operations.
/// The Control class depends on the type of target application.
/// Used in:
/// - SunamoCef/CefBrowser
/// - WebSunamo/SunamoBrowser
/// - UniversalWebControl/SunamoBrowser
/// </summary>
public interface ISunamoBrowser
{
    /// <summary>
    /// Gets or sets the source URI of the browser.
    /// </summary>
    Uri Source { get; set; }

    /// <summary>
    /// Gets the HTML content.
    /// </summary>
    string HTML { get; }

    /// <summary>
    /// Gets the HTML document.
    /// Must be Task due to UWP! Much time spent with this on GeoCachingTool.
    /// </summary>
    /// <returns>HTML document task.</returns>
    Task<HtmlDocument> GetHtmlDocument();

    /// <summary>
    /// Gets the HTML content.
    /// Sometimes getting outer HTML is quite slow, so put await Task.Delay(500) before calling GetContent().
    /// Remember for troubles with GeoCachingTool.
    /// </summary>
    /// <returns>HTML content task.</returns>
    Task<string> GetContent();

    /// <summary>
    /// Navigates to the specified URI.
    /// </summary>
    /// <param name="uri">The URI to navigate to.</param>
    void Navigate(string uri);

    /// <summary>
    /// Scrolls to the end of the page.
    /// </summary>
    /// <returns>True if successfully scrolled to end; otherwise, false.</returns>
    bool ScrollToEnd();

    /// <summary>
    /// Initializes the browser control.
    /// </summary>
    void Init();
}
