using Content.Client.Message;
using Content.Client.Stylesheets;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using Content.Shared.Pinpointer;
using Robust.Shared.Map;

namespace Content.Client.GPS.UI;

public sealed class PinpointerStatusControl : Control
{
    private readonly Entity<PinpointerComponent> _parent;
    private readonly RichTextLabel _label;
    private float _updateDif;
    private readonly IEntityManager _entMan;
    private readonly SharedTransformSystem _transform;

    public PinpointerStatusControl(Entity<PinpointerComponent> parent)
    {
        _parent = parent;
        _entMan = IoCManager.Resolve<IEntityManager>();
        _transform = _entMan.System<TransformSystem>();
        _label = new RichTextLabel { StyleClasses = { StyleClass.ItemStatus } };
        AddChild(_label);
        UpdatePinpointerDetails();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        // don't display the label if the gps component is being removed
        if (_parent.Comp.LifeStage > ComponentLifeStage.Running)
        {
            _label.Visible = false;
            return;
        }

        _updateDif += args.DeltaSeconds;
        if (_updateDif < _parent.Comp.UpdateRate)
            return;

        _updateDif -= _parent.Comp.UpdateRate;

        UpdatePinpointerDetails();
    }

    private void UpdatePinpointerDetails()
    {
        if (_parent.Comp.Target != null)
        {
            var posText = $"({(int)_parent.Comp.TargetCoordinates.X}, {(int)_parent.Comp.TargetCoordinates.Y})";
            _label.SetMarkup(Loc.GetString("handheld-gps-coordinates-title", ("coordinates", posText)));
        }
    }
}
