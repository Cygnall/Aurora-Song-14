using Content.Shared.Humanoid.Prototypes;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private new void SetHeight(float newHeight)
    {
        if (Profile != null &&
            _prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            newHeight = Math.Clamp(newHeight, species.MinHeight, species.MaxHeight);

            var appearance = Profile.Appearance.WithHeight(newHeight);
            Profile = Profile.WithCharacterAppearance(appearance);
        }

        SetDirty();
        ReloadPreview();
        UpdateSpriteViewScale();
    }

    private void ResetHeight()
    {
        if (Profile != null &&
            _prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            var midpoint = (species.MinHeight + species.MaxHeight) / 2f;
            SetHeight(midpoint);

            if (HeightSlider != null)
                HeightSlider.Value = midpoint;
        }

        UpdateHeightControls();
    }

    private new void SetWidth(float newWidth)
    {
        if (Profile != null &&
            _prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            newWidth = Math.Clamp(newWidth, species.MinWidth, species.MaxWidth);

            var appearance = Profile.Appearance.WithWidth(newWidth);
            Profile = Profile.WithCharacterAppearance(appearance);
        }

        SetDirty();
        ReloadPreview();
        UpdateSpriteViewScale();
    }

    private void ResetWidth()
    {
        if (Profile != null &&
            _prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            var midpoint = (species.MinWidth + species.MaxWidth) / 2f;
            SetWidth(midpoint);

            if (WidthSlider != null)
                WidthSlider.Value = midpoint;
        }

        UpdateWidthControls();
    }

    private void UpdateHeightControls()
    {
        if (Profile == null || HeightSlider == null)
            return;

        if (_prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            HeightSlider.MinValue = species.MinHeight;
            HeightSlider.MaxValue = species.MaxHeight;

            // Clamp profile value to species range
            var clamped = Math.Clamp(Profile.Appearance.Height, species.MinHeight, species.MaxHeight);
            HeightSlider.Value = clamped;
        }
    }

    private void UpdateWidthControls()
    {
        if (Profile == null || WidthSlider == null)
            return;

        if (_prototypeManager.TryIndex<SpeciesPrototype>(Profile.Species, out var species))
        {
            WidthSlider.MinValue = species.MinWidth;
            WidthSlider.MaxValue = species.MaxWidth;
            // Clamp profile value to species range
            var clamped = Math.Clamp(Profile.Appearance.Width, species.MinWidth, species.MaxWidth);
            WidthSlider.Value = clamped;
        }
    }
}
