using Content.Client.Computer;
using Content.Client.UserInterface.Controls;
using Content.Shared.Shuttles.BUIStates;

namespace Content.Client.Shuttles.UI;

// Frontier: Empty partial class - SetConsole is now in base class
public sealed partial class RadarConsoleWindow : FancyWindow,
    IComputerWindow<NavInterfaceState>
{
}
