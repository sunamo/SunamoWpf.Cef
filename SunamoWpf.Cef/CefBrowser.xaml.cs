namespace SunamoWpf.Cef;

/// <summary>
/// WPF browser control wrapping CefSharp's <see cref="ChromiumWebBrowser"/>, implementing the
/// shared <see cref="ISunamoBrowserT{T}"/> contract used across the Sunamo browser wrappers
/// (SunamoCef/CefBrowser, WebSunamo/SunamoBrowser, UniversalWebControl/SunamoBrowser).
/// </summary>
public partial class CefBrowser : UserControl, ISunamoBrowserT<Control>
{
    private static bool cefInitialized;
    private readonly ChromiumWebBrowser chromiumBrowser;

    /// <summary>
    /// Raised when the user activates the browser's close button. Not wired to any built-in UI;
    /// the host application decides when to raise it.
    /// </summary>
    public event Action? CloseButtonClick;

    /// <summary>
    /// Raised when the user activates a host-defined custom button. Not wired to any built-in UI;
    /// the host application decides when to raise it.
    /// </summary>
    public event Action? CustomButtonClick;

    /// <summary>
    /// Raised with the loaded URL when the main frame finishes loading.
    /// </summary>
    public event VoidString? LoadCompleted;

    /// <summary>
    /// Raised when a popup window would be shown, carrying the control that would host it.
    /// </summary>
    public event VoidT<Control>? ShowPopup;

    /// <summary>
    /// Creates the browser control and its underlying <see cref="ChromiumWebBrowser"/>.
    /// </summary>
    public CefBrowser()
    {
        InitializeComponent();
        EnsureCefInitialized();

        chromiumBrowser = new ChromiumWebBrowser();
        chromiumBrowser.FrameLoadEnd += OnFrameLoadEnd;
        RootGrid.Children.Add(chromiumBrowser);
    }

    private static void EnsureCefInitialized()
    {
        if (cefInitialized) return;
        if (!global::CefSharp.Cef.IsInitialized.GetValueOrDefault())
        {
            var settings = new CefSettings();
            global::CefSharp.Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        cefInitialized = true;
    }

    private void OnFrameLoadEnd(object? sender, FrameLoadEndEventArgs eventArgs)
    {
        if (eventArgs.Frame.IsMain) LoadCompleted?.Invoke(eventArgs.Url);
    }

    /// <summary>
    /// Gets or sets the currently loaded URI. Setting it navigates the browser.
    /// </summary>
    public Uri Source
    {
        get => Uri.TryCreate(chromiumBrowser.Address, UriKind.Absolute, out var uri) ? uri : new Uri("about:blank");
        set => chromiumBrowser.Address = value.ToString();
    }

    /// <summary>
    /// Gets the page HTML synchronously. Not supported by CefSharp's asynchronous model;
    /// use <see cref="GetContent"/> instead.
    /// </summary>
    public string HTML => throw new NotSupportedException($"{nameof(HTML)} is synchronous; use {nameof(GetContent)} instead.");

    /// <summary>
    /// Gets the current page's outer HTML.
    /// </summary>
    /// <returns>The page's outer HTML.</returns>
    public async Task<string> GetContent()
    {
        var response = await chromiumBrowser.EvaluateScriptAsync("document.documentElement.outerHTML");
        return response.Success ? response.Result?.ToString() ?? string.Empty : string.Empty;
    }

    /// <summary>
    /// Gets the current page parsed as an <see cref="HtmlDocument"/>.
    /// </summary>
    /// <returns>The parsed HTML document.</returns>
    public async Task<HtmlDocument> GetHtmlDocument()
    {
        var html = await GetContent();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        return htmlDocument;
    }

    /// <summary>
    /// Navigates the browser to the given URI.
    /// </summary>
    /// <param name="uri">The absolute URI to navigate to.</param>
    public void Navigate(string uri)
    {
        chromiumBrowser.Load(uri);
    }

    /// <summary>
    /// Scrolls the page to its bottom.
    /// </summary>
    /// <returns>True if the scroll script was sent successfully; otherwise, false.</returns>
    public bool ScrollToEnd()
    {
        var result = chromiumBrowser.EvaluateScriptAsync("window.scrollTo(0, document.body.scrollHeight);")
            .GetAwaiter().GetResult();
        return result.Success;
    }

    /// <summary>
    /// Ensures the underlying CEF runtime is initialized. Called automatically from the constructor;
    /// exposed so hosts can pre-initialize CEF before creating the control.
    /// </summary>
    public void Init()
    {
        EnsureCefInitialized();
    }
}
