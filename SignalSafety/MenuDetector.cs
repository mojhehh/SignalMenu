using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PlayFab;
using GorillaNetworking;
using SignalMenu.Managers;
using UnityEngine;

namespace SignalMenu.SignalSafety
{
    public static class MenuDetector
    {
        private static readonly Dictionary<string, string> KnownMenus = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "iiMenu",         "ii's Stupid Menu" },
            { "iisStupidMenu",  "ii's Stupid Menu" },
            { "Bark",           "Bark Menu" },
            { "BarkMenu",       "Bark Menu" },
            { "Aspect",         "Aspect Menu" },
            { "AspectMenu",     "Aspect Menu" },
            { "Lunacy",         "Lunacy" },
            { "GoldMenu",       "Gold Menu" },
            { "MiiMenu",        "Mii Menu" },
            { "Bobcat",         "Bobcat" },
            { "BobcatMenu",     "Bobcat" },
            { "Gecko",          "Gecko Menu" },
            { "GTag_AntiCheat", "Anti-Cheat Mod" },
            { "PigeonClient",   "Pigeon Client" },
            { "MonkeMenuV2",    "Monke Menu" },
            { "DaMonkeMenu",    "Da Monke" },
            { "CheatMenu",      "Generic Cheat" },
        };

        private static readonly string[] KnownHarmonyIds =
        {
            "org.iidk.gorillatag",
            "com.bark.gorillatag",
            "com.aspect.gorillatag",
            "com.lunacy.gorillatag",
            "com.goldmenu.gorillatag",
            "com.mii.gorillatag",
            "com.bobcat.gorillatag",
            "com.gecko.gorillatag",
            "com.pigeon.gorillatag",
        };

        public static bool MenuDetected { get; private set; }
        public static string DetectedMenuName { get; private set; } = "";
        public static List<string> AllDetected { get; private set; } = new List<string>();
        public static int OverriddenPatchCount { get; private set; }
        public static bool ScanComplete { get; private set; }

        private static float _lastScanTime = 0f;
        private static bool _alertPlayed = false;
        private static bool _alreadyLogged = false;
        private static bool _overrideAttempted = false;

        public static void FullScan(Harmony ourHarmony)
        {
            AllDetected.Clear();
            MenuDetected = false;
            DetectedMenuName = "";
            OverriddenPatchCount = 0;

            ScanAssemblies();
            ScanHarmonyPatches(ourHarmony);

            ScanComplete = true;
            _lastScanTime = Time.time;

            if (MenuDetected)
            {
                if (!_alreadyLogged)
                {
                    _alreadyLogged = true;
                    Plugin.Instance?.Log($"[MenuDetector] DETECTED: {DetectedMenuName} ({AllDetected.Count} total conflicts)");
                }

                if (SafetyConfig.MenuDetectionAlertEnabled && !_alertPlayed)
                {
                    _alertPlayed = true;
                    AudioManager.Play("menu_detected", AudioManager.AudioCategory.Warning);
                    try { NotificationManager.SendNotification($"<color=grey>[</color><color=red>MENU DETECTED</color><color=grey>]</color> {DetectedMenuName} is loaded alongside this menu."); } catch { }
                }

                if (SafetyConfig.AutoOverrideOnDetection && !_overrideAttempted)
                {
                    _overrideAttempted = true;
                    OverriddenPatchCount = ForceOverrideAll(ourHarmony);
                    if (OverriddenPatchCount > 0)
                    {
                        Plugin.Instance?.Log($"[MenuDetector] Overrode {OverriddenPatchCount} conflicting patches");
                        AudioManager.Play("menu_detected", AudioManager.AudioCategory.PatchOverride);
                        try { NotificationManager.SendNotification($"<color=grey>[</color><color=yellow>PATCH OVERRIDE</color><color=grey>]</color> Overrode {OverriddenPatchCount} conflicting patches."); } catch { }
                    }
                }
            }
            else
            {
                if (_alreadyLogged)
                {
                    Plugin.Instance?.Log("[MenuDetector] Conflict cleared");
                    _alreadyLogged = false;
                    _overrideAttempted = false;
                }
            }
        }

        public static void PeriodicScan(Harmony ourHarmony)
        {
            if (!SafetyConfig.MenuDetectionEnabled) return;
            if (Time.time - _lastScanTime < 30f) return;

            bool wasPreviouslyDetected = MenuDetected;
            FullScan(ourHarmony);

            if (MenuDetected && !wasPreviouslyDetected)
            {
                Plugin.Instance?.Log($"[MenuDetector] NEW menu detected at runtime: {DetectedMenuName}");
                if (SafetyConfig.MenuDetectionAlertEnabled)
                {
                    AudioManager.Play("menu_detected", AudioManager.AudioCategory.Warning);
                }
            }
        }

        private static void ScanAssemblies()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string asmName = asm.GetName().Name;
                    if (string.IsNullOrEmpty(asmName)) continue;

                    if (asmName.Contains("SignalSafetyMenu") || asmName.Contains("VRInputModule")) continue;

                    foreach (var kv in KnownMenus)
                    {
                        if (asmName.Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!AllDetected.Contains(kv.Value))
                                AllDetected.Add(kv.Value);
                            MenuDetected = true;
                            if (string.IsNullOrEmpty(DetectedMenuName))
                                DetectedMenuName = kv.Value;
                            Plugin.Instance?.Log($"[MenuDetector] Assembly match: '{asmName}' == '{kv.Key}' -> {kv.Value}");
                            break;
                        }
                    }

                    try
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            string typeName = type.FullName ?? "";
                            string className = type.Name ?? "";
                            
                            bool isMenu = false;
                            string label = "";
                            
                            if (typeName.StartsWith("iiMenu.") || typeName.StartsWith("iisStupidMenu.") || className == "iiMenu")
                            {
                                isMenu = true;
                                label = "ii's Stupid Menu";
                            }
                            else if (typeName.StartsWith("BarkMod.") || className == "BarkMod")
                            {
                                isMenu = true;
                                label = "Bark Menu";
                            }
                            else if (typeName.StartsWith("AspectMod.") || className == "AspectMod")
                            {
                                isMenu = true;
                                label = "Aspect Menu";
                            }
                            else if (typeName.StartsWith("LunacyMenu.") || className == "LunacyMenu")
                            {
                                isMenu = true;
                                label = "Lunacy";
                            }
                            
                            if (isMenu)
                            {
                                Plugin.Instance?.Log($"[MenuDetector] Type match: '{typeName}' -> {label}");
                                if (!AllDetected.Contains(label))
                                    AllDetected.Add(label);
                                MenuDetected = true;
                                if (string.IsNullOrEmpty(DetectedMenuName))
                                    DetectedMenuName = label;
                            }
                        }
                    }
                    catch {  }
                }
            }
            catch (Exception e)
            {
                Plugin.Instance?.Log($"[MenuDetector] Assembly scan error: {e.Message}");
            }
        }

        private static void ScanHarmonyPatches(Harmony ourHarmony)
        {
            try
            {
                var allIds = Harmony.GetAllPatchedMethods()
                    .SelectMany(m =>
                    {
                        var info = Harmony.GetPatchInfo(m);
                        if (info == null) return Enumerable.Empty<string>();
                        return info.Prefixes.Select(p => p.owner)
                            .Concat(info.Postfixes.Select(p => p.owner))
                            .Concat(info.Transpilers.Select(p => p.owner));
                    })
                    .Distinct()
                    .Where(id => !string.IsNullOrEmpty(id) && id != ourHarmony.Id)
                    .ToList();

                foreach (var id in allIds)
                {
                    foreach (var knownId in KnownHarmonyIds)
                    {
                        if (id.Equals(knownId, StringComparison.OrdinalIgnoreCase))
                        {
                            string label = $"Harmony: {id}";
                            Plugin.Instance?.Log($"[MenuDetector] Harmony match: '{id}' == '{knownId}'");
                            if (!AllDetected.Contains(label))
                                AllDetected.Add(label);
                            MenuDetected = true;
                            if (DetectedMenuName == "")
                                DetectedMenuName = id;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Instance?.Log($"[MenuDetector] Harmony scan error: {e.Message}");
            }
        }

        public static int ForceOverrideAll(Harmony ourHarmony)
        {
            int removed = 0;

            var criticalMethods = new (Type type, string method)[]
            {
                (typeof(MonkeAgent), "SendReport"),
                (typeof(MonkeAgent), "DispatchReport"),
                (typeof(MonkeAgent), "CheckReports"),
                (typeof(MonkeAgent), "SliceUpdate"),
                (typeof(MonkeAgent), "IncrementRPCCallLocal"),
                (typeof(MonkeAgent), "IncrementRPCCall"),
                (typeof(MonkeAgent), "IncrementRPCTracker"),
                (typeof(MonkeAgent), "CloseInvalidRoom"),
                (typeof(MonkeAgent), "QuitDelay"),
                (typeof(MonkeAgent), "ShouldDisconnectFromRoom"),
                (typeof(MonkeAgent), "RefreshRPCs"),
                (typeof(MonkeAgent), "LogErrorCount"),
                (typeof(GorillaTelemetry), "EnqueueTelemetryEvent"),
                (typeof(GorillaTelemetry), "PostBuilderKioskEvent"),
                (typeof(GorillaTelemetry), "SuperInfectionEvent"),
                (typeof(PlayFabClientInstanceAPI), "ReportPlayer"),
                (typeof(GorillaNetworking.GorillaComputer), "CheckAutoBanListForName"),
                (typeof(GorillaNetworking.GorillaComputer), "CheckAutoBanListForRoomName"),
                (typeof(GorillaNetworking.GorillaComputer), "CheckAutoBanListForPlayerName"),
                (typeof(GorillaNetworking.GorillaComputer), "CheckAutoBanListForTroopName"),
                (typeof(GorillaPlayerScoreboardLine), "ReportPlayer"),
                (typeof(GorillaNetworking.GorillaComputer), "GeneralFailureMessage"),
                (typeof(GorillaNetworking.GorillaComputer), "UpdateFailureText"),
            };

            foreach (var (type, method) in criticalMethods)
            {
                try
                {
                    var original = AccessTools.Method(type, method);
                    if (original == null) continue;

                    var patches = Harmony.GetPatchInfo(original);
                    if (patches == null) continue;

                    foreach (var prefix in patches.Prefixes)
                    {
                        if (prefix.owner == ourHarmony.Id) continue;
                        try
                        {
                            ourHarmony.Unpatch(original, prefix.PatchMethod);
                            removed++;
                            Plugin.Instance?.Log($"[Override] Removed {prefix.owner} prefix from {type.Name}.{method}");
                        }
                        catch { }
                    }

                    foreach (var postfix in patches.Postfixes)
                    {
                        if (postfix.owner == ourHarmony.Id) continue;
                        try
                        {
                            ourHarmony.Unpatch(original, postfix.PatchMethod);
                            removed++;
                        }
                        catch { }
                    }

                    foreach (var transpiler in patches.Transpilers)
                    {
                        if (transpiler.owner == ourHarmony.Id) continue;
                        try
                        {
                            ourHarmony.Unpatch(original, transpiler.PatchMethod);
                            removed++;
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return removed;
        }

        public static string GetDetectionSummary()
        {
            if (!ScanComplete) return "Scan pending...";
            if (!MenuDetected) return "No conflicts detected";
            return $"Detected: {string.Join(", ", AllDetected)} | Overrides: {OverriddenPatchCount}";
        }
    }
}
