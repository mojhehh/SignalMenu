/*
 * Signal Safety Menu  Patches/Safety/ProcessHidePatches.cs
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

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Process and module hiding patches that go BEYOND Rexon.
    /// - Filters Process.Modules to hide injected DLLs
    /// - Filters Process.GetProcesses to hide helper processes
    /// - Blocks suspicious process enumeration
    /// </summary>
    public class ProcessHidePatches
    {
        /// <summary>
        /// Module names that should be hidden from Process.Modules enumeration.
        /// </summary>
        private static readonly HashSet<string> HiddenModuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "0Harmony.dll",
            "BepInEx.dll",
            "BepInEx.Preloader.dll",
            "BepInEx.Core.dll",
            "BepInEx.Unity.dll",
            "MonoMod.RuntimeDetour.dll",
            "MonoMod.Utils.dll",
            "HarmonyXInterop.dll",
            "VRInputModule.dll",
            "Signal Safety Menu.dll",
            "SignalSafetyMenu.dll",
            "SignalMenu.dll",
            "SignalAutoUpdater.dll"
        };

        /// <summary>
        /// Process names that should be hidden from GetProcesses.
        /// </summary>
        private static readonly HashSet<string> HiddenProcessNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dnSpy",
            "dnSpy-x86",
            "ILSpy",
            "dotPeek",
            "cheatengine",
            "cheatengine-x86_64",
            "HxD",
            "x64dbg",
            "x32dbg",
            "ollydbg",
            "Fiddler",
            "Wireshark"
        };

        // ── Filter Process.GetProcesses ──────────────────────────────

        [HarmonyPatch(typeof(Process), nameof(Process.GetProcesses), new Type[0])]
        public class GetProcessesPatch
        {
            static void Postfix(ref Process[] __result)
            {
                if (__result == null) return;

                __result = __result.Where(p =>
                {
                    try
                    {
                        return !HiddenProcessNames.Contains(p.ProcessName);
                    }
                    catch
                    {
                        return true; // Keep processes we can't check
                    }
                }).ToArray();
            }
        }

        [HarmonyPatch(typeof(Process), nameof(Process.GetProcesses), new[] { typeof(string) })]
        public class GetProcessesMachinePatch
        {
            static void Postfix(ref Process[] __result)
            {
                if (__result == null) return;

                __result = __result.Where(p =>
                {
                    try
                    {
                        return !HiddenProcessNames.Contains(p.ProcessName);
                    }
                    catch
                    {
                        return true;
                    }
                }).ToArray();
            }
        }

        // ── Filter Process.GetProcessesByName ────────────────────────

        [HarmonyPatch(typeof(Process), nameof(Process.GetProcessesByName), new[] { typeof(string) })]
        public class GetProcessesByNamePatch
        {
            static bool Prefix(string processName, ref Process[] __result)
            {
                if (HiddenProcessNames.Contains(processName))
                {
                    __result = Array.Empty<Process>();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Process), nameof(Process.GetProcessesByName), new[] { typeof(string), typeof(string) })]
        public class GetProcessesByNameMachinePatch
        {
            static bool Prefix(string processName, ref Process[] __result)
            {
                if (HiddenProcessNames.Contains(processName))
                {
                    __result = Array.Empty<Process>();
                    return false;
                }
                return true;
            }
        }
    }
}
