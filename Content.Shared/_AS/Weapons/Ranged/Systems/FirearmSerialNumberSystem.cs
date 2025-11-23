using Content.Shared._AS.Weapons.Ranged.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Weapons.Ranged.Systems;

public sealed class FirearmSerialNumberSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FirearmSerialNumberComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FirearmSerialNumberComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<FirearmSerialNumberComponent, FileSerialDoAfterEvent>(OnFileSerialComplete);
    }

    private void OnMapInit(EntityUid uid, FirearmSerialNumberComponent component, MapInitEvent args)
    {
        if (string.IsNullOrEmpty(component.SerialNumber))
            component.SerialNumber = GenerateSerialNumber();
    }

    private void OnInteractUsing(EntityUid uid, FirearmSerialNumberComponent component, InteractUsingEvent args)
    {
        if (args.Handled || component.SerialFiled)
            return;

        // Check if using a welder
        if (!TryComp<ToolComponent>(args.Used, out var tool))
            return;

        // Filing off serial numbers requires significant fuel (5 units)
        args.Handled = _tool.UseTool(
            args.Used,
            args.User,
            uid,
            110f,
            new[] { "Welding" },
            new FileSerialDoAfterEvent(),
            fuel: 5f,
            toolComponent: tool);
    }

    private void OnFileSerialComplete(EntityUid uid, FirearmSerialNumberComponent component, FileSerialDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        component.SerialFiled = true;
        Dirty(uid, component);
        args.Handled = true;
    }

    private string GenerateSerialNumber()
    {
        const string hexChars = "0123456789ABCDEF";
        var chars = new char[16];
        for (int i = 0; i < 16; i++)
            chars[i] = hexChars[_random.Next(16)];
        return new string(chars);
    }
}

[Serializable, NetSerializable]
public sealed partial class FileSerialDoAfterEvent : SimpleDoAfterEvent
{
}
