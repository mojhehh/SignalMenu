using UnityEngine;
using System.Collections;
using SignalMenu.SignalSafety.Patches;

namespace SignalMenu.SignalSafety
{
    public class SafetyLoader : MonoBehaviour
    {
        private static bool _initialized = false;
        private static SafetyLoader _instance;

        public static void Initialize(MonoBehaviour runner)
        {
            if (_initialized) return;
            _initialized = true;

            _instance = runner as SafetyLoader;

            Plugin.Initialize(runner);

            try { SafetyConfig.Load(); } catch { }

            try { SafetyPatches.RPCProtection(); } catch { }

            Managers.LogManager.Log("[SignalSafety] Safety systems initialized");

            if (_instance != null)
                _instance.StartCoroutine(_instance.AnnounceProtection());
        }

        private IEnumerator AnnounceProtection()
        {
            yield return new WaitForSeconds(2f);

            if (SafetyConfig.IsFirstOpen)
            {
                AudioManager.Play("first_open_raw", AudioManager.AudioCategory.Protection);
                yield return new WaitForSeconds(5f);
            }

            AudioManager.Play("protection_enabled", AudioManager.AudioCategory.Protection);
            try { SignalMenu.Managers.NotificationManager.SendNotification("<color=grey>[</color><color=green>SIGNAL</color><color=grey>]</color> All protection systems online."); } catch { }

            yield return new WaitForSeconds(3f);
            CheckForBanOnScreen();
        }

        private void CheckForBanOnScreen()
        {
            try
            {
                if (GorillaNetworking.GorillaComputer.instance?.screenText != null)
                {
                    string text = GorillaNetworking.GorillaComputer.instance.screenText.currentText;
                    if (!string.IsNullOrEmpty(text))
                    {
                        string upper = text.ToUpperInvariant();
                        if (upper.Contains("BANNED") || upper.Contains("BAN EXPIRES"))
                        {
                            SafetyPatches.AnnounceBanOnce();
                        }
                    }
                }
            }
            catch { }
        }

        void Update()
        {
            try { SafetyPatches.RPCProtection(); } catch { }
            try { ModeratorDetector.Check(); } catch { }
            try { ContentCreatorDetector.Check(); } catch { }
            try { CosmeticNotifier.Check(); } catch { }
            try { AutomodBypass.Update(); } catch { }
            try { AntiLurkerSystem.Update(); } catch { }
            try { AutoGC.Update(); } catch { }
            try { SupportPageSpoofer.Update(); } catch { }
            try { RankedSpoofer.Update(); } catch { }
            try { FakeBehaviors.FakeOculusMenu(); } catch { }
            try { FakeBehaviors.FakeBrokenController(); } catch { }
            try { AntiBan.Update(); } catch { }
            try { AntiReport.RunAntiReport(); } catch { }
            try { if (AntiReport.VisualizerEnabled) AntiReport.VisualizeAntiReport(); } catch { }
        }

        void LateUpdate()
        {
            try { AntiPredictions.LateUpdate(); } catch { }
        }
    }
}
