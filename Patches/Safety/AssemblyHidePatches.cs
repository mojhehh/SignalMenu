/*
 * Signal Safety Menu  Patches/Safety/AssemblyHidePatches.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Hides mod-related assemblies from reflection-based detection.
    /// - Spoofs Assembly.Location and Assembly.CodeBase to return empty strings for mod DLLs
    /// - Filters mod assemblies from AppDomain.GetAssemblies() results
    /// - Prevents stack trace inspection from revealing mod types
    /// </summary>
    public class AssemblyHidePatches
    {
        /// <summary>
        /// Assembly names that should be hidden from detection.
        /// </summary>
        private static readonly HashSet<string> HiddenAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "VRInputModule",
            "Signal Safety Menu",
            "SignalSafetyMenu",
            "SignalMenu",
            "SignalAutoUpdater",
            "AntiBanAnalyzer",
            "0Harmony",
            "BepInEx",
            "BepInEx.Preloader",
            "BepInEx.Core",
            "BepInEx.Unity",
            "MonoMod.RuntimeDetour",
            "MonoMod.Utils",
            "HarmonyXInterop"
        };

        /// <summary>
        /// Recursion guard to prevent stack overflow when checking assembly properties
        /// that are themselves patched by this class.
        /// </summary>
        [ThreadStatic] private static bool _checking;

        private static bool IsModAssembly(Assembly asm)
        {
            if (asm == null) return false;
            if (_checking) return false; // Prevent infinite recursion
            _checking = true;
            try
            {
                string name = asm.GetName().Name;
                if (HiddenAssemblyNames.Contains(name))
                    return true;

                // Also hide anything from the BepInEx plugins folder
                string location = "";
                try { location = asm.Location; } catch { }
                if (!string.IsNullOrEmpty(location) &&
                    (location.Contains("BepInEx") || location.Contains("plugins")))
                    return true;
            }
            catch { }
            finally { _checking = false; }
            return false;
        }

        // ── Assembly.Location spoofing ────────────────────────────────

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.Location), MethodType.Getter)]
        public class LocationPatch
        {
            static void Postfix(Assembly __instance, ref string __result)
            {
                if (IsModAssembly(__instance))
                    __result = "";
            }
        }

        // ── Assembly.CodeBase spoofing ────────────────────────────────

#pragma warning disable SYSLIB0012 // CodeBase is obsolete
        [HarmonyPatch(typeof(Assembly), nameof(Assembly.CodeBase), MethodType.Getter)]
        public class CodeBasePatch
        {
            static void Postfix(Assembly __instance, ref string __result)
            {
                if (IsModAssembly(__instance))
                    __result = "";
            }
        }
#pragma warning restore SYSLIB0012

        // ── AppDomain.GetAssemblies filtering ─────────────────────────

        [HarmonyPatch(typeof(AppDomain), nameof(AppDomain.GetAssemblies), new Type[0])]
        public class GetAssembliesPatch
        {
            static void Postfix(ref Assembly[] __result)
            {
                if (__result == null) return;
                __result = __result.Where(a => !IsModAssembly(a)).ToArray();
            }
        }

        // ── Console.SetOut protection ─────────────────────────────────
        // Prevents the game from redirecting stdout to capture mod output.
        // Only blocks calls originating from game/anti-cheat code, not our own.

        [HarmonyPatch(typeof(Console), nameof(Console.SetOut))]
        public class ConsoleSetOutPatch
        {
            static bool Prefix()
            {
                // Check if the caller is game code trying to hijack stdout
                var stackTrace = new System.Diagnostics.StackTrace();
                for (int i = 1; i < stackTrace.FrameCount && i < 6; i++)
                {
                    var method = stackTrace.GetFrame(i)?.GetMethod();
                    string ns = method?.DeclaringType?.Namespace ?? "";
                    // Allow our own code and BepInEx to call SetOut
                    if (ns.StartsWith("SignalMenu") || ns.StartsWith("BepInEx"))
                        return true;
                }
                return false; // Block game code from redirecting stdout
            }
        }
    }
}
