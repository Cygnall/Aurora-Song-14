namespace Content.Shared.Metabolism;

public sealed partial class MetabolizerComponent
{
    [DataField]
    public float EfficiencyModifier = 1f;

    [DataField]
    public float RateModifier = 1f;
}
