using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PassivePowers.Data;
using UnityEngine;

namespace PassivePowers.Patches;

[HarmonyPatch]
public class ItemStandPatch
{
    private const string PowersSaveKey =
        $"{Plugin.ModGuid}.selectedPassivePowers.v1";

    private static HashSet<string> ReadSelectedPowers(Player player)
    {
        if (!player.m_customData.TryGetValue(PowersSaveKey, out var saved))
            return new HashSet<string>(StringComparer.Ordinal);

        return new HashSet<string>(
            saved.Split([';'], StringSplitOptions.RemoveEmptyEntries),
            StringComparer.Ordinal);
    }

    private static void WriteSelectedPowers(
        Player player,
        HashSet<string> powers)
    {
        // Keep the key even when empty: "none selected" is valid saved data.
        player.m_customData[PowersSaveKey] =
            string.Join(";", powers.OrderBy(name => name));
    }

    private static bool TogglePassivePower(Player player, string guardianName)
    {
        if (!PassivePowersPlayerPatch.PowerMapping.TryGetValue(guardianName, out var passiveName))
            return false;

        var seMan = player.GetSEMan();
        if (seMan == null)
            return false;

        var selected = ReadSelectedPowers(player);
        var hash = passiveName.GetStableHashCode();

        if (selected.Remove(guardianName))
        {
            seMan.RemoveStatusEffect(hash);
            WriteSelectedPowers(player, selected);
            return true;
        }

        var effect = GetStatusEffect.GetPassiveStatusEffect(passiveName);

        if (effect == null)
        {
            Plugin.Logger.LogWarning($"Passive effect is not ready: {passiveName}");
            return false;
        }

        if (!seMan.HaveStatusEffect(hash))
        {
            seMan.AddStatusEffect(effect);

            if (!seMan.HaveStatusEffect(hash))
                return false;
        }

        selected.Add(guardianName);
        WriteSelectedPowers(player, selected);
        return true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game), "SpawnPlayer")]
    private static void SpawnPlayerPostfix()
    {
        var player = Player.m_localPlayer;

        if (player != null && ZNetScene.instance != null)
            player.StartCoroutine(RestorePassivePowers(player));
    }

    private static IEnumerator RestorePassivePowers(Player player)
    {
        // Give spawning/loading a frame to complete.
        yield return null;

        while (player &&
               player == Player.m_localPlayer &&
               ZNetScene.instance)
        {
            if (!Plugin.MultiplePowersEnabled.Value)
                yield break;

            var seMan = player.GetSEMan();

            if (!GetStatusEffect.Initialized || seMan == null)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            var retry = false;

            foreach (var guardianName in ReadSelectedPowers(player))
            {
                // Preserve unknown saved names, but do not apply them.
                if (!PassivePowersPlayerPatch.PowerMapping.TryGetValue(guardianName, out var passiveName))
                    continue;

                var hash = passiveName.GetStableHashCode();

                if (seMan.HaveStatusEffect(hash))
                    continue;

                var effect =
                    GetStatusEffect.GetPassiveStatusEffect(passiveName);

                if (!effect)
                {
                    retry = true;
                    continue;
                }

                seMan.AddStatusEffect(effect);

                if (!seMan.HaveStatusEffect(hash))
                    retry = true;
            }

            if (!retry)
                yield break;

            yield return new WaitForSeconds(0.5f);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.GetHoverText))]
    private static void ItemStandHoverTextPostfix(
        ItemStand __instance,
        StatusEffect ___m_guardianPower,
        ref string __result)
    {
        var player = Player.m_localPlayer;

        if (player == null ||
            ___m_guardianPower == null ||
            !__instance.HaveAttachment() ||
            !PassivePowersPlayerPatch.PowerMapping.TryGetValue(
                ___m_guardianPower.name, out var passiveName))
        {
            return;
        }

        var passiveEffect =
            GetStatusEffect.GetPassiveStatusEffect(passiveName);

        if (passiveEffect == null)
            return;

        var description = Localization.instance.Localize(
            passiveEffect.GetTooltipString());

        // Single-power mode: preserve vanilla's name, interaction prompt,
        // and already-selected text. Replace only its effect description.
        if (!Plugin.MultiplePowersEnabled.Value)
        {
            var vanillaDescription = Localization.instance.Localize(
                ___m_guardianPower.GetTooltipString());

            if (!string.IsNullOrEmpty(vanillaDescription))
            {
                __result = __result.Replace(
                    vanillaDescription,
                    description);
            }

            return;
        }

        // Multiple-power mode: use your custom add/remove interaction.
        var selected = ReadSelectedPowers(player)
            .Contains(___m_guardianPower.name);

        var action = selected ? "$remove_power" : "$add_power";

        __result = Localization.instance.Localize(
        $"<color=orange><b>{___m_guardianPower.m_name}</b></color>\n" +
            $"{description}\n\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] {action}");
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemStand), "IsGuardianPowerActive")]
    private static void IsGuardianPowerActivePostfix(
        StatusEffect ___m_guardianPower,
        ref bool __result)
    {
        var player = Player.m_localPlayer;

        if (!Plugin.MultiplePowersEnabled.Value ||
            player == null ||
            ___m_guardianPower == null ||
            !PassivePowersPlayerPatch.PowerMapping.ContainsKey(___m_guardianPower.name))
        {
            return;
        }

        __result = ReadSelectedPowers(player)
            .Contains(___m_guardianPower.name);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemStand), "Interact")]
    private static bool ItemStandInteractPrefix(
        ItemStand __instance,
        Humanoid user,
        bool hold,
        bool alt,
        StatusEffect ___m_guardianPower,
        ref bool __result)
    {
        if (!Plugin.MultiplePowersEnabled.Value)
            return true;

        if (___m_guardianPower == null)
            return true;

        var guardianName = ___m_guardianPower.name;

        if (!PassivePowersPlayerPatch.PowerMapping.ContainsKey(guardianName))
            return true;

        // Preserve trophy placement and alternate interactions.
        if (!__instance.HaveAttachment() || alt)
            return true;

        // Holding the interaction key must not repeatedly toggle the power.
        if (hold)
        {
            __result = false;
            return false;
        }

        var player = user as Player;

        if (player == null ||
            player != Player.m_localPlayer ||
            ZNetScene.instance == null ||
            !GetStatusEffect.Initialized)
        {
            __result = false;
            return false;
        }

        __result = TogglePassivePower(player, guardianName);
        return false;
    }
}