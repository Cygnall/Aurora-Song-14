// ReSharper disable once CheckNamespace
namespace Content.Shared.Preferences;

public sealed partial class HumanoidCharacterProfile
{
    [ViewVariables(VVAccess.ReadOnly)]
    public int? ProfileId { get; init; }
}
