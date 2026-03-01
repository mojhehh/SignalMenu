/*
 * Signal Safety Menu  Patches/Safety/AntiTeleportPatches.cs
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

using GorillaLocomotion;
using HarmonyLib;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Bypasses GTPlayer.AntiTeleportTechnology which detects large position changes
    /// between frames (teleportation). Without this patch, any teleport/fly mod would
    /// be snapped back to the last valid position by the client-side anti-teleport check.
    /// </summary>
    public class AntiTeleportPatches
    {
        public static bool enabled = true;

        [HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.AntiTeleportTechnology))]
        public class AntiTeleportBypass
        {
            static bool Prefix() => !enabled;
        }
    }
}
