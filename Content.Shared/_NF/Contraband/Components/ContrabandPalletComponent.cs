using Content.Shared.Stacks;
using Robust.Shared.Audio; // Aurora's Song
using Robust.Shared.Prototypes; // Aurora's Song
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._NF.Contraband.Components;

[RegisterComponent] // Aurora's Song - Contraband Pallet component for contraband registration and licensing
[Access(typeof(SharedContrabandTurnInSystem))]
public sealed partial class ContrabandPalletConsoleComponent : Component
{
    // Aurora's Song
    /// <summary>
    /// The primary currency that should be reward. Tries to send it to an entity with a <see cref="ScuOutputComponent"/> first, then the triggering entities hand, and if both of those fail, spawns the coins on the console.
    /// Also determines what currency is given as a registration reward.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("cashType", serverOnly: true, customTypeSerializer: typeof(PrototypeIdSerializer<StackPrototype>))]
    public string? RewardType = null;

    // Aurora's Song
    /// <summary>
    /// The reward that should be sent to the triggering entities hand, or spawn on the console if it can't
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("altCashType", serverOnly: true, customTypeSerializer: typeof(PrototypeIdSerializer<StackPrototype>))]
    public string? RewardTypeAlternate = null; // AS: Allow alt reward currencies

    // Aurora's Song
    [DataField]
    public SoundSpecifier ErrorSound = new SoundCollectionSpecifier("CargoError");

    // Aurora's Song
    /// <summary>
    /// Leave null for no licence required
    /// </summary>
    [DataField]
    public string? LicenseRequired = "contraband handling license";

    [ViewVariables(VVAccess.ReadWrite), DataField(serverOnly: true)]
    public string Faction = "SLE"; // Aurora's Song - Changed from "NFSD" to "SLE" (Station Law Enforcement)

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public string LocStringPrefix = string.Empty;

    // Aurora's Song
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int PalletDistance = 8;

    // Aurora's Song
    [DataField]
    public Dictionary<EntProtoId, EntProtoId> RegisterRecipies = new()
    {
        {"NFWeaponPistolMk58Expedition","NFWeaponPistolMk58"},
        {"NFWeaponPistolPollockExpedition","NFWeaponPistolPollockHighCapacityMag"},
        {"NFWeaponPistolUniversalExpedition","ASWeaponPistolUniversal"},
        {"NFWeaponRevolverArgentiExpedition","NFWeaponRevolverArgenti"},
        {"NFWeaponRevolverFaithExpedition","NFWeaponRevolverFaith"},
        {"NFWeaponRevolverDeckardExpedition","NFWeaponRevolverDeckard"},
        {"NFWeaponRevolverPython45Expedition","NFWeaponRevolverPython45"},
        {"NFWeaponRifleBarlowsBoltExpedition","NFWeaponRifleBarlowsBolt"},
        {"NFWeaponRifleCeremonialExpedition","NFWeaponRifleCeremonial"},
        {"NFWeaponRifleRepeaterExpedition","NFWeaponRifleRepeater"},
        {"NFWeaponPistolCobraExpedition","NFWeaponPistolCobra"},
        {"NFWeaponPistolN1984Expedition","NFWeaponPistolN1984"},
        {"NFWeaponPistolViperExpedition","NFWeaponPistolViper"},
        {"NFWeaponRevolverFitzExpedition","NFWeaponRevolverFitz"},
        {"NFWeaponRevolverLuckyExpedition","NFWeaponRevolverLucky"},
        {"NFWeaponRevolverPirateExpedition","NFWeaponRevolverPirate"},
        {"NFWeaponRevolverWard45Expedition","NFWeaponRevolverWard45"},
        {"NFWeaponShotgunKammererExpedition","NFWeaponShotgunKammerer"},
        {"NFWeaponSubMachineGunC20rExpedition","NFWeaponSubMachineGunC20r"},
        {"NFWeaponRifleAssaultNovaliteC1Expedition","NFWeaponRifleAssaultNovaliteC1"},
        {"NFWeaponRifleAssaultJackdawExpedition","NFWeaponRifleAssaultJackdaw"},
        {"NFWeaponRifleAssaultGestioExpedition","NFWeaponRifleAssaultGestio"},
        {"NFWeaponRifleSVSExpedition","NFWeaponRifleSVS"},
        {"NFWeaponEnergyPistolLaserExpedition","NFWeaponEnergyPistolLaser"},
        {"NFWeaponRifleAssaultLecterExpedition","ASWeaponRifleAssaultSurpLecter"},
        {"NFWeaponRifleAssaultM90Expedition","NFWeaponRifleAssaultM90"},
        {"NFWeaponRifleSniperHristovExpedition","NFWeaponRifleSniperHristov"},
        {"NFWeaponRifleMusketExpedition","NFWeaponRifleMusket"},
        {"NFWeaponSubMachineGunWt550Expedition","ASWeaponSubMachineGunSurpWt550"},
        {"NFWeaponSubMachineGunDrozdExpedition","ASWeaponSubMachineGunSurpDrozd"},
        {"NFWeaponSubMachineGunAtreidesExpedition","ASWeaponSubMachineGunSurpAtreides"},
        {"NFWeaponSubMachineGunTypewriterExpedition","NFWeaponSubMachineGunTypewriter"},
        {"NFWeaponEnergyPistolLaserSvalinnExpedition","NFWeaponEnergyPistolLaserSvalinn"},
        {"NFWeaponShotgunEnforcerExpedition","NFWeaponShotgunEnforcer"},
        {"NFWeaponShotgunBulldogExpedition","NFWeaponShotgunBulldog"},
        {"NFWeaponRifleAssaultSmExpedition","ASWeaponRifleAssaultSurpSm"},
        {"NFWeaponRifleAssaultVulcanExpedition","NFWeaponRifleAssaultVulcan"},
        {"NFWeaponEnergyRifleCarbineExpedition","NFWeaponEnergyRifleCarbine"},
        {"NFWeaponEnergyPistolLaserAdvancedExpedition","NFWeaponEnergyPistolLaserAdvanced"},
        {"NFWeaponEnergyPistolLaserAntiqueExpedition","NFWeaponEnergyPistolLaserAntique"},
        {"NFWeaponEnergySubMachineGunDeltaVExpedition","NFWeaponEnergySubMachineGunDeltaV"},
        {"NFWeaponEnergyRifleSniperXrayCannonExpedition","NFWeaponEnergyRifleSniperXrayCannon"},
        {"NFWeaponEnergyRifleSniperCannonExpedition","NFWeaponEnergyRifleSniperCannon"},
        {"NFWeaponEnergyRifleTemperatureExpedition","NFWeaponEnergyRifleTemperature"},
        {"ASWeaponRifleAssaultEstocExpedition","ASWeaponRifleAssaultEstoc"},
        {"ASWeaponKasyreLasRifleExpedition","ASWeaponKasyreLasRifle"},
        {"ASWeaponBasinLasPistolExpedition","ASWeaponBasinLasPistol"},
        {"ASWeaponSubMachineGunAP4CExpedition","ASWeaponSubMachineGunAP4C"},
        {"ASWeaponShotgunFalchionExpedition","ASWeaponShotgunSurpFalchion"},
        {"ASWeaponShotgunHushpupExpedition","ASWeaponShotgunHushpup"},

        {"ClothingOuterHardsuitSyndie","ClothingOuterHardsuitShanlinUnpainted"},
        {"ClothingOuterHardsuitSyndieElite","ClothingOuterHardsuitShiweiUnpainted"},

        {"ClothingShoesBootsMagSyndie","ClothingShoesBootsMagSyndieRegistered"},

        {"CrateSyndicateLockedHardsuitFilled","ClothingOuterHardsuitShanlinUnpainted"},
        {"CrateSyndicateLockedEliteHardsuitFilled","ClothingOuterHardsuitShiweiUnpainted"},
    };
}
