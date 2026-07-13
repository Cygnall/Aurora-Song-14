// ReSharper disable once CheckNamespace
namespace Content.Shared.Access.Components;

public sealed partial class IdCardComponent : Component
{
    /// <summary>
    /// The ID associated with the profile stored in the database.
    /// </summary>
    [DataField]
    public int? ProfileId;
}
