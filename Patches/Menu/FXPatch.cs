/*
 * Signal Safety Menu  Patches/Menu/FXPatch.cs
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

﻿using HarmonyLib;
using SignalMenu.Menu;

namespace SignalMenu.Patches.Menu
{
    [HarmonyPatch(typeof(FXSystem), nameof(FXSystem.PlayFXForRig), typeof(FXType), typeof(IFXContext), typeof(PhotonMessageInfoWrapped))]
    public class FXPatch
    {
        public static bool Prefix(FXType fxType, IFXContext context, PhotonMessageInfoWrapped info = default(PhotonMessageInfoWrapped))
        {
            NetPlayer player = info.Sender;
            if (player != null && Main.ShouldBypassChecks(player))
            {
                context.OnPlayFX();
                return false;
            }

            return true;
        }
    }
}
