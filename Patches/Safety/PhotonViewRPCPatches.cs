/*
 * Signal Safety Menu  Patches/Safety/PhotonViewRPCPatches.cs
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
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Intercepts PhotonView.RPC and PhotonView.RpcSecure at the view level
    /// to filter suspicious outgoing RPCs that could trigger anti-cheat detection.
    /// This is a safety layer below the menu's RPCFilter (which patches PhotonNetwork.RPC).
    /// </summary>
    public class PhotonViewRPCPatches
    {
        /// <summary>
        /// RPC method names that should be silently blocked to prevent anti-cheat triggering.
        /// </summary>
        public static HashSet<string> BlockedRPCs = new HashSet<string>();

        /// <summary>
        /// RPC method names that are rate-limited. Stores the last send time per RPC name.
        /// </summary>
        private static readonly Dictionary<string, float> RpcTimestamps = new Dictionary<string, float>();

        /// <summary>
        /// Minimum interval between identical RPCs (in seconds). 0 = no limit.
        /// </summary>
        public static float RateLimitInterval = 0f;

        private static bool ShouldAllowRPC(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                return true;

            // Block explicitly blocked RPCs
            if (BlockedRPCs.Contains(methodName))
                return false;

            // Rate limiting
            if (RateLimitInterval > 0f)
            {
                float now = UnityEngine.Time.unscaledTime;
                if (RpcTimestamps.TryGetValue(methodName, out float lastTime))
                {
                    if (now - lastTime < RateLimitInterval)
                        return false;
                }
                RpcTimestamps[methodName] = now;
            }

            return true;
        }

        // ── PhotonView.RPC (string, RpcTarget, params object[]) ──────

        [HarmonyPatch(typeof(PhotonView), nameof(PhotonView.RPC), typeof(string), typeof(RpcTarget), typeof(object[]))]
        public class RPC1
        {
            static bool Prefix(string methodName) => ShouldAllowRPC(methodName);
        }

        // ── PhotonView.RPC (string, Player, params object[]) ─────────

        [HarmonyPatch(typeof(PhotonView), nameof(PhotonView.RPC), typeof(string), typeof(Player), typeof(object[]))]
        public class RPC2
        {
            static bool Prefix(string methodName) => ShouldAllowRPC(methodName);
        }

        // ── PhotonView.RpcSecure (string, RpcTarget, bool, params object[]) ──

        [HarmonyPatch(typeof(PhotonView), nameof(PhotonView.RpcSecure), typeof(string), typeof(RpcTarget), typeof(bool), typeof(object[]))]
        public class RpcSecure1
        {
            static bool Prefix(string methodName) => ShouldAllowRPC(methodName);
        }

        // ── PhotonView.RpcSecure (string, Player, bool, params object[]) ─────

        [HarmonyPatch(typeof(PhotonView), nameof(PhotonView.RpcSecure), typeof(string), typeof(Player), typeof(bool), typeof(object[]))]
        public class RpcSecure2
        {
            static bool Prefix(string methodName) => ShouldAllowRPC(methodName);
        }
    }
}
