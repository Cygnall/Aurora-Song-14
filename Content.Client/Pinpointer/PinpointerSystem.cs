using Content.Client.GPS.UI; // Aurora's Song
using Content.Client.Items; // Aurora's Song
using Content.Shared.Pinpointer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.Pinpointer;

public sealed partial class PinpointerSystem : SharedPinpointerSystem
{
    [Dependency] private IEyeManager _eyeManager = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    // Aurora's Song Start - Pinpointer GPS
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<PinpointerComponent>(ent => new PinpointerStatusControl(ent));
    }
    // Aurora's Song End

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we want to show pinpointers arrow direction relative
        // to players eye rotation (like it was in SS13)

        // because eye can change it rotation anytime
        // we need to update this arrow in a update loop
        var query = EntityQueryEnumerator<PinpointerComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var pinpointer, out var sprite))
        {
            // Frontier: ensure question mark is aligned with the screen
            if (!pinpointer.HasTarget)
            {
                sprite.LayerSetRotation(PinpointerLayers.Screen, Angle.Zero);
                continue;
            }
            // End Frontier: ensure question mark is aligned with the screen

            var eye = _eyeManager.CurrentEye;
            var angle = pinpointer.ArrowAngle + eye.Rotation;

            switch (pinpointer.DistanceToTarget)
            {
                case Distance.Close:
                case Distance.Medium:
                case Distance.Far:
                    _sprite.LayerSetRotation((uid, sprite), PinpointerLayers.Screen, angle);
                    break;
                default:
                    _sprite.LayerSetRotation((uid, sprite), PinpointerLayers.Screen, Angle.Zero);
                    break;
            }
        }
    }
}
