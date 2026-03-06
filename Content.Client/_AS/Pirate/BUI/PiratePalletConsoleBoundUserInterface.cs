using Content.Shared._AS.Contraband.Events;
using Content.Shared._NF.Contraband.BUI;
using Content.Shared._NF.Contraband.Components;
using Content.Shared._NF.Contraband.Events;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client._AS.Pirate.BUI;

public sealed class PiratePalletConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private _AS.Pirate.UI.PiratePalletMenu? _menu;

    [ViewVariables]
    private string _locPrefix = string.Empty;

    public PiratePalletConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        if (EntMan.TryGetComponent<ContrabandPalletConsoleComponent>(owner, out var console))
            _locPrefix = console.LocStringPrefix ?? string.Empty;
    }

    protected override void Open()
    {
        base.Open();

        if (_menu == null)
        {
            _menu = this.CreateWindow<_AS.Pirate.UI.PiratePalletMenu>();
            _menu.AppraiseRequested += OnAppraisal;
            _menu.SellRequested += OnSell;
            _menu.SetWindowText(_locPrefix);
            var disclaimer = new FormattedMessage();
            disclaimer.AddText(Loc.GetString($"{_locPrefix}contraband-pallet-disclaimer"));
            _menu.Disclaimer.SetMessage(disclaimer);
        }
    }

    private void OnAppraisal()
    {
        SendMessage(new ContrabandPalletAppraiseMessage()); // AS: Fix pirate console
    }

    private void OnSell()
    {
        SendMessage(new ContrabandPalletSellMessage()); // AS: Fix pirate console
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ContrabandPalletConsoleInterfaceState palletState)
            return;

        _menu?.SetEnabled(palletState.Enabled);
        _menu?.SetAppraisal(palletState.Appraisal);
        _menu?.SetCount(palletState.Count);
    }
}
