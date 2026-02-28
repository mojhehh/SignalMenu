/*
 * Signal Safety Menu  Patches/Safety/StackTraceSafetyPatches.cs
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
using System.Diagnostics;
using System.Text;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Stack trace sanitization patches that go BEYOND Rexon.
    /// Removes mod-related frames from stack traces so that any server-side 
    /// or client-side analysis of stack traces can't detect mod code.
    /// </summary>
    public class StackTraceSafetyPatches
    {
        /// <summary>
        /// Namespace prefixes that should be removed from stack traces.
        /// </summary>
        private static readonly string[] HiddenNamespaces = new[]
        {
            "SignalMenu",
            "SignalAutoUpdater",
            "AntiBanAnalyzer",
            "HarmonyLib",
            "HarmonyLib.Internal",
            "MonoMod",
            "BepInEx",
            "0Harmony",
            "patch_",
            "DMD<",
            "Trampoline<"
        };

        /// <summary>
        /// Check if a stack frame line should be hidden.
        /// </summary>
        private static bool ShouldHideFrame(string frameLine)
        {
            if (string.IsNullOrEmpty(frameLine)) return false;
            string trimmed = frameLine.TrimStart();
            foreach (var ns in HiddenNamespaces)
            {
                // "at Namespace.Class.Method" format
                if (trimmed.Contains(ns))
                    return true;
            }
            return false;
        }

        // ── Sanitize Environment.StackTrace ──────────────────────────

        [HarmonyPatch(typeof(Environment), nameof(Environment.StackTrace), MethodType.Getter)]
        public class EnvironmentStackTracePatch
        {
            static void Postfix(ref string __result)
            {
                if (__result == null) return;

                var lines = __result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var sb = new StringBuilder();
                bool first = true;

                foreach (var line in lines)
                {
                    if (!ShouldHideFrame(line))
                    {
                        if (!first) sb.Append(Environment.NewLine);
                        sb.Append(line);
                        first = false;
                    }
                }

                __result = sb.ToString();
            }
        }

        // ── Sanitize StackTrace.ToString() ───────────────────────────

        [HarmonyPatch(typeof(StackTrace), nameof(StackTrace.ToString), new Type[0])]
        public class StackTraceToStringPatch
        {
            static void Postfix(ref string __result)
            {
                if (__result == null) return;

                var lines = __result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var sb = new StringBuilder();
                bool first = true;

                foreach (var line in lines)
                {
                    if (!ShouldHideFrame(line))
                    {
                        if (!first) sb.Append(Environment.NewLine);
                        sb.Append(line);
                        first = false;
                    }
                }

                __result = sb.ToString();
            }
        }

        // ── Sanitize Exception.StackTrace property ───────────────────

        [HarmonyPatch(typeof(Exception), nameof(Exception.StackTrace), MethodType.Getter)]
        public class ExceptionStackTracePatch
        {
            static void Postfix(ref string __result)
            {
                if (__result == null) return;

                var lines = __result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var sb = new StringBuilder();
                bool first = true;

                foreach (var line in lines)
                {
                    if (!ShouldHideFrame(line))
                    {
                        if (!first) sb.Append(Environment.NewLine);
                        sb.Append(line);
                        first = false;
                    }
                }

                __result = sb.ToString();
            }
        }
    }
}
