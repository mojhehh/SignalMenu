/*
 * Signal Safety Menu  Plugin.cs
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

using BepInEx;
using BepInEx.Logging;
using SignalMenu.Classes.Menu;
using SignalMenu.Managers;
using SignalMenu.Menu;
using SignalMenu.Patches;
using SignalMenu.Patches.Menu;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SignalMenu.SignalSafety;
using Console = System.Console;

namespace SignalMenu
{
    [Description(PluginInfo.Description)]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public static ManualLogSource PluginLogger => instance.Logger;
        public static bool FirstLaunch;

        private void Awake()
        {
            instance = this;

            LogManager.Log($"[{PluginInfo.Name}] v{PluginInfo.Version} loaded");

            FirstLaunch = !Directory.Exists(PluginInfo.BaseDirectory);

            string[] ExistingDirectories = {
                "",
                "/Sounds",
                "/Plugins",
                "/Backups",
                "/Macros",
                "/TTS",
                "/PlayerInfo",
                "/CustomScripts",
                "/Friends",
                "/Friends/Messages",
                "/Achievements"
            };

            foreach (string DirectoryString in ExistingDirectories)
            {
                string DirectoryTarget = $"{PluginInfo.BaseDirectory}{DirectoryString}";
                if (!Directory.Exists(DirectoryTarget))
                    Directory.CreateDirectory(DirectoryTarget);
            }

            // Ugily hard-coded but works so well
            if (File.Exists($"{PluginInfo.BaseDirectory}/prefs.dat"))
            {
                var lines = File.ReadAllLines($"{PluginInfo.BaseDirectory}/prefs.dat");
                if (lines.Length > 0 && lines[0].Split(";;").Contains("Accept TOS"))
                    TOSPatches.enabled = true;
            }

            if (File.Exists($"{PluginInfo.BaseDirectory}/notelem.dat"))
                ServerData.DisableTelemetry = true;
            
            GorillaTagger.OnPlayerSpawned(LoadMenu);
        }

        private void OnDestroy() =>
            Main.UnloadMenu();

        private static void LoadMenu()
        {
            // Initialize file I/O safety filtering before patches are applied
            try
            {
                string gamePath = Assembly.GetExecutingAssembly().Location.Replace("\\", "/").Split("/BepInEx")[0];
                Patches.Safety.FileIOPatches.Initialize(gamePath);
            }
            catch (Exception ex) { Managers.LogManager.LogError($"FileIOPatches init failed: {ex.Message}"); }

            PatchHandler.PatchAll();

            GameObject Loader = new GameObject(ObjectNames.Get("Loader"));
            Loader.AddComponent<CoroutineManager>();
            Loader.AddComponent<NotificationManager>();
            Loader.AddComponent<CustomBoardManager>();

            Loader.AddComponent<UI>();

            var safetyRunner = Loader.AddComponent<SignalSafety.SafetyLoader>();
            SignalSafety.SafetyLoader.Initialize(safetyRunner);

            DontDestroyOnLoad(Loader);
        }

        // For SharpMonoInjector usage
        // Don't merge these methods, it just doesn't work
        public static void Inject()
        {
            GameObject menuObj = new GameObject(ObjectNames.Get("Inject"));
            menuObj.AddComponent<Plugin>();
        }

        public static void InjectDontDestroy()
        {
            GameObject menuObj = new GameObject(ObjectNames.Get("Inject"));
            menuObj.AddComponent<Plugin>();
            DontDestroyOnLoad(menuObj);
        }
    }
}
