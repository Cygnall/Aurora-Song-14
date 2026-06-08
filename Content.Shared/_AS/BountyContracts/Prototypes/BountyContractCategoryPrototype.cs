using Robust.Shared.Prototypes;

namespace Content.Shared._AS.BountyContracts.Prototypes;

/// <summary>
///     An individual category of the bounty contract PDA app.
///     This represents a "type" of bounty, or what specifically is being paid for.
/// </summary>
[Prototype]
public sealed partial class BountyContractCategoryPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     A locale corresponding to this category's display name.
    /// </summary>
    [DataField]
    public LocId Name = string.Empty;

    /// <summary>
    ///     A localized name for this category.
    /// </summary>
    public string DisplayName => Loc.GetString(Name);

    /// <summary>
    ///     A locale corresponding to some text that is displayed when this bounty is created.
    /// </summary>
    [DataField]
    public LocId? Announcement = null;

    /// <summary>
    ///     The background color of bounties with this category in the UI.
    /// </summary>
    [DataField]
    public Color UiColor = Color.White;

    /// <summary>
    ///     Whether or not the name of the bounty should be custom by default.
    /// </summary>
    [DataField]
    public bool CustomNameDefault = true;
}
