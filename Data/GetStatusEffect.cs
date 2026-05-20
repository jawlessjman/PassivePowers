using UnityEngine;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;

namespace PassivePowers.Data;

/// <summary>
/// Class that creates and registers all passive powers.
/// </summary>
public static class GetStatusEffect
{
    /// <summary>
    /// Dictionary that maps the name of the passive power to the status effect.
    /// </summary>
    private static Dictionary<string, CustomStatusEffect> _effects;
    
    /// <summary>
    /// Flag that indicates if the class has been initialized.
    /// </summary>
    public static bool Initialized = false;

    /// <summary>
    /// Gets the status effect for a given passive power.
    /// </summary>
    /// <param name="name">Name of the passive power</param>
    /// <returns>Status effect or null</returns>
    public static StatusEffect GetPassiveStatusEffect(string name)
    {
        if (!Initialized) return null;
        if (string.IsNullOrEmpty(name)) return null;
        if (ObjectDB.instance == null) return null;
        if (_effects == null) return null;
        if (ObjectDB.instance.m_StatusEffects == null) return null;

        switch (name)
        {
            case "SE_PassiveEikthyrEffect":
                if (!Plugin.EikthyrEnabled.Value) return null;
                break;
            case "SE_PassiveElderEffect":
                if (!Plugin.ElderEnabled.Value) return null;
                break;
            case "SE_PassiveBonemassEffect":
                if (!Plugin.BonemassEnabled.Value) return null;
                break;
            case "SE_PassiveModerEffect":
                if (!Plugin.ModerEnabled.Value) return null;
                break;
            case "SE_PassiveYagluthEffect":
                if (!Plugin.YagluthEnabled.Value) return null;
                break;
            case "SE_PassiveQueenEffect":
                if (!Plugin.QueenEnabled.Value) return null;
                break;
            case "SE_PassiveFaderEffect":
                if (!Plugin.FaderEnabled.Value) return null;
                break;
        }
        
        return _effects.TryGetValue(name, out var effect) ? effect.StatusEffect : null;
    }
    
    /// <summary>
    /// Registers all passive powers.
    /// </summary>
    public static void RegisterAllPassivePowers()
    {
        string[] powerNames = ["GP_Eikthyr", "GP_TheElder", "GP_Bonemass", "GP_Moder", "GP_Yagluth", "GP_Queen", "GP_Fader"
        ];
        
        _effects = new Dictionary<string, CustomStatusEffect>();
        
        foreach (var power in powerNames)
        {
            var effectInstance = CreatePassivePowerInstance(power);
            if (effectInstance == null)
            {
                Plugin.Logger.LogError($"Failed to create instance for passive power {power}");
                continue;
            }
            // Wrap and safely add to Valheim's ObjectDB database exactly once
            var customStatus = new CustomStatusEffect(effectInstance,false);
            _effects.Add(effectInstance.name, customStatus);
            ItemManager.Instance.AddStatusEffect(customStatus);
        }
        
        Initialized = true;
    }

    /// <summary>
    /// Creates an instance of the passive powers.
    /// </summary>
    /// <param name="powerName">Passive power name</param>
    /// <returns>Passive Status Effect</returns>
    private static StatusEffect CreatePassivePowerInstance(string powerName)
    {
        var prefab = ObjectDB.instance.GetStatusEffect(powerName.GetStableHashCode());
        if (prefab == null) return null;

        return powerName switch
        {
            "GP_Eikthyr" => CreateEikthyrPassive(prefab),
            "GP_TheElder" => CreateElderPassive(prefab),
            "GP_Bonemass" => CreateBonemassPassive(prefab),
            "GP_Moder" => CreateModerPassive(prefab),
            "GP_Yagluth" => CreateYagluthPassive(prefab),
            "GP_Queen" => CreateQueenPassive(prefab),
            "GP_Fader" => CreateFaderPassive(prefab),
            _ => null
        };
    }

    /// <summary>
    /// Creates the eikthyr passive power.
    /// </summary>
    /// <param name="prefab">eikthyr forsaken power prefab</param>
    /// <returns>Eikthyr passive power</returns>
    private static StatusEffect CreateEikthyrPassive(StatusEffect prefab)
    {
        var effect = ScriptableObject.CreateInstance<SE_Stats>();
        effect.name = "SE_PassiveEikthyrEffect";
        effect.m_name = "$passive_eikthyr_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_eikthyr_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_cooldownIcon = false;
        effect.m_tooltip = "$passive_eikthyr_effect_tooltip";
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;
        
        effect.m_swimStaminaUseModifier = -ForsakenPowerStats.EikthyrStaminaModifier * Plugin.PowerAmount.Value;
        effect.m_runStaminaUseModifier = -ForsakenPowerStats.EikthyrStaminaModifier  * Plugin.PowerAmount.Value;
        effect.m_jumpStaminaUseModifier = -ForsakenPowerStats.EikthyrStaminaModifier  * Plugin.PowerAmount.Value;
        
        return effect;
    }

    /// <summary>
    /// Creates the elder passive power.
    /// </summary>
    /// <param name="prefab">elder forsaken power</param>
    /// <returns>Passive Eldier Power</returns>
    private static StatusEffect CreateElderPassive(StatusEffect prefab)
    {
        var effect = ScriptableObject.CreateInstance<SE_Stats>();
        effect.name = "SE_PassiveElderEffect";
        effect.m_name = "$passive_elder_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_elder_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_tooltip = "$passive_elder_effect_tooltip";
        effect.m_cooldownIcon = false;
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;
        
        effect.m_healthRegenMultiplier = 1f + ForsakenPowerStats.ElderHealthRegenModifier * Plugin.PowerAmount.Value;
        effect.m_percentigeDamageModifiers = new HitData.DamageTypes(){m_chop = ForsakenPowerStats.ElderDamageModifier * Plugin.PowerAmount.Value, m_pickaxe = ForsakenPowerStats.ElderDamageModifier * Plugin.PowerAmount.Value};
        
        return effect;
    }

    /// <summary>
    /// Creates the bonemass passive power.
    /// </summary>
    /// <param name="prefab">prefab</param>
    /// <returns>Passive Bonesmass Power</returns>
    private static StatusEffect CreateBonemassPassive(StatusEffect prefab)
    {
        var effect = ScriptableObject.CreateInstance<SE_Stats>();
        effect.name = "SE_PassiveBonemassEffect";
        effect.m_name = "$passive_bonemass_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_bonemass_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_tooltip = "$passive_bonemass_effect_tooltip";
        effect.m_cooldownIcon = false;
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;
        
        effect.m_blockStaminaUseModifier = -ForsakenPowerStats.BonemassBlockStaminaModifier * Plugin.PowerAmount.Value;
        
        return effect;
    }

    /// <summary>
    /// Creates the moder passive power.
    /// </summary>
    /// <param name="prefab">moder forsaken power prefab</param>
    /// <returns>Passive moder power</returns>
    private static StatusEffect CreateModerPassive(StatusEffect prefab)
    {
        var effect = Object.Instantiate(prefab);

        effect.name = "SE_PassiveModerEffect";
        effect.m_name = "$passive_moder_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_moder_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_tooltip = "$passive_moder_effect_tooltip";
        effect.m_cooldownIcon = false;
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;

        if (effect is not SE_Stats stats) return effect;
        stats.m_addMaxCarryWeight = ForsakenPowerStats.ModerCarryWeightModifier * Plugin.PowerAmount.Value;
        stats.m_speedModifier = ForsakenPowerStats.ModerSpeedModifier * Plugin.PowerAmount.Value;
        stats.m_mods =
        [
            new HitData.DamageModPair()
                { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal }
        ];

        return effect;
    }

    /// <summary>
    /// Creates the yagluth passive power.
    /// </summary>
    /// <param name="prefab">yagluth forsaken power prefab</param>
    /// <returns>Passive yagluth power</returns>
    private static StatusEffect CreateYagluthPassive(StatusEffect prefab)
    {
        var effect = ScriptableObject.CreateInstance<SE_Stats>();
        
        effect.name = "SE_PassiveYagluthEffect";
        effect.m_name = "$passive_yagluth_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_yagluth_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_tooltip = "$passive_yagluth_effect_tooltip";
        effect.m_cooldownIcon = false;
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;

        effect.m_skillLevel = Skills.SkillType.Farming;
        effect.m_skillLevelModifier = ForsakenPowerStats.YagluthFarmingSkillModifier * Plugin.PowerAmount.Value;

        effect.m_percentigeDamageModifiers = new HitData.DamageTypes()
        {
            m_blunt = ForsakenPowerStats.YagluthDamageModifier * Plugin.PowerAmount.Value, 
            m_slash = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value, 
            m_pierce = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value, 
            m_frost = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value,
            m_chop = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value,
            m_fire = ForsakenPowerStats.YagluthDamageModifier * Plugin.PowerAmount.Value,
            m_lightning = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value,
            m_poison = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value,
            m_spirit = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value,
            m_pickaxe = ForsakenPowerStats.YagluthDamageModifier  * Plugin.PowerAmount.Value,
        };

        return effect;
    }

    /// <summary>
    /// Creates the queen passive power.
    /// </summary>
    /// <param name="prefab">queen forsaken power prefab</param>
    /// <returns>Passive queen power</returns>
    private static StatusEffect CreateQueenPassive(StatusEffect prefab)
    {
        var effect = ScriptableObject.CreateInstance<SE_Stats>();
        
        effect.name = "SE_PassiveQueenEffect";
        effect.m_name = "$passive_queen_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_queen_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_tooltip = "$passive_queen_effect_tooltip";
        effect.m_cooldownIcon = false;
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;

        effect.m_eitrRegenMultiplier = 1f + ForsakenPowerStats.QueenEitrRegenModifier * Plugin.PowerAmount.Value;
        effect.m_sneakStaminaUseModifier = -ForsakenPowerStats.QueenSneakStaminaUsage * Plugin.PowerAmount.Value;

        return effect;
    }

    /// <summary>
    /// Creates the fader passive power.
    /// </summary>
    /// <param name="prefab">Fader forsaken power prefab</param>
    /// <returns>Passive Fader power</returns>
    private static StatusEffect CreateFaderPassive(StatusEffect prefab)
    {
        var effect = ScriptableObject.CreateInstance<SE_Stats>();
        
        effect.name = "SE_PassiveFaderEffect";
        effect.m_name = "$passive_fader_effect";
        effect.m_icon = prefab.m_icon;
        effect.m_startMessage = "$passive_fader_effect_start";
        effect.m_startMessageType = prefab.m_startMessageType;
        effect.m_tooltip = "$passive_fader_effect_tooltip";
        effect.m_cooldownIcon = false;
        effect.m_flashIcon = false;
        effect.m_ttl = 0.0f;

        effect.m_staggerModifier = ForsakenPowerStats.FaderStaggerModifier * Plugin.PowerAmount.Value;
        
        effect.m_adrenalineModifier = ForsakenPowerStats.FaderAdrenelineIncrease * Plugin.PowerAmount.Value;

        return effect;
    }
}