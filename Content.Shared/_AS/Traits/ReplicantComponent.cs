using Robust.Shared.GameStates;

namespace Content.Shared._AS.Traits;

/// <summary>
/// Used for Replicant trait. Used with <see cref="ReplicantSystem"/> to change the player's typing indicator.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ReplicantSystem))]
public sealed partial class ReplicantComponent : Component { }
