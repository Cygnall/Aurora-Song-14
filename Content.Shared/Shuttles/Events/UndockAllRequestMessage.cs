using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events; // Mono

/// <summary>
/// Raised on the client when it wishes to undock all docking ports at once.
/// </summary>
[Serializable, NetSerializable]
public sealed class UndockAllRequestMessage : BoundUserInterfaceMessage
{
    public List<NetEntity> DockEntities;

    public UndockAllRequestMessage(List<NetEntity> dockEntities)
    {
        DockEntities = dockEntities;
    }
}