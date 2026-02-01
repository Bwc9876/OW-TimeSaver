using System.Reflection;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using UnityEngine;

namespace TimeSaver;

[HarmonyPatch]
public class TimeSaver : ModBehaviour, ITimeSaver
{
    public static TimeSaver Instance;

    private struct TimeSaverSettings
    {
        public bool? SkipSplash,
            SkipDeathSequence,
            SuppressSlate,
            AlwaysStartWithSuit,
            SkipStartupPopup,
            SkipCredits,
            SkipPostCredits;
    }

    private TimeSaverSettings _settingsOverride;
    private TimeSaverSettings _settings;
    private bool QSBEnabled = false;

    public bool SkipSplash
    {
        get => _settingsOverride.SkipSplash ?? _settings.SkipSplash ?? false;
    }
    public bool SkipDeathSequence
    {
        get => _settingsOverride.SkipDeathSequence ?? _settings.SkipDeathSequence ?? false;
    }
    public bool SuppressSlate
    {
        get => _settingsOverride.SuppressSlate ?? _settings.SuppressSlate ?? false;
    }
    public bool AlwaysStartWithSuit
    {
        get =>
            !QSBEnabled
            && (_settingsOverride.AlwaysStartWithSuit ?? _settings.AlwaysStartWithSuit ?? false);
    }
    public bool SkipStartupPopup
    {
        get => _settingsOverride.SkipStartupPopup ?? _settings.SkipStartupPopup ?? false;
    }
    public bool SkipCredits
    {
        get => _settingsOverride.SkipCredits ?? _settings.SkipCredits ?? false;
    }
    public bool SkipPostCredits
    {
        get => _settingsOverride.SkipPostCredits ?? _settings.SkipPostCredits ?? false;
    }

    public override object GetApi()
    {
        return Instance;
    }

    public override void Configure(IModConfig config)
    {
        _settings = new TimeSaverSettings
        {
            SkipSplash = config.GetSettingsValue<bool>("Skip Splash"),
            SkipDeathSequence = config.GetSettingsValue<bool>("Skip Death Sequence"),
            SuppressSlate = config.GetSettingsValue<bool>("Suppress Slate"),
            AlwaysStartWithSuit = config.GetSettingsValue<bool>("Always Start With Suit"),
            SkipStartupPopup = config.GetSettingsValue<bool>("Skip Startup Popups"),
            SkipCredits = config.GetSettingsValue<bool>("Skip Credits"),
            SkipPostCredits = config.GetSettingsValue<bool>("Skip Post Credits"),
        };
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        QSBEnabled = Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies");
        SkipSplashPatch();
        GlobalMessenger.AddListener(
            "WakeUp",
            () =>
            {
                ModHelper.Events.Unity.FireOnNextUpdate(OnWakeUpTimeFoSchoo);
            }
        );
    }

    public void ToggleSkipSplash(bool? enabled)
    {
        _settingsOverride.SkipSplash = enabled;
    }

    public void ToggleSkipDeathSequence(bool? enabled)
    {
        _settingsOverride.SkipDeathSequence = enabled;
    }

    public void ToggleSkipPopups(bool? enabled)
    {
        _settingsOverride.SkipStartupPopup = enabled;
    }

    public void ToggleSkipCredits(bool? enabled)
    {
        _settingsOverride.SkipCredits = enabled;
    }

    public void ToggleSkipPostCredits(bool? enabled)
    {
        _settingsOverride.SkipPostCredits = enabled;
    }

    public void ToggleAlwaysStartWithSuit(bool? enabled)
    {
        _settingsOverride.AlwaysStartWithSuit = enabled;
    }

    public void ToggleSuppressSlate(bool? enabled)
    {
        _settingsOverride.SuppressSlate = enabled;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TitleScreenAnimation), nameof(TitleScreenAnimation.Awake))]
    private static void SkipSplashPatch()
    {
        if (Instance.SkipSplash)
        {
            var titleScreenAnimation = FindObjectOfType<TitleScreenAnimation>();
            titleScreenAnimation._fadeDuration = 0;
            titleScreenAnimation._gamepadSplash = false;
            titleScreenAnimation._introPan = false;
            titleScreenAnimation.Invoke("FadeInTitleLogo");

            var titleAnimationController = FindObjectOfType<TitleAnimationController>();
            titleAnimationController._logoFadeDelay = 0.001f;
            titleAnimationController._logoFadeDuration = 0.001f;
            titleAnimationController._optionsFadeDelay = 0.001f;
            titleAnimationController._optionsFadeDuration = 0.001f;
            titleAnimationController._optionsFadeSpacing = 0.001f;
        }
    }

    private static void OnWakeUpTimeFoSchoo()
    {
        if (Instance.AlwaysStartWithSuit)
        {
            var pickup = GameObject
                .Find("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear")
                .GetComponent<SuitPickupVolume>();
            pickup.OnPressInteract(pickup._interactVolume.GetInteractionAt(0).inputCommand);
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Flashback), nameof(Flashback.OnTriggerFlashback))]
    public static void FlashBackGo(Flashback __instance)
    {
        if (Instance.SkipDeathSequence && !Instance.QSBEnabled)
        {
            __instance._flashbackTimer.endTime = __instance._flashbackTimer.startTime;
        }
    }

    [
        HarmonyPrefix,
        HarmonyPatch(
            typeof(RemoteDialogueTrigger),
            nameof(RemoteDialogueTrigger.ConversationTriggered)
        )
    ]
    public static bool RemoteDialogueTriggerConvoTriggered(
        RemoteDialogueTrigger __instance,
        ref bool __result
    )
    {
        if (Instance.SuppressSlate && __instance.gameObject.name == "2ndLoopConversation_Trigger")
        {
            __result = false;
            return false;
        }
        else
        {
            return true;
        }
    }

    [
        HarmonyPostfix,
        HarmonyPatch(
            typeof(TitleScreenManager),
            nameof(TitleScreenManager.TryShowStartupPopupsAndShowMenu)
        )
    ]
    public static void TitleScreenManagerTryShowThingies(TitleScreenManager __instance)
    {
        if (Instance.SkipStartupPopup)
        {
            __instance._okCancelPopup.InvokeOk();
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Credits), nameof(Credits.Start))]
    public static void SkipCreditsPatch(Credits __instance)
    {
        if (Instance.SkipCredits)
        {
            __instance.LoadNextScene();
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PostCreditsManager), nameof(Credits.Start))]
    public static void SkipPostCreditsPatch()
    {
        if (Instance.SkipPostCredits)
        {
            LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 0.5f);
        }
    }
}
