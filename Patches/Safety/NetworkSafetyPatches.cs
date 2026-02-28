/*
 * Signal Safety Menu  Patches/Safety/NetworkSafetyPatches.cs
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

using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Advanced network safety layer that goes BEYOND what Rexon does.
    /// - Filters suspicious custom properties that could flag the anti-cheat
    /// - Blocks outgoing events that could leak mod state
    /// - Sanitizes room properties before they're sent
    /// </summary>
    public class NetworkSafetyPatches
    {
        /// <summary>
        /// Property keys that should be stripped from outgoing custom properties
        /// to prevent anti-cheat from reading mod-related data.
        /// </summary>
        private static readonly HashSet<string> SuspiciousPropertyKeys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "cheater", "modded", "hacker", "exploit", "signal", "menu",
            "detected", "violation", "banned", "kicked"
        };

        /// <summary>
        /// Photon event codes that should be blocked from outgoing traffic.
        /// These are custom event codes that anti-cheat systems may use.
        /// </summary>
        public static HashSet<byte> BlockedEventCodes = new HashSet<byte>();

        // ── Filter suspicious custom properties from being set ────────

        [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.SetPlayerCustomProperties))]
        public class SetPlayerCustomPropertiesPatch
        {
            static void Prefix(ref Hashtable customProperties)
            {
                if (customProperties == null) return;

                var keysToRemove = new List<object>();
                foreach (var key in customProperties.Keys)
                {
                    string keyStr = key?.ToString()?.ToLowerInvariant() ?? "";
                    foreach (var suspicious in SuspiciousPropertyKeys)
                    {
                        if (keyStr.Contains(suspicious))
                        {
                            keysToRemove.Add(key);
                            break;
                        }
                    }
                }

                foreach (var key in keysToRemove)
                    customProperties.Remove(key);
            }
        }

        // ── Block suspicious outgoing Photon events ───────────────────

        [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.RaiseEvent))]
        public class RaiseEventPatch
        {
            static bool Prefix(byte eventCode)
            {
                if (BlockedEventCodes.Contains(eventCode))
                    return false;
                return true;
            }
        }

        // ── Prevent server-initiated disconnect from anti-cheat ───────

        [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.Disconnect))]
        public class DisconnectPatch
        {
            /// <summary>
            /// When false (default), anti-cheat triggered disconnects are blocked.
            /// Set to true to allow all disconnects (e.g., during intentional leave).
            /// </summary>
            public static bool AllowDisconnect = false;

            static bool Prefix()
            {
                if (AllowDisconnect)
                    return true; // User explicitly allowed this disconnect

                // Check if this disconnect was called from MonkeAgent or anti-cheat code
                var stackTrace = new System.Diagnostics.StackTrace();
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var method = stackTrace.GetFrame(i)?.GetMethod();
                    if (method == null) continue;
                    string declaringType = method.DeclaringType?.Name ?? "";
                    if (declaringType == "MonkeAgent" ||
                        declaringType == "GorillaNetworkPublicTestsJoin" ||
                        declaringType == "GorillaNetworkPublicTestJoin2")
                        return false; // Block anti-cheat triggered disconnects
                }
                return true;
            }
        }
    }
}
