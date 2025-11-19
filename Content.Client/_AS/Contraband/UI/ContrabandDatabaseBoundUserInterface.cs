using Content.Shared._AS.Contraband.BUI;
using Robust.Client.UserInterface;

namespace Content.Client._AS.Contraband.UI;

public sealed class ContrabandDatabaseBoundUserInterface : BoundUserInterface
{
    private ContrabandDatabaseMenu? _menu;

    public ContrabandDatabaseBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ContrabandDatabaseMenu>();
        _menu.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is ContrabandDatabaseState databaseState)
        {
            _menu?.UpdateState(databaseState);
        }
    }
}
