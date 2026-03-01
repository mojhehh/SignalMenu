/*
 * Signal Safety Menu  Utilities/FileUtilities.cs
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

using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SignalMenu.Utilities
{
    public class FileUtilities
    {
        public static string GetFileExtension(string fileName)
        {
            var cleanName = fileName.Split('?')[0];
            return Path.GetExtension(cleanName).TrimStart('.').ToLower();
        }

        public static string RemoveLastDirectory(string directory) =>
            directory == "" || directory.LastIndexOf('/') <= 0 ? "" : directory[..directory.LastIndexOf('/')];

        public static string RemoveFileExtension(string file)
        {
            if (string.IsNullOrEmpty(file)) return "";
            int lastDot = file.LastIndexOf('.');
            if (lastDot <= 0) return file; // No extension or dot at start (hidden file)
            return file[..lastDot];
        }

        public static AudioType GetAudioType(string extension)
        {
            return extension.ToLower() switch
            {
                "mp3" => AudioType.MPEG,
                "wav" => AudioType.WAV,
                "ogg" => AudioType.OGGVORBIS,
                "aiff" => AudioType.AIFF,
                _ => AudioType.WAV,
            };
        }

        public static string GetFullPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        public static string GetGamePath() =>
            Assembly.GetExecutingAssembly().Location.Replace("\\", "/").Split("/BepInEx")[0];

        public static string SanitizeFileName(string input)
        {
            input = input.Trim();
            char[] illegalChars = Path.GetInvalidFileNameChars();
            input = illegalChars.Aggregate(input, (current, c) => current.Replace(c, '_'));

            input = input.Replace("../", "")
                         .Replace("..\\", "")
                         .Replace("./", "")
                         .Replace(".\\", "");

            input = input.Replace(":", "")
                         .Replace("\\", "")
                         .Replace("/", "");

            if (input.Length > 64)
                input = input[..64];

            if (string.IsNullOrWhiteSpace(input))
                input = "file"; // fallback

            return input;
        }
    }
}