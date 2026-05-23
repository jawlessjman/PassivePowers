using System;
using HarmonyLib;
using PassivePowers.Data;

namespace PassivePowers.Patches;

/// <summary>
/// Harmony patch for the player.
/// </summary>
[HarmonyPatch]
public class PassivePowersPlayerPatch
{
    /// <summary>
    /// Maps the name of the guardian power to the name of the passive power effect.
    /// </summary>
    private static readonly System.Collections.Generic.Dictionary<string, string> PowerMapping = new()
    {
        { "GP_Eikthyr", "SE_PassiveEikthyrEffect" },
        { "GP_TheElder", "SE_PassiveElderEffect" },
        { "GP_Bonemass", "SE_PassiveBonemassEffect" },
        { "GP_Moder", "SE_PassiveModerEffect" },
        { "GP_Yagluth", "SE_PassiveYagluthEffect" },
        { "GP_Queen", "SE_PassiveQueenEffect" },
        { "GP_Fader", "SE_PassiveFaderEffect" }
    };
    
    /// <summary>
    /// Prevents the guardian power from being activated.
    /// </summary>
    /// <returns>If the power should be used</returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.StartGuardianPower))]
    private static bool StartGuardianPowerPrefix(Player __instance)
    {
        if (__instance == null) return false;

        var selectedPower = __instance.GetGuardianPowerName();

        return selectedPower switch
        {
            "GP_Eikthyr" => !Plugin.EikthyrEnabled.Value,
            "GP_TheElder" => !Plugin.ElderEnabled.Value,
            "GP_Bonemass" => !Plugin.BonemassEnabled.Value,
            "GP_Moder" => !Plugin.ModerEnabled.Value,
            "GP_Yagluth" => !Plugin.YagluthEnabled.Value,
            "GP_Queen" => !Plugin.QueenEnabled.Value,
            "GP_Fader" => !Plugin.FaderEnabled.Value,
            _ => false
        };
    }
    
    /// <summary>
    /// Adjusts the HUD for the forsaken power.
    /// </summary>
    /// <param name="se">Status effect</param>
    /// <param name="cooldown">cooldown</param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.GetGuardianPowerHUD))]
    private static bool GetGuardianPowerHudPrefix(Player __instance, out StatusEffect se, out float cooldown)
    {
        se = (StatusEffect)null;
        cooldown = 0.0f;
        
        if (__instance == null)
        {
            return false;
        }
        var currentPower = __instance.GetGuardianPowerName();
        
        return currentPower switch
        {
            "GP_Eikthyr" => !Plugin.EikthyrEnabled.Value,
            "GP_TheElder" => !Plugin.ElderEnabled.Value,
            "GP_Bonemass" => !Plugin.BonemassEnabled.Value,
            "GP_Moder" => !Plugin.ModerEnabled.Value,
            "GP_Yagluth" => !Plugin.YagluthEnabled.Value,
            "GP_Queen" => !Plugin.QueenEnabled.Value,
            "GP_Fader" => !Plugin.FaderEnabled.Value,
            _ => false
        };
    }
    
    /// <summary>
    /// Binds the passive power to the player's guardian power.'
    /// </summary>
    /// <param name="__instance">Player</param>
    /// <param name="name">Guardian power name</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.SetGuardianPower))]
    private static void SetGuardianPowerPostfix(Player __instance, string name)
    {
        try
        {
            if (__instance == null || string.IsNullOrEmpty(name)) return;
            if (!GetStatusEffect.Initialized) return;
            
            // Do not add status effects in the main menu (character selection)
            if (ZNetScene.instance == null) return;
            
            var seMan = __instance.GetSEMan();
            if (seMan == null) return;

            // Remove all passive powers
            foreach (var passiveName in PowerMapping.Values)
            {
                seMan.RemoveStatusEffect(passiveName.GetStableHashCode());
            }

            // Get the target passive power
            if (!PowerMapping.TryGetValue(name, out var targetPassiveName)) return;
            var targetEffect = GetStatusEffect.GetPassiveStatusEffect(targetPassiveName);

            // Add the passive power
            if (targetEffect != null)
            {
                Plugin.Logger.LogInfo($"Successfully bound passive layer: {targetPassiveName}");
                seMan.AddStatusEffect(targetEffect);
            }
            else
            {
                Plugin.Logger.LogWarning(
                    $"Target effect {targetPassiveName} was missing from the Jötunn ObjectDB registry! or the passive effect is not ready");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning(ex.ToString());
        }
    }
}