using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CharacterDrop;

[HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
public class CharacterDropPatch {

    public static void WriteLog(string text) {
#if DEBUG
        PseudoDropMod.Log.LogInfo(text);
#else
        PseudoDropMod.Log.LogDebug(text);
#endif
    }

    [HarmonyPrefix]
    public static void Prefix(CharacterDrop __instance) {
        WriteLog($"Intercepted GenerateDropList for {__instance.m_character.GetHoverName()} (Level {__instance.m_character.GetLevel()})");
        int levelMultiplier = ((!__instance.m_character) ? 1 : Mathf.Max(1, (int)Mathf.Pow(2f, __instance.m_character.GetLevel() - 1)));

        foreach (var drop in __instance.m_drops) {
            float dropChance = drop.m_chance;
            if (drop.m_levelMultiplier)
                dropChance *= levelMultiplier;
            if (dropChance <= 0.3) {
                string itemName = drop.m_prefab.name;

                if (!s_pseudoCounter.ContainsKey(itemName) || s_pseudoCounter[itemName].Item1 != dropChance) {
                    int injectedCounter = FirstInterval(drop.m_chance);
                    s_pseudoCounter[itemName] = new Tuple<float, int>(drop.m_chance, injectedCounter);
                    WriteLog($"Initial Drop Counter: item={itemName} chance={dropChance} counter={injectedCounter}");
                }
            }
        }
    }

    [HarmonyPostfix]
    public static void PostFix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result) {
        int levelMultiplier = ((!__instance.m_character) ? 1 : Mathf.Max(1, (int)Mathf.Pow(2f, __instance.m_character.GetLevel() - 1)));

        foreach (var drop in __instance.m_drops) {
            string itemName = drop.m_prefab.name;
            float dropChance = drop.m_chance;

            if (drop.m_levelMultiplier)
                dropChance *= levelMultiplier;

            if (dropChance > 0.3)
                continue;

            int extantCounter = s_pseudoCounter[itemName].Item2;

            if (!__result.Any(item => item.Key == drop.m_prefab)) {
                WriteLog($"Item will drop in {extantCounter} kill{(extantCounter != 1 ? "s" : "")}: item={itemName} chance={dropChance} counter={extantCounter}");
                continue;
            } else {
                var counter = SubsequentInterval(dropChance);
                s_pseudoCounter[itemName] = new Tuple<float, int>(dropChance, counter);
                WriteLog($"Confirmed Drop of {itemName} chance={dropChance}; Counter set to {counter} (was {extantCounter})");
            }
        }
    }

    public static int FirstInterval(float chance) {
        float realBound = (1.0f / chance * 2.0f) - 1f;
        int intBound = (int)realBound + (UnityEngine.Random.value <= realBound % 1 ? 1 : 0);
        int totalWeight = (intBound * (intBound + 1)) / 2;
        int randomSample = UnityEngine.Random.Range(1, totalWeight + 1);
        int discriminant = (int)Math.Pow(2 * intBound + 1, 2) - 8 * randomSample;
        float realInterval = ((2f * intBound + 1f) - (float)Math.Sqrt(discriminant)) / 2.0f;
        return Mathf.CeilToInt(realInterval);
    }

    public static int SubsequentInterval(float chance) {
        return UnityEngine.Random.Range(1, (int)(1.0f / chance * 2.0f));
    }
}