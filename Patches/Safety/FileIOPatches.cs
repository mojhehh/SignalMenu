/*
 * Signal Safety Menu  Patches/Safety/FileIOPatches.cs
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
using System.IO;
using System.Text;

namespace SignalMenu.Patches.Safety
{
    /// <summary>
    /// Intercepts all System.IO.File write operations to prevent the game from
    /// writing detection evidence, crash logs, or anti-cheat telemetry to disk.
    /// Only blocks writes to game-related directories; allows writes elsewhere
    /// (e.g. mod config files) to pass through normally.
    /// </summary>
    public class FileIOPatches
    {
        private static readonly HashSet<string> BlockedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static string _gameDir;

        public static void Initialize(string gorillaTagPath)
        {
            _gameDir = gorillaTagPath?.ToLowerInvariant() ?? "";
            // Block writes to the game's own directories that could store evidence
            BlockedDirectories.Add(Path.Combine(gorillaTagPath, "Logs"));
            BlockedDirectories.Add(Path.Combine(gorillaTagPath, "CrashDumps"));
            BlockedDirectories.Add(Path.Combine(gorillaTagPath, "ErrorLogs"));
        }

        /// <summary>
        /// Checks if a file path is a game telemetry/logging path that should be blocked.
        /// </summary>
        private static bool ShouldBlockWrite(string path)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(_gameDir))
                return false;

            try
            {
                string fullPath = Path.GetFullPath(path).ToLowerInvariant();

                // Block writes to known telemetry/evidence file patterns
                string fileName = Path.GetFileName(fullPath);
                if (fileName.Contains("report") ||
                    fileName.Contains("telemetry") ||
                    fileName.Contains("crash") ||
                    fileName.Contains("anticheat") ||
                    fileName.Contains("monkeagent") ||
                    fileName.Contains("gorilla_not") ||
                    fileName.Contains("violation"))
                    return true;

                // Block writes to game log directories
                foreach (var dir in BlockedDirectories)
                {
                    if (fullPath.StartsWith(dir.ToLowerInvariant()))
                        return true;
                }
            }
            catch
            {
                // If we can't parse the path, don't block it
            }

            return false;
        }

        // ── WriteAllText ──────────────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.WriteAllText), typeof(string), typeof(string))]
        public class WriteAllText1
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.WriteAllText), typeof(string), typeof(string), typeof(Encoding))]
        public class WriteAllText2
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        // ── WriteAllLines ─────────────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.WriteAllLines), typeof(string), typeof(string[]))]
        public class WriteAllLines1
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.WriteAllLines), typeof(string), typeof(string[]), typeof(Encoding))]
        public class WriteAllLines2
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.WriteAllLines), typeof(string), typeof(IEnumerable<string>))]
        public class WriteAllLines3
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.WriteAllLines), typeof(string), typeof(IEnumerable<string>), typeof(Encoding))]
        public class WriteAllLines4
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        // ── WriteAllBytes ─────────────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.WriteAllBytes))]
        public class WriteAllBytes1
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        // ── AppendAllText ─────────────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.AppendAllText), typeof(string), typeof(string))]
        public class AppendAllText1
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.AppendAllText), typeof(string), typeof(string), typeof(Encoding))]
        public class AppendAllText2
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        // ── AppendAllLines ────────────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.AppendAllLines), typeof(string), typeof(IEnumerable<string>))]
        public class AppendAllLines1
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.AppendAllLines), typeof(string), typeof(IEnumerable<string>), typeof(Encoding))]
        public class AppendAllLines2
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        // ── Copy / Move / Replace ─────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.Copy), typeof(string), typeof(string))]
        public class Copy1
        {
            static bool Prefix(string sourceFileName, string destFileName) => !ShouldBlockWrite(destFileName);
        }

        [HarmonyPatch(typeof(File), nameof(File.Copy), typeof(string), typeof(string), typeof(bool))]
        public class Copy2
        {
            static bool Prefix(string sourceFileName, string destFileName) => !ShouldBlockWrite(destFileName);
        }

        [HarmonyPatch(typeof(File), nameof(File.Move), typeof(string), typeof(string))]
        public class Move1
        {
            static bool Prefix(string sourceFileName, string destFileName) => !ShouldBlockWrite(destFileName);
        }

        [HarmonyPatch(typeof(File), nameof(File.Replace), typeof(string), typeof(string), typeof(string))]
        public class Replace1
        {
            static bool Prefix(string sourceFileName, string destinationFileName) => !ShouldBlockWrite(destinationFileName);
        }

        [HarmonyPatch(typeof(File), nameof(File.Replace), typeof(string), typeof(string), typeof(string), typeof(bool))]
        public class Replace2
        {
            static bool Prefix(string sourceFileName, string destinationFileName) => !ShouldBlockWrite(destinationFileName);
        }

        // ── Create ────────────────────────────────────────────────────

        [HarmonyPatch(typeof(File), nameof(File.Create), typeof(string))]
        public class Create1
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.Create), typeof(string), typeof(int))]
        public class Create2
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }

        [HarmonyPatch(typeof(File), nameof(File.Create), typeof(string), typeof(int), typeof(FileOptions))]
        public class Create3
        {
            static bool Prefix(string path) => !ShouldBlockWrite(path);
        }
    }
}
