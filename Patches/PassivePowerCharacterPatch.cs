using System.Linq;
using HarmonyLib;
using PassivePowers.Data;

namespace PassivePowers.Patches;

/// <summary>
/// Harmony patch for the character.
/// </summary>
[HarmonyPatch]
public class PassivePowerCharacterPatch
{
  /// <summary>
  /// Hash code for the bonemass status effect.
  /// </summary>
  private static readonly int BonemassHash = "SE_PassiveBonemassEffect".GetStableHashCode();
  
  /// <summary>
  /// Applies the damage reduction for the different passive powers
  /// </summary>
  /// <param name="__instance">Character</param>
  /// <param name="hit">hit</param>
  [HarmonyPrefix]
  [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
  private static void ApplyDamagePrefix(Character __instance, ref HitData hit)
  {
    if (__instance is not Player player) return;

    var effects = player.GetSEMan().GetStatusEffects();
    if (effects == null) return;

    foreach (var effect in effects.Where(effect => effect.name.StartsWith("SE")))
    {
      // Check if the effect is a passive power
      switch (effect.name.Replace("(Clone)", ""))
      {
        case "SE_PassiveBonemassEffect":
          hit.m_damage.m_blunt *= 1f - (ForsakenPowerStats.BonemassDamageReduction * Plugin.PowerAmount.Value);
          hit.m_damage.m_slash *= 1f - (ForsakenPowerStats.BonemassDamageReduction * Plugin.PowerAmount.Value);
          hit.m_damage.m_pierce *= 1f - (ForsakenPowerStats.BonemassDamageReduction * Plugin.PowerAmount.Value);
          break;
        case "SE_PassiveModerEffect":
          hit.m_damage.m_frost *= 1f - (ForsakenPowerStats.ModerFrostResistance * Plugin.PowerAmount.Value);
          break;
        case "SE_PassiveYagluthEffect":
          hit.m_damage.m_lightning *= 1f - (ForsakenPowerStats.YagluthLightningResistance * Plugin.PowerAmount.Value);
          break;
        case "SE_PassiveQueenEffect":
          hit.m_damage.m_poison *= 1f - (ForsakenPowerStats.QueenPosionResistance * Plugin.PowerAmount.Value);
          break;
        case "SE_PassiveFaderEffect":
          hit.m_damage.m_fire *= 1f - (ForsakenPowerStats.FaderFireResistance * Plugin.PowerAmount.Value);
          break;
      }
    }
  }
  
  /// <summary>
  /// Applies the bonemass block stamina bonus
  /// </summary>
  /// <param name="__instance">Character</param>
  /// <param name="hit">hit</param>
  [HarmonyPostfix]
  [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
  private static void DamagePostfix(Character __instance, ref HitData hit)
  {
    if (__instance is not Player player) return;
    
    if (!player.IsBlocking())
    {
      return;
    }

    if (!hit.m_blockable)
    {
      return;
    }

    if (!player.GetSEMan().HaveStatusEffect(BonemassHash))
    {
      return;
    }
    
    player.AddStamina(ForsakenPowerStats.BonemassBlockStaminaBonus * Plugin.PowerAmount.Value);
  }
}