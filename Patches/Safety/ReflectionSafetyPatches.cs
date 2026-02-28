/*
 * Signal Safety Menu  Patches/Safety/ReflectionSafetyPatches.cs
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
using System.Reflection;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Reflection safety patches that go BEYOND Rexon.
    /// Prevents anti-cheat from discovering mod types, methods, or assemblies
    /// through reflection API calls.
    /// </summary>
    public class ReflectionSafetyPatches
    {
        /// <summary>
        /// Recursion guard: when true, reflection patches pass through unfiltered.
        /// This prevents breaking Harmony's internal type resolution and our own code.
        /// </summary>
        [ThreadStatic] private static bool _bypassing;

        /// <summary>
        /// Type name prefixes that should be hidden from Type.GetType calls.
        /// </summary>
        private static readonly HashSet<string> HiddenTypeNamespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SignalMenu",
            "HarmonyLib",
            "MonoMod",
            "BepInEx"
        };

        /// <summary>
        /// Check if a type should be hidden from reflection.
        /// </summary>
        private static bool ShouldHideType(Type type)
        {
            if (type == null || _bypassing) return false;
            string ns = type.Namespace ?? "";
            string fullName = type.FullName ?? "";

            foreach (var hidden in HiddenTypeNamespaces)
            {
                if (ns.StartsWith(hidden, StringComparison.OrdinalIgnoreCase) ||
                    fullName.StartsWith(hidden, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Also hide dynamically generated Harmony types
            if (fullName.StartsWith("patch_", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("DMD<", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the caller is our own code or Harmony internals — if so, don't filter.
        /// </summary>
        private static bool IsInternalCaller()
        {
            var trace = new System.Diagnostics.StackTrace(false);
            for (int i = 2; i < trace.FrameCount && i < 8; i++)
            {
                var method = trace.GetFrame(i)?.GetMethod();
                string ns = method?.DeclaringType?.Namespace ?? "";
                if (ns.StartsWith("SignalMenu") || ns.StartsWith("HarmonyLib") || ns.StartsWith("MonoMod") || ns.StartsWith("BepInEx"))
                    return true;
            }
            return false;
        }

        // ── Filter Type.GetType to return null for mod types ─────────

        [HarmonyPatch(typeof(Type), nameof(Type.GetType), new[] { typeof(string) })]
        public class TypeGetTypePatch
        {
            static void Postfix(ref Type __result)
            {
                if (ShouldHideType(__result))
                    __result = null;
            }
        }

        [HarmonyPatch(typeof(Type), nameof(Type.GetType), new[] { typeof(string), typeof(bool) })]
        public class TypeGetTypeBoolPatch
        {
            static void Postfix(ref Type __result)
            {
                if (ShouldHideType(__result))
                    __result = null;
            }
        }

        [HarmonyPatch(typeof(Type), nameof(Type.GetType), new[] { typeof(string), typeof(bool), typeof(bool) })]
        public class TypeGetTypeBoolBoolPatch
        {
            static void Postfix(ref Type __result)
            {
                if (ShouldHideType(__result))
                    __result = null;
            }
        }

        // ── Filter Assembly.GetTypes to exclude mod types ────────────

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetTypes), new Type[0])]
        public class AssemblyGetTypesPatch
        {
            static void Postfix(ref Type[] __result)
            {
                if (__result == null || __result.Length == 0 || _bypassing) return;
                if (IsInternalCaller()) return; // Don't hide types from our own code or Harmony

                _bypassing = true;
                try
                {
                    var filtered = new List<Type>(__result.Length);
                    foreach (var type in __result)
                    {
                        if (!ShouldHideType(type))
                            filtered.Add(type);
                    }

                    if (filtered.Count != __result.Length)
                        __result = filtered.ToArray();
                }
                finally { _bypassing = false; }
            }
        }

        // ── Filter Assembly.GetExportedTypes ─────────────────────────

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetExportedTypes), new Type[0])]
        public class AssemblyGetExportedTypesPatch
        {
            static void Postfix(ref Type[] __result)
            {
                if (__result == null || __result.Length == 0 || _bypassing) return;
                if (IsInternalCaller()) return;

                _bypassing = true;
                try
                {
                    var filtered = new List<Type>(__result.Length);
                    foreach (var type in __result)
                    {
                        if (!ShouldHideType(type))
                            filtered.Add(type);
                    }

                    if (filtered.Count != __result.Length)
                        __result = filtered.ToArray();
                }
                finally { _bypassing = false; }
            }
        }

        // ── Hide Assembly.GetType for single type lookups ────────────

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetType), new[] { typeof(string) })]
        public class AssemblyGetTypePatch
        {
            static void Postfix(ref Type __result)
            {
                if (ShouldHideType(__result))
                    __result = null;
            }
        }

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetType), new[] { typeof(string), typeof(bool) })]
        public class AssemblyGetTypeBoolPatch
        {
            static void Postfix(ref Type __result)
            {
                if (ShouldHideType(__result))
                    __result = null;
            }
        }

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetType), new[] { typeof(string), typeof(bool), typeof(bool) })]
        public class AssemblyGetTypeBoolBoolPatch
        {
            static void Postfix(ref Type __result)
            {
                if (ShouldHideType(__result))
                    __result = null;
            }
        }
    }
}
