
using Content.Shared.Atmos.Components;

namespace Content.Shared.Atmos.EntitySystems;

public partial class GasMaxPressureSystem<T> : EntitySystem where T : IGasMaxPressureHolder, IComponent
{
    /// <see cref="ToggleVacate(Entity{T},bool,EntityUid?)"/>
    public void ToggleVacate(Entity<T> entity, EntityUid? user = null)
    {
        ToggleVacate(entity, !entity.Comp.Vacate, user);
    }

    /// <summary>
    /// Toggles the vacation pump for this <see cref="T"/> on or off
    /// </summary>
    /// <param name="entity">Entity whose vacation pump we're toggling</param>
    /// <param name="vacate">Whether we're turning on or off the pump</param>
    /// <param name="user">Optional user who is performing the action.</param>
    public void ToggleVacate(Entity<T> entity, bool vacate, EntityUid? user = null)
    {
        entity.Comp.Vacate = vacate;
        Audio.PlayPredicted(entity.Comp.ValveSound, entity, user);
        Dirty(entity);
    }
}
