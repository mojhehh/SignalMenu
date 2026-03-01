/*
 * Signal Safety Menu  PluginInfo.cs
 * A fork of ii's Stupid Menu - all credit to iiDk / Goldentrophy Software
 * Original: https://github.com/iiDk-the-actual/iis.Stupid.Menu
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

namespace SignalMenu
{
    public class PluginInfo
    {
        public const string GUID = "com.unity.xr.inputmodule";
        public const string Name = "VRInputModule";
        public const string Description = "";
        public const string BuildTimestamp = "2026-02-28T22:30:00Z";
        public const string Version = "1.2.3";

        public const string BaseDirectory = ".gtdata";
        public const string ClientResourcePath = "SignalMenu.Resources.Client";
        public const string ServerResourcePath = "https://raw.githubusercontent.com/mojhehh/SignalMenu/master/Resources/Server";
        public const string FallbackResourcePath = "https://raw.githubusercontent.com/iiDk-the-actual/iis.Stupid.Menu/master/Resources/Server";
        public const string ServerAPI = "";

        public const string OriginalCredit = "Based on ii's Stupid Menu by iiDk (https://github.com/iiDk-the-actual/iis.Stupid.Menu)";

        public const string Logo = @"   __  __           _
  |  \/  | ___ _ __| |_
  | |\/| |/ _ \ '__| __|
  | |  | |  __/ |  | |_
  |_|  |_|\___|_|   \__|";

#if DEBUG
        public static bool BetaBuild = true;
#else
        public static bool BetaBuild = false;
#endif
    }
}
