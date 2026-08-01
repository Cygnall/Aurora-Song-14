using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Forensics;
using Content.Shared.Forensics.Components;

namespace Content.Shared._AS.Traits;

/// <summary>
/// System that handles swapping blood reagents. Used with <see cref="BloodSwapComponent"/>.
/// </summary>
public sealed partial class BloodSwapSystem : EntitySystem
{
    [Dependency] private SharedBloodstreamSystem _bloodSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodSwapComponent, ComponentInit>(OnBloodSwapStartup);
    }

    private void OnBloodSwapStartup(EntityUid uid, BloodSwapComponent component, ComponentInit args)
    {
        // We need BloodstreamComponent to know the volume of the player's bloodstream. We also need BloodReagent to not be null
        if (!TryComp<BloodstreamComponent>(uid, out var bloodStream) || component.BloodReagent == null)
            return;

        var bloodVolume = bloodStream.BloodReferenceSolution.Volume;

        // Create a solution made from the reagent defined in BloodSwapComponent and the volume gotten from BloodstreamComponent.
        Solution bloodSolution = new(component.BloodReagent, bloodVolume);

        // Replace the player's blood with this solution.
        _bloodSystem.ChangeBloodReagents(uid, bloodSolution);

        if (!TryComp<DnaComponent>(uid, out var dnaComp) || dnaComp.DNA == null)
            return;

        // Regenerate the DNA in the solution so DNA properly gets added.
        var ev = new GenerateDnaEvent { Owner = uid, DNA = dnaComp.DNA };
        RaiseLocalEvent(uid, ref ev);
    }
}
