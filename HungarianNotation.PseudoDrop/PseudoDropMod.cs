using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
public class PseudoDropMod : BaseUnityPlugin {
    internal static new ManualLogSource Log;

    public const string PluginGUID = "HungarianNotation.PseudoDrop";
    public const string PluginName = "PseudoDropFix";
    public const string PluginVersion = "1.0.0";

    Harmony harmony = new Harmony(PluginGUID);

    void Awake() {
        Log = base.Logger;
        harmony.PatchAll();
        Logger.LogDebug($"Running {PluginName} Startup Tests");
        StartupTest(0.3f);
        StartupTest(0.1f);
    }

    private void StartupTest(float chance) {
        int drops = 0;
        int trials = 100000;

        var values = new Dictionary<int, int>();

        for (int i = 0; i < trials; ++i) {
            int interval = CharacterDropPatch.FirstInterval(chance);

            if (interval <= 1) {
                ++drops;
            }

            if (values.ContainsKey(interval)) {
                values[interval] += 1;
            } else {
                values[interval] = 1;
            }
        }

        Logger.LogMessage($"Startup Test (Rate: {chance}) {drops} / {trials} = {((float)drops / (float)trials):P} (should be around {chance:P})");

        int min = values.Keys.Min();
        int max = values.Keys.Max();

        for (int i = min; i <= max; ++i) {
            if (values.TryGetValue(i, out var count)) {
                Logger.LogDebug($"  {i} = {count} {count / (float)trials:P}");
            } else {
                Logger.LogDebug($"  {i} = {0} {0f:P}");
            }
        }
    }
}