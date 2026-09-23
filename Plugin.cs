using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using ServerSync;
using PassivePowers.Data;

namespace PassivePowers;

/// <summary>
/// The main class that is called by BepInEx.
/// </summary>
[BepInPlugin(ModGuid, ModName, ModVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;

    // Plugin info
    private const string ModGuid = "jawlessjman.PassivePowers";
    public const string ModName = "PassivePowers";
    public const string ModVersion = "1.1.1";

    // Config values
    private ConfigEntry<bool> _powersEnabled;
    public static ConfigEntry<float> PowerAmount;

    public static ConfigEntry<bool> EikthyrEnabled;
    public static ConfigEntry<bool> ElderEnabled;
    public static ConfigEntry<bool> BonemassEnabled;
    public static ConfigEntry<bool> ModerEnabled;
    public static ConfigEntry<bool> YagluthEnabled;
    public static ConfigEntry<bool> QueenEnabled;
    public static ConfigEntry<bool> FaderEnabled;

    public static readonly ConfigSync ConfigSync = new(ModGuid)
    {
        DisplayName = ModName,
        CurrentVersion = ModVersion,
        MinimumRequiredVersion = "1.1.0",
        IsLocked = true,
        ModRequired = true
    };

private Harmony _harmony;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        
        BindConfigs();
        
        // Load local translation for English
        const string resourceName = $"PassivePowers.Assets.Translations.English.passivePowers.json";
        var localizedEnglish = AssetUtils.LoadTextFromResources(resourceName);
        if (string.IsNullOrEmpty(localizedEnglish))
        {
            Logger.LogInfo($"Loading English translation file from resources");
        }
        else
        {
            LocalizationManager.Instance.GetLocalization().AddJsonFile("English", localizedEnglish);
        }
        
        // Load translations for other languages
        LoadTranslations();
        
        Logger.LogInfo($"Plugin {ModName}-{ModVersion} is loaded!");

        if (!_powersEnabled.Value)
        {
            Logger.LogInfo("Passive Powers are disabled in the config!");
            return;
        }
        
        ItemManager.OnItemsRegistered += GetStatusEffect.RegisterAllPassivePowers;
        
        SynchronizationManager.OnConfigurationSynchronized += (_, _) =>
        {
            Logger.LogInfo("Configuration synchronized with server.");
            
            GetStatusEffect.ResetStatusEffects();
            GetStatusEffect.RegisterAllPassivePowers();
        };
        
        _harmony = new Harmony(ModGuid);
        _harmony.PatchAll();
    }
    
    /// <summary>
    /// Loads translations from the Translations folder.
    /// </summary>
    private void LoadTranslations()
    {
        var root = Path.Combine(
            Path.GetDirectoryName(Info.Location)!,
            "Translations"
        );

        if (!Directory.Exists(root)) return;
        
        foreach (var file in Directory.GetFiles(root, "passivePowers.json", SearchOption.AllDirectories))
        {
            Logger.LogInfo($"Loading translation file: {file}");
            LocalizationManager.Instance.GetLocalization().AddFileByPath(file);
        }
    }

    private void BindConfigs()
    {
        // Load config values
        _powersEnabled = Config.Bind(
            "General",
            "Enable Passive Powers",
            true,
            new ConfigDescription("Enable Passive Powers mod")
        );
        ConfigSync.AddConfigEntry(_powersEnabled).SynchronizedConfig = true;

        PowerAmount = Config.Bind(
            "General",
            "Passive Power Percent",
            0.1f,
            new ConfigDescription("The percentage of the base forsaken power to grant default is 10% (0.1) 500% (5)", new AcceptableValueRange<float>(0.01f, 5f))
        );
        ConfigSync.AddConfigEntry(PowerAmount).SynchronizedConfig = true;

        EikthyrEnabled = Config.Bind(
            "General",
            "Eikthyr Passive Power Enabled",
            true,
            new ConfigDescription("Enable Eikthyr Passive Power")
        );
        ConfigSync.AddConfigEntry(EikthyrEnabled).SynchronizedConfig = true;
        
        ElderEnabled = Config.Bind(
            "General",
            "Elder Passive Power Enabled",
            true,
            new ConfigDescription("Enable Elder Passive Power")
        );
        ConfigSync.AddConfigEntry(ElderEnabled).SynchronizedConfig = true;
        
        BonemassEnabled = Config.Bind(
            "General",
            "Bonemass Passive Power Enabled",
            true,
            new ConfigDescription("Enable Bonemass Passive Power")
        );
        ConfigSync.AddConfigEntry(BonemassEnabled).SynchronizedConfig = true;
        
        ModerEnabled = Config.Bind(
            "General",
            "Moder Passive Power Enabled",
            true,
            new ConfigDescription("Enable Moder Passive Power")
        );
        ConfigSync.AddConfigEntry(ModerEnabled).SynchronizedConfig = true;
        
        YagluthEnabled = Config.Bind(
            "General",
            "Yagluth Passive Power Enabled",
            true,
            new ConfigDescription("Enable Yagluth Passive Power")
        );
        ConfigSync.AddConfigEntry(YagluthEnabled).SynchronizedConfig = true;
        
        QueenEnabled = Config.Bind(
            "General",
            "Queen Passive Power Enabled",
            true,
            new ConfigDescription("Enable Queen Passive Power")
        );
        ConfigSync.AddConfigEntry(QueenEnabled).SynchronizedConfig = true;
        
        FaderEnabled = Config.Bind(
            "General",
            "Fader Passive Power Enabled",
            true,
            new ConfigDescription("Enable Fader Passive Power")
        );
        ConfigSync.AddConfigEntry(FaderEnabled).SynchronizedConfig = true;
    }
}
