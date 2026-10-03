using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Extensions;
using Jotunn.Utils;

namespace HungarianNotation.PseudoDrop;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
public partial class Plugin : BaseUnityPlugin {

    public const string PluginGUID = "HungarianNotation.PseudoDrop";
    public const string PluginName = "PseudoDropFix";

    readonly Harmony harmony = new Harmony(PluginGUID);

    ConfigEntry<bool> config_Enable;
    ConfigEntry<bool> config_DoLogging;

    public static Plugin Instance { get; private set; }
    public static new ManualLogSource Logger { get; private set; }

    public static bool IsEnabled { get { return Instance.config_Enable.Value; } }
    public static bool IsLoggingEnabled { get { return Instance.config_DoLogging.Value; } }

    protected void Awake() {
        Instance = this;
        Logger = base.Logger;
        CreateConfig();
        harmony.PatchAll();

        Logger.LogDebug($"version={PluginVersion}");
    }

    void CreateConfig() {

        config_Enable = Config.BindConfig(
            "General", "Enable",
            true,
            "Enables/Disables the entire mod.",
            order: 0,
            synced: true);

        config_DoLogging = Config.BindConfig(
            "General", "DebugLogging",
            false,
            "Enables debug logging that writes drop chances and rolled intervals to the console. Not recommended for normal play.",
            order: 1,
            synced: true);

        config_Enable.SettingChanged += Config_ConfigReloaded;
        config_DoLogging.SettingChanged += Config_ConfigReloaded;
    }

    private void Config_ConfigReloaded(object sender, System.EventArgs e) {
        base.Logger.LogInfo($"config changed (enabled={config_Enable.Value}, logging={config_DoLogging.Value})");
    }
}