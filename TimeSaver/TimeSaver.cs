using System.Linq;
using System.Reflection;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using UnityEngine;

namespace TimeSaver
{
    public class TimeSaver : ModBehaviour
    {
        public static TimeSaver Instance;
        
        private struct TimeSaverSettings
        {
            public bool SkipSplash, SkipDeathSequence, SuppressSlate, AlwaysStartWithSuit, SkipStartupPopup, SkipCredits, SkipPostCredits;
        }

        private TimeSaverSettings _settings;
        private bool QSBEnabled = false;

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
                SkipPostCredits = config.GetSettingsValue<bool>("Skip Post Credits")
            };
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            QSBEnabled = Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies");
            SkipSplash();
            ModHelper.HarmonyHelper.AddPrefix<TitleScreenAnimation>("Awake", typeof(TimeSaver), nameof(SkipSplash));
            ModHelper.HarmonyHelper.AddPrefix<RemoteDialogueTrigger>("ConversationTriggered", typeof(TimeSaver), nameof(RemoteDialogueTriggerConvoTriggered));
            ModHelper.HarmonyHelper.AddPostfix<Flashback>("OnTriggerFlashback", typeof(TimeSaver), nameof(FlashBackGo));
            GlobalMessenger.AddListener("WakeUp", () =>
            {
                ModHelper.Events.Unity.FireOnNextUpdate(OnWakeUpTimeFoSchoo);
            });
            // Stolen from menu framework
            if (typeof(TitleScreenManager).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Any(x => x.Name == "ShowStartupPopupsAndShowMenu"))
            {
                Instance.ModHelper.HarmonyHelper.AddPostfix<TitleScreenManager>(nameof(TitleScreenManager.ShowStartupPopupsAndShowMenu), typeof(TimeSaver), nameof(TitleScreenManagerTryShowThingies));
            }
            else
            {
                Instance.ModHelper.HarmonyHelper.AddPostfix<TitleScreenManager>("TryShowStartupPopupsAndShowMenu", typeof(TimeSaver), nameof(TitleScreenManagerTryShowThingies));
            }
            ModHelper.HarmonyHelper.AddPostfix<Credits>("Start", typeof(TimeSaver), nameof(SkipCredits));
            ModHelper.HarmonyHelper.AddPostfix<PostCreditsManager>("Start", typeof(TimeSaver), nameof(SkipPostCredits));
        }

        private static void SkipSplash()
        {
            if (Instance._settings.SkipSplash)
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
            if (Instance._settings.AlwaysStartWithSuit && !Instance.QSBEnabled)
            {
                var pickup = GameObject.Find("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear").GetComponent<SuitPickupVolume>();
                pickup.OnPressInteract(pickup._interactVolume.GetInteractionAt(0).inputCommand); 
            }
        }

        public static void FlashBackGo(Flashback __instance)
        {
            if (Instance._settings.SkipDeathSequence && !Instance.QSBEnabled)
            {
                __instance._flashbackTimer.endTime = __instance._flashbackTimer.startTime;
            }
        }

        public static bool RemoteDialogueTriggerConvoTriggered(RemoteDialogueTrigger __instance, ref bool __result)
        {
            if (Instance._settings.SuppressSlate && __instance.gameObject.name == "2ndLoopConversation_Trigger")
            {
                __result = false;
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void TitleScreenManagerTryShowThingies(TitleScreenManager __instance)
        {
            if (Instance._settings.SkipStartupPopup)
            {
                __instance._okCancelPopup.InvokeOk();
            }
        }

        public static void SkipCredits(Credits __instance)
        {
            if (Instance._settings.SkipCredits)
            {
                __instance.LoadNextScene();
            }
        }

        public static void SkipPostCredits()
        {
            if (Instance._settings.SkipPostCredits)
            {
                LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 0.5f);
            }
        }
    }
}
