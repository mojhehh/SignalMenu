/*
 * Signal Safety Menu  Patches/Safety/DebugLogPatches.cs
 * A mod menu for Gorilla Tag with over 1000+ mods
 *
 * Copyright (C) 2026  mojhehh (forked from Goldentrophy Software)
 * https://github.com/mojhehh/SignalMenu
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using System;
using UnityEngine;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Suppresses Unity debug log output that could reveal mod activity.
    /// Filters log messages containing mod-related keywords while allowing
    /// normal game logs to pass through. This prevents the game or anti-cheat
    /// from detecting mod traces in the Unity debug log system.
    /// </summary>
    public class DebugLogPatches
    {
        /// <summary>
        /// Keywords that indicate a log message is mod-related and should be suppressed.
        /// Kept specific to avoid matching legitimate game strings like "model", "dispatch", etc.
        /// </summary>
        private static readonly string[] SuspiciousKeywords = new string[]
        {
            "signal menu", "signal safety", "signalmenu", "vrinputmodule",
            "harmony", "bepinex", "bepinex.core",
            "inject", "injector", "injecting",
            "hook", "hooking", "detour",
            "bypass", "spoof", "spoofing",
            "cheat", "exploit", "hacking",
            "mod menu", "mod loader", "modloader",
            "plugin loaded", "plugin manager", "pluginmanager",
            "patcher", "patching", "unpatching",
            "monkeagent bypass", "anti-cheat bypass"
        };

        /// <summary>
        /// Returns true if the message should be blocked (contains suspicious keywords).
        /// </summary>
        private static bool ShouldSuppressLog(object message)
        {
            if (message == null) return false;

            string msg = message.ToString().ToLowerInvariant();
            for (int i = 0; i < SuspiciousKeywords.Length; i++)
            {
                if (msg.Contains(SuspiciousKeywords[i]))
                    return true;
            }

            return false;
        }

        // ── Debug.Log ─────────────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.Log), typeof(object))]
        public class Log1
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.Log), typeof(object), typeof(UnityEngine.Object))]
        public class Log2
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        // ── Debug.LogWarning ──────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), typeof(object))]
        public class LogWarning1
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), typeof(object), typeof(UnityEngine.Object))]
        public class LogWarning2
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        // ── Debug.LogError ────────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogError), typeof(object))]
        public class LogError1
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogError), typeof(object), typeof(UnityEngine.Object))]
        public class LogError2
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        // ── Debug.LogException ────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogException), typeof(Exception))]
        public class LogException1
        {
            static bool Prefix(Exception exception) =>
                exception == null || !ShouldSuppressLog(exception.ToString());
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogException), typeof(Exception), typeof(UnityEngine.Object))]
        public class LogException2
        {
            static bool Prefix(Exception exception) =>
                exception == null || !ShouldSuppressLog(exception.ToString());
        }

        // ── Debug.LogFormat ───────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogFormat), typeof(string), typeof(object[]))]
        public class LogFormat1
        {
            static bool Prefix(string format) => !ShouldSuppressLog(format);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogFormat), typeof(UnityEngine.Object), typeof(string), typeof(object[]))]
        public class LogFormat2
        {
            static bool Prefix(UnityEngine.Object context, string format) => !ShouldSuppressLog(format);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogFormat), typeof(LogType), typeof(LogOption), typeof(UnityEngine.Object), typeof(string), typeof(object[]))]
        public class LogFormat3
        {
            static bool Prefix(LogType logType, LogOption logOptions, UnityEngine.Object context, string format) => !ShouldSuppressLog(format);
        }

        // ── Debug.LogWarningFormat ────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarningFormat), typeof(string), typeof(object[]))]
        public class LogWarningFormat1
        {
            static bool Prefix(string format) => !ShouldSuppressLog(format);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarningFormat), typeof(UnityEngine.Object), typeof(string), typeof(object[]))]
        public class LogWarningFormat2
        {
            static bool Prefix(UnityEngine.Object context, string format) => !ShouldSuppressLog(format);
        }

        // ── Debug.LogErrorFormat ──────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogErrorFormat), typeof(string), typeof(object[]))]
        public class LogErrorFormat1
        {
            static bool Prefix(string format) => !ShouldSuppressLog(format);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogErrorFormat), typeof(UnityEngine.Object), typeof(string), typeof(object[]))]
        public class LogErrorFormat2
        {
            static bool Prefix(UnityEngine.Object context, string format) => !ShouldSuppressLog(format);
        }

        // ── Debug.Assert ──────────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.Assert), typeof(bool))]
        public class Assert1
        {
            static bool Prefix(bool condition) => condition; // Only allow passing assertions
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.Assert), typeof(bool), typeof(object))]
        public class Assert2
        {
            static bool Prefix(bool condition) => condition;
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.Assert), typeof(bool), typeof(string))]
        public class Assert3
        {
            static bool Prefix(bool condition) => condition;
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.Assert), typeof(bool), typeof(object), typeof(UnityEngine.Object))]
        public class Assert4
        {
            static bool Prefix(bool condition) => condition;
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.Assert), typeof(bool), typeof(string), typeof(UnityEngine.Object))]
        public class Assert5
        {
            static bool Prefix(bool condition) => condition;
        }

        // ── Debug.LogAssertion ────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogAssertion), typeof(object))]
        public class LogAssertion1
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogAssertion), typeof(object), typeof(UnityEngine.Object))]
        public class LogAssertion2
        {
            static bool Prefix(object message) => !ShouldSuppressLog(message);
        }

        // ── Debug.LogAssertionFormat ──────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogAssertionFormat), typeof(string), typeof(object[]))]
        public class LogAssertionFormat1
        {
            static bool Prefix(string format) => !ShouldSuppressLog(format);
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.LogAssertionFormat), typeof(UnityEngine.Object), typeof(string), typeof(object[]))]
        public class LogAssertionFormat2
        {
            static bool Prefix(UnityEngine.Object context, string format) => !ShouldSuppressLog(format);
        }

        // ── Debug.AssertFormat ────────────────────────────────────────

        [HarmonyPatch(typeof(Debug), nameof(Debug.AssertFormat), typeof(bool), typeof(string), typeof(object[]))]
        public class AssertFormat1
        {
            static bool Prefix(bool condition) => condition;
        }

        [HarmonyPatch(typeof(Debug), nameof(Debug.AssertFormat), typeof(bool), typeof(UnityEngine.Object), typeof(string), typeof(object[]))]
        public class AssertFormat2
        {
            static bool Prefix(bool condition) => condition;
        }
    }
}
