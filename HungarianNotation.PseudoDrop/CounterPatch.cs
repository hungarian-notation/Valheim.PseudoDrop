using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CharacterDrop;

namespace HungarianNotation.PseudoDrop;

[HarmonyPatch]
[HarmonyWrapSafe]
public class CounterPatch {

    public static void WriteLog(string text) {
        if (Plugin.IsLoggingEnabled)
            Plugin.Logger.LogInfo(text);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
    public static void GenerateDropList_Prefix(CharacterDrop __instance) {
        if (!Plugin.IsEnabled)
            return;

        WriteLog($"Intercepted GenerateDropList for {__instance.m_character.GetHoverName()} (Level {__instance.m_character.GetLevel()})");

        //var lastHit = __instance?.m_character?.m_lastHit;
        //var lastAttacker = lastHit?.GetAttacker();

        //if (lastAttacker && lastAttacker.IsPlayer() && lastAttacker is Player player) {
        //    string playerName = player.GetPlayerName();
        //    var playerId = lastAttacker.IsPlayer() ? ((Player)lastAttacker).GetPlayerID().ToString() : null;
        //    WriteLog($"\tKilled by player: {playerName}");
        //}

        int levelMultiplier = GetLevelMultiplier(__instance);

        foreach (var drop in __instance.m_drops) {
            float dropChance = drop.m_chance;
            if (drop.m_levelMultiplier)
                dropChance *= levelMultiplier;
            if (dropChance <= 0.3) {
                string itemName = drop.m_prefab.name;

                if (!s_pseudoCounter.ContainsKey(itemName) || s_pseudoCounter[itemName].Item1 != dropChance) {
                    if (s_pseudoCounter.ContainsKey(itemName)) {
                        WriteLog(
                            $"Discarding counter with mismatched drop chance: " +
                            $"chance={s_pseudoCounter[itemName].Item1} counter={s_pseudoCounter[itemName].Item2}");
                    }

                    int injectedCounter = FirstInterval(drop.m_chance);
                    s_pseudoCounter[itemName] = new Tuple<float, int>(drop.m_chance, injectedCounter);
                    WriteLog($"Setting initial drop counter: item={itemName} chance={dropChance} counter={injectedCounter}");
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
    public static void GenerateDropList_Postfix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result) {
        if (!Plugin.IsEnabled)
            return;

        int levelMultiplier = GetLevelMultiplier(__instance);

        foreach (var drop in __instance.m_drops) {
            string itemName = drop.m_prefab.name;

            float dropChance = GetDropChance(drop, levelMultiplier);

            if (dropChance > 0.3)
                continue;

            int extantCounter = s_pseudoCounter[itemName].Item2;

            if (!__result.Any(item => item.Key == drop.m_prefab)) {
                WriteLog($"Item \"{itemName}\" will drop in {extantCounter} kill{(extantCounter != 1 ? "s" : "")}; chance={dropChance} counter={extantCounter}");
                continue;
            } else {
                var counter = SubsequentInterval(dropChance);
                s_pseudoCounter[itemName] = new Tuple<float, int>(dropChance, counter);
                WriteLog($"Confirmed drop of {itemName} chance={dropChance}; Counter set to {counter} (was {extantCounter})");
            }
        }

    }

    private static float GetDropChance(Drop drop, int levelMultiplier) {
        float dropChance = drop.m_chance;
        if (drop.m_levelMultiplier)
            dropChance *= levelMultiplier;
        return dropChance;
    }

    private static int GetLevelMultiplier(CharacterDrop characterDrop) {
        return ((!characterDrop.m_character) ? 1 : Mathf.Max(1, (int)Mathf.Pow(2f, characterDrop.m_character.GetLevel() - 1)));
    }

    //[HarmonyTranspiler]
    //[HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
    //public static IEnumerable<CodeInstruction> MyTranspiler(IEnumerable<CodeInstruction> instructions) {
    //    var indexProperty = typeof(Dictionary<string, Tuple<float, int>>).GetProperty("Item");
    //    var indexSetter = indexProperty?.GetSetMethod();
    //    var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Callvirt, indexSetter)).Repeat(matcher => {
    //        Plugin.Logger.LogError($"Matched {matcher.Instruction} at {matcher.Pos}");
    //        matcher.Advance(1);
    //    });

    //    return matcher.InstructionEnumeration();
    //}

    public static int FirstInterval(float chance) {
        float realBound = (1.0f / chance * 2.0f) - 1f;
        int intBound = (int)realBound + (UnityEngine.Random.value <= realBound % 1 ? 1 : 0);

        int totalWeight = (intBound * (intBound + 1)) / 2;
        int randomSample = UnityEngine.Random.Range(1, totalWeight + 1);
        int discriminant = (int)Math.Pow(2 * intBound + 1, 2) - 8 * randomSample;
        float realInterval = ((2f * intBound + 1f) - Mathf.Sqrt(discriminant)) / 2.0f;
        return Mathf.CeilToInt(realInterval);
    }

    public static int SubsequentInterval(float chance) {
        float realBound = (1.0f / chance * 2.0f);
        int intBound = (int)realBound + (UnityEngine.Random.value <= realBound % 1 ? 1 : 0);
        return UnityEngine.Random.Range(1, intBound);
    }
}