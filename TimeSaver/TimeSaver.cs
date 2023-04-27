using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System.Reflection;
using UnityEngine;

namespace TimeSaver
{
    [HarmonyPatch]
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
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            QSBEnabled = Instance.ModHelper.Interaction.ModExists("Raicuparta.QuantumSpaceBuddies");
            SkipSplash();
            GlobalMessenger.AddListener("WakeUp", () =>
            {
                ModHelper.Events.Unity.FireOnNextUpdate(OnWakeUpTimeFoSchoo);
            });
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TitleScreenAnimation), nameof(TitleScreenAnimation.Awake))]
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

        [HarmonyPostfix, HarmonyPatch(typeof(Flashback), nameof(Flashback.OnTriggerFlashback))]
        public static void FlashBackGo(Flashback __instance)
        {
            if (Instance._settings.SkipDeathSequence && !Instance.QSBEnabled)
            {
                __instance._flashbackTimer.endTime = __instance._flashbackTimer.startTime;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RemoteDialogueTrigger), nameof(RemoteDialogueTrigger.ConversationTriggered))]
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

        [HarmonyPostfix, HarmonyPatch(typeof(TitleScreenManager), nameof(TitleScreenManager.TryShowStartupPopupsAndShowMenu))]
        public static void TitleScreenManagerTryShowThingies(TitleScreenManager __instance)
        {
            if (Instance._settings.SkipStartupPopup)
            {
                __instance._okCancelPopup.InvokeOk();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Credits), nameof(Credits.Start))]
        public static void SkipCredits(Credits __instance)
        {
            if (Instance._settings.SkipCredits)
            {
                __instance.LoadNextScene();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PostCreditsManager), nameof(Credits.Start))]
        public static void SkipPostCredits()
        {
            if (Instance._settings.SkipPostCredits)
            {
                LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 0.5f);
            }
        }
    }
}
