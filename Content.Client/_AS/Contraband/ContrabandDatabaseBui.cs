using Content.Client._AS.Contraband.UI;
using Content.Shared._AS.Contraband.BUI;
using Robust.Client.UserInterface;

namespace Content.Client._AS.Contraband;

public sealed class ContrabandDatabaseBui : BoundUserInterface
{
    [ViewVariables]
    private ContrabandDatabaseMenu? _menu;

    public ContrabandDatabaseBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ContrabandDatabaseMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ContrabandDatabaseState cast)
            return;

        _menu?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _menu?.Close();
        }
    }
}
