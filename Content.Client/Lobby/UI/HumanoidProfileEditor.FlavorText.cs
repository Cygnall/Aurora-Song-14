using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private bool _allowFlavorText;

    private FlavorText.FlavorText? _flavorText;
    private TextEdit? _flavorSfwTextEdit; // DEN
    private TextEdit? _flavorNsfwTextEdit; // DEN
    private TextEdit? _characterConsent; // DEN

    /// <summary>
    /// Refreshes the flavor text editor status.
    /// </summary>
    public void RefreshFlavorText()
    {
        if (_allowFlavorText)
        {
            if (_flavorText != null)
                return;

            // Start TheDen
            _flavorText = new();

            _flavorText.OnSfwFlavorTextChanged += OnSfwFlavorTextChange;
            _flavorText.OnNsfwFlavorTextChanged += OnNsfwFlavorTextChange;
            _flavorText.OnCharacterConsentChanged += OnCharacterConsentChange;

            _flavorSfwTextEdit = _flavorText.CFlavorTextSFWInput;
            _flavorNsfwTextEdit = _flavorText.CFlavorTextNSFWInput;
            _characterConsent = _flavorText.CFlavorTextConsentInput;

            TabContainer.AddChild(_flavorText);
            TabContainer.SetTabTitle(4, Loc.GetString("humanoid-profile-editor-flavortext-tab"));
            // End TheDen
        }
        else
        {
            if (_flavorText == null)
                return;

            TabContainer.RemoveChild(_flavorText);
            // Start TheDen
            _flavorText.OnSfwFlavorTextChanged -= OnSfwFlavorTextChange;
            _flavorText.OnNsfwFlavorTextChanged -= OnNsfwFlavorTextChange;
            _flavorText.OnCharacterConsentChanged -= OnCharacterConsentChange;
            // End TheDen
            _flavorText.Dispose();
            // Start TheDen
            _flavorSfwTextEdit?.Dispose();
            _flavorNsfwTextEdit?.Dispose();
            _characterConsent?.Dispose();
            // End TheDen
            _flavorText = null;
            // Start TheDen
            _flavorSfwTextEdit = null;
            _flavorNsfwTextEdit = null;
            _characterConsent = null;
            // End TheDen
        }
    }

    private void OnSfwFlavorTextChange(string content) // TheDen - Change to Sfw
    {
        if (Profile is null)
            return;

        Profile = Profile.WithFlavorText(content);
        SetDirty();
    }

    // TheDen
    private void OnNsfwFlavorTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithNsfwFlavorText(content);
        SetDirty();
    }

    // TheDen
    private void OnCharacterConsentChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithCharacterConsent(content);
        SetDirty();
    }

    private void UpdateFlavorTextEdit()
    {
        // Start TheDen
        if (_flavorSfwTextEdit != null)
            _flavorSfwTextEdit.TextRope = new Rope.Leaf(Profile?.FlavorText ?? "");

        if (_flavorNsfwTextEdit != null)
            _flavorNsfwTextEdit.TextRope = new Rope.Leaf(Profile?.NsfwFlavorText ?? "");

        if (_characterConsent != null)
            _characterConsent.TextRope = new Rope.Leaf(Profile?.CharacterConsent ?? "");
        // End TheDen
    }
}
