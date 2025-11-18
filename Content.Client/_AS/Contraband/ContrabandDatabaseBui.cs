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
        _menu.OnRefreshPressed += RequestStateUpdate;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ContrabandDatabaseBuiState cast)
            return;

        _menu?.UpdateState(cast);
    }

    private void RequestStateUpdate()
    {
        // Simply opening the UI requests an update, but we can force a refresh
        SendMessage(new ContrabandDatabaseViewCharacterMessage(string.Empty));
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
