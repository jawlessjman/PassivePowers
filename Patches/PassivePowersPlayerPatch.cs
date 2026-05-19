using System;
using HarmonyLib;
using PassivePowers.Data;
using Object = UnityEngine.Object;

namespace PassivePowers.Patches;

/// <summary>
/// Harmony patch for the player.
/// </summary>
[HarmonyPatch]
public class PassivePowersPlayerPatch
{
    private static readonly int ModerHash = "GP_Moder".GetStableHashCode();
    private static readonly int PassiveModerHash = "SE_PassiveModerEffect".GetStableHashCode();
    
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
    private static bool GetGuardianPowerHudPrefix(out StatusEffect se, out float cooldown)
    {
        //Plugin.Logger.LogInfo("Reset cooldown and removed status effect from forsaken power");
        se = (StatusEffect)null;
        cooldown = 0.0f;
        return false;
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
            Plugin.Logger.LogError(ex.ToString());
        }
    }
    
    /// <summary>
    /// Adds the moder effect when the player starts controlling the boat.
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.StartDoodadControl))]
    public static void StartDoodadControlPostfix(Player __instance)
    {
        if (__instance == null) return;
        if (!((Object)Player.m_localPlayer != (Object)null)) return;
        
        var seMan = __instance.GetSEMan();
        if (seMan == null) return;
        if (!seMan.HaveStatusEffect(PassiveModerHash) || seMan.HaveStatusEffect(ModerHash)) return;
        
        // Create the moder effect
        var infiniteModerEffect = ObjectDB.instance.GetStatusEffect(ModerHash);
        if (infiniteModerEffect == null) return;

        infiniteModerEffect.m_ttl = 0.0f;
        infiniteModerEffect.m_name = "$passive_moder_sail_effect";
        infiniteModerEffect.m_startMessage = "$passive_moder_sail_start";
        infiniteModerEffect.m_tooltip = "$passive_moder_sail_tooltip";
        
        //Remove the moder effect stats
        if (infiniteModerEffect is SE_Stats stats)
        {
            stats.m_addMaxCarryWeight = 0.0f;
            stats.m_speedModifier = 0.0f;
            stats.m_mods =
            [
                new HitData.DamageModPair()
                    { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal }
            ];
        }
        
        seMan.AddStatusEffect(ModerHash);
    }
    
    /// <summary>
    /// Remove the moder effect when the player stops controlling the boat.
    /// </summary>
    /// <param name="__instance">Player</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.StopDoodadControl))]
    public static void StopDoodadControlPostfix(Player __instance)
    {
        if (__instance == null) return;
        if (!((Object)Player.m_localPlayer != (Object)null)) return;
        
        var seMan = __instance.GetSEMan();
        if (seMan == null) return;
        if (!seMan.HaveStatusEffect(PassiveModerHash)) return;
        
        if (!seMan.HaveStatusEffect(ModerHash)) return;
        seMan.RemoveStatusEffect(ModerHash);
    }
}