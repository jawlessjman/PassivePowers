using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using PassivePowers.Data;

namespace PassivePowers;

/// <summary>
/// The main class that is called by BepInEx.
/// </summary>
[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    
    // Plugin info
    private const string ModGUID = "jawlessjman.PassivePowers";
    public const string ModName = "PassivePowers";
    public const string ModVersion = "1.0.0";   
    
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
    
    private Harmony _harmony;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        
        // Load config values
        _powersEnabled = Config.Bind(
            "General",
            "Enable Passive Powers",
            true,
            new ConfigDescription("Enable Passive Powers mod")
        );

        PowerAmount = Config.Bind(
            "General",
            "Passive Power Percent",
            0.1f,
            new ConfigDescription("The percentage of the base forsaken power to grant default is 10% (0.1)", new AcceptableValueRange<float>(0.01f, 1f))
        );

        EikthyrEnabled = Config.Bind(
            "General",
            "Eikthyr Passive Power Enabled",
            true,
            new ConfigDescription("Enable Eikthyr Passive Power")
            );
        
        ElderEnabled = Config.Bind(
            "General",
            "Elder Passive Power Enabled",
            true,
            new ConfigDescription("Enable Elder Passive Power")
        );
        
        BonemassEnabled = Config.Bind(
            "General",
            "Bonemass Passive Power Enabled",
            true,
            new ConfigDescription("Enable Bonemass Passive Power")
        );
        
        ModerEnabled = Config.Bind(
            "General",
            "Moder Passive Power Enabled",
            true,
            new ConfigDescription("Enable Moder Passive Power")
        );
        
        YagluthEnabled = Config.Bind(
            "General",
            "Yagluth Passive Power Enabled",
            true,
            new ConfigDescription("Enable Yagluth Passive Power")
        );
        
        QueenEnabled = Config.Bind(
            "General",
            "Queen Passive Power Enabled",
            true,
            new ConfigDescription("Enable Queen Passive Power")
        );
        
        FaderEnabled = Config.Bind(
            "General",
            "Fader Passive Power Enabled",
            true,
            new ConfigDescription("Enable Fader Passive Power")
        );
        
        // Load local translation for English
        var assembly = Assembly.GetExecutingAssembly();

        const string resourceName = $"PassivePowers.Assets.Translations.English.passivePowers.json";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            Logger.LogError($"Resource: {resourceName} to load English translation file");
        }
        else
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            Jotunn.Managers.LocalizationManager.Instance.GetLocalization().AddJsonFile("English", json);
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
        
        _harmony = new Harmony(ModGUID);
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
        
        foreach (var file in Directory.GetFiles(root, "*.json", SearchOption.AllDirectories))
        {
            Logger.LogInfo($"Loading translation file: {file}");
            Jotunn.Managers.LocalizationManager.Instance.GetLocalization().AddFileByPath(file);
        }
    }
}
