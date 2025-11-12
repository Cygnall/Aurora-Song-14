namespace Content.Shared._Mono.FireControl;

/// <summary>
/// These are for the consoles that provide the user interface for fire control servers.
/// </summary>
[RegisterComponent]
public sealed partial class FireControlConsoleComponent : Component
{
    [ViewVariables]
    public EntityUid? ConnectedServer = null;

    /// <summary>
    /// Next time a shipgun firing log should be created
    /// </summary>
    [DataField]
    public TimeSpan NextLog = TimeSpan.Zero;

    /// <summary>
    /// Range to look for grids when logging shipgun fires
    /// </summary>
    [DataField]
    public float LogGridLookupRange = 2000f;

    /// <summary>
    /// Minimum time between shipgun firing logs
    /// </summary>
    [DataField]
    public TimeSpan LogSpacing = TimeSpan.FromSeconds(3);
}
