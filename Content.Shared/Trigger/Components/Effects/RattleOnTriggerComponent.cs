using Content.Shared.Mobs;
using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Trigger.Components.Effects;

/// <summary>
/// Sends an emergency message over coms when triggered giving information about the entity's mob status.
/// If TargetUser is true then the user's mob state will be used instead.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RattleOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// The radio channel the message will be sent to.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Syndicate";

    /// <summary>
    /// The message to be send depending on the target's current mob state.
    /// </summary>
    [DataField]
    public Dictionary<MobState, LocId> Messages = new()
    {
        {MobState.Critical, "deathrattle-implant-critical-message"},
        {MobState.Dead, "deathrattle-implant-dead-message"} // Aurora's Song: Reverted these to the older, more informative locale
    };

    // Aurora's Song.
    /// <summary>
    /// What time the user died, so Medics know how long they have been neglected for.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public TimeSpan DeathTime = TimeSpan.Zero;

    // Aurora's Song.
    /// <summary>
    /// What time the implant should next be retriggered automatically.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public TimeSpan NextTrigger = TimeSpan.Zero;
    
    // Aurora's Song.
    /// <summary>
    /// The delay between implant retriggers.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public TimeSpan RetriggerDelay = TimeSpan.FromMinutes(5);
}