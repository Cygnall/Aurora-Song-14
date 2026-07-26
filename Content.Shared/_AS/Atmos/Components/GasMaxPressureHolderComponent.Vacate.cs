namespace Content.Shared.Atmos.Components;

public partial class GasMaxPressureHolderComponent
{
    /// <summary>
    /// This pumps air out of the cans when in atmosphere. Rather than equalizing
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Vacate { get; set; } = true;
}
