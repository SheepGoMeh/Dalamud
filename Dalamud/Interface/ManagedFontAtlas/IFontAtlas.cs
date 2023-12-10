using System.Threading.Tasks;

using Dalamud.Interface.GameFonts;

using ImGuiNET;

namespace Dalamud.Interface.ManagedFontAtlas;

/// <summary>
/// Wrapper for <see cref="ImFontAtlasPtr"/>.
/// </summary>
public interface IFontAtlas : IDisposable
{
    /// <summary>
    /// Event to be called on build step changes.<br />
    /// <see cref="IFontAtlasBuildToolkit.Font"/> is meaningless for this event.
    /// </summary>
    event FontAtlasBuildStepDelegate? BuildStepChange;

    /// <summary>
    /// Event fired when a font rebuild operation is suggested.<br />
    /// This will be invoked from the main thread.
    /// </summary>
    event Action? RebuildRecommend;

    /// <summary>
    /// Gets the name of the atlas. For logging and debugging purposes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value how the atlas should be rebuilt when the relevant Dalamud Configuration changes. 
    /// </summary>
    FontAtlasAutoRebuildMode AutoRebuildMode { get; }

    /// <summary>
    /// Gets the font atlas. Might be empty.
    /// </summary>
    ImFontAtlasPtr ImAtlas { get; }

    /// <summary>
    /// Gets the task that represents the current font rebuild state.
    /// </summary>
    Task BuildTask { get; }

    /// <summary>
    /// Gets a value indicating whether there exists any built atlas, regardless of <see cref="BuildTask"/>.
    /// </summary>
    bool HasBuiltAtlas { get; }

    /// <summary>
    /// Gets a value indicating whether this font atlas is under the effect of global scale.
    /// </summary>
    bool IsGlobalScaled { get; }

    /// <summary>
    /// Suppresses automatically rebuilding fonts for the scope.
    /// </summary>
    /// <returns>An instance of <see cref="IDisposable"/> that will release the suppression.</returns>
    /// <remarks>
    /// Use when you will be creating multiple new handles, and want rebuild to trigger only when you're done doing so.
    /// This function will effectively do nothing, if <see cref="AutoRebuildMode"/> is set to
    /// <see cref="FontAtlasAutoRebuildMode.Disable"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// using (atlas.SuppressBuild()) {
    ///     this.font1 = atlas.NewGameFontHandle(...);
    ///     this.font2 = atlas.NewDelegateFontHandle(...);
    /// }
    /// </code>
    /// </example>
    public IDisposable SuppressAutoRebuild();

    /// <summary>
    /// Creates a new <see cref="IFontHandle"/> from game's built-in fonts.
    /// </summary>
    /// <param name="style">Font to use.</param>
    /// <returns>Handle to a font that may or may not be ready yet.</returns>
    public IFontHandle NewGameFontHandle(GameFontStyle style);

    /// <summary>
    /// Creates a new IFontHandle using your own callbacks.
    /// </summary>
    /// <param name="buildStepDelegate">Callback for <see cref="IFontAtlas.BuildStepChange"/>.</param>
    /// <returns>Handle to a font that may or may not be ready yet.</returns>
    /// <example>
    /// <b>On initialization</b>:
    /// <code>
    /// this.fontHandle = atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => {
    ///     var config = new SafeFontConfig { SizePx = 16 };
    ///     config.MergeFont = tk.AddFontFromFile(@"C:\Windows\Fonts\comic.ttf", config);
    ///     tk.AddGameSymbol(config);
    ///     tk.AddExtraGlyphsForDalamudLanguage(config);
    ///     // optionally do the following if you have to add more than one font here,
    ///     // to specify which font added during this delegate is the final font to use.
    ///     tk.Font = config.MergeFont;
    /// }));
    /// // or
    /// this.fontHandle = atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(36)));
    /// </code>
    /// <br />
    /// <b>On use</b>:
    /// <code>
    /// using (this.fontHandle.Push())
    ///     ImGui.TextUnformatted("Example");
    /// </code>
    /// </example>
    public IFontHandle NewDelegateFontHandle(FontAtlasBuildStepDelegate buildStepDelegate);

    /// <summary>
    /// Queues rebuilding fonts, on the main thread.<br />
    /// Note that <see cref="BuildTask"/> would not necessarily get changed from calling this function.
    /// </summary>
    /// <exception cref="InvalidOperationException">If <see cref="AutoRebuildMode"/> is <see cref="FontAtlasAutoRebuildMode.Async"/>.</exception>
    void BuildFontsOnNextFrame();

    /// <summary>
    /// Rebuilds fonts immediately, on the current thread.<br />
    /// Even the callback for <see cref="FontAtlasBuildStep.PostPromotion"/> will be called on the same thread.
    /// </summary>
    /// <exception cref="InvalidOperationException">If <see cref="AutoRebuildMode"/> is <see cref="FontAtlasAutoRebuildMode.Async"/>.</exception>
    void BuildFontsImmediately();

    /// <summary>
    /// Rebuilds fonts asynchronously, on any thread. 
    /// </summary>
    /// <param name="callPostPromotionOnMainThread">Call <see cref="FontAtlasBuildStep.PostPromotion"/> on the main thread.</param>
    /// <returns>The task.</returns>
    /// <exception cref="InvalidOperationException">If <see cref="AutoRebuildMode"/> is <see cref="FontAtlasAutoRebuildMode.OnNewFrame"/>.</exception>
    Task BuildFontsAsync(bool callPostPromotionOnMainThread = true);
}
