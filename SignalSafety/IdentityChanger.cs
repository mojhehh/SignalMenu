using System;
using System.Security.Cryptography;
using UnityEngine;
using Photon.Pun;

namespace SignalMenu.SignalSafety
{
    public static class IdentityChanger
    {
        private static string originalName = null;
        private static string currentName = null;
        private static bool _wasInRoom = false;
        private static float _lastSoundTime = -999f;
        private const float SOUND_COOLDOWN = 5f;

        private static readonly string[] FirstNames = new string[]
        {
            "Alex", "Jordan", "Sam", "Taylor", "Morgan", "Casey", "Riley", "Quinn", "Avery", "Blake",
            "Cameron", "Dakota", "Drew", "Eli", "Finn", "Harper", "Hunter", "Jamie", "Jesse", "Kai",
            "Logan", "Max", "Noah", "Owen", "Parker", "Reece", "River", "Rowan", "Sage", "Skyler",
            "Tyler", "Will", "Zach", "Ben", "Chris", "Dan", "Jack", "Luke", "Mike", "Tom",
            "Ryan", "Kyle", "Matt", "Nick", "Jake", "Ethan", "Liam", "Mason", "Aiden", "Dylan"
        };

        private static readonly string[] Prefixes = new string[]
        {
            "EPIC", "REAL", "MONKE", "BIG", "LIL", "PRO", "MR", "COOL", "MEGA", "ULTRA",
            "BABY", "KING", "DARK", "ICE", "FIRE", "WILD", "FAST", "SUS", "CRZY", "GOAT"
        };

        private static readonly string[] BaseNames = new string[]
        {
            "SHIBA", "PBBV", "BANANA", "GORILLA", "MONKE", "APE", "CHIMP", "FOREST",
            "TREE", "LEAF", "BRANCH", "ROCKET", "CRYSTAL", "SHADOW", "NINJA", "PIXEL",
            "CLOUD", "STORM", "FLASH", "BUZZ", "SPIKE", "FROST", "BLAZE", "PHANTOM"
        };

        private static readonly string[] Suffixes = new string[]
        {
            "", "", "", "", "",
            "YT", "TTV", "VR", "XD", "GG", "BTW", "FTW", "OG", "HD",
            "_plays", "_vr", "_gt", "420", "69", "123", "99", "07", "08",
            "2k", "3k", "x", "xx"
        };

        private static int SecureRand(int max)
        {
            byte[] buf = new byte[4];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(buf);
            return (int)(BitConverter.ToUInt32(buf, 0) % (uint)max);
        }

        public static string GenerateRandomName()
        {
            string name;
            if (SecureRand(2) == 0)
            {
                name = FirstNames[SecureRand(FirstNames.Length)] + Suffixes[SecureRand(Suffixes.Length)];
                if (SecureRand(10) < 3) name += (SecureRand(999) + 1).ToString();
            }
            else
            {
                if (SecureRand(2) == 0)
                    name = Prefixes[SecureRand(Prefixes.Length)] + BaseNames[SecureRand(BaseNames.Length)];
                else
                    name = BaseNames[SecureRand(BaseNames.Length)] + Suffixes[SecureRand(Suffixes.Length)];
            }

            if (name.Length > 20) name = name.Substring(0, 20);
            return name;
        }

        public static void ApplyRandomName()
        {
            ApplyName(GenerateRandomName());
            if (SafetyConfig.ColorChangeEnabled) ApplyRandomColor();
        }

        public static void ApplyCustomName(string name)
        {
            if (string.IsNullOrEmpty(name)) { ApplyRandomName(); return; }

            string sanitized = "";
            foreach (char c in name)
                if (char.IsLetterOrDigit(c) || c == ' ') sanitized += c;

            if (sanitized.Length > 20) sanitized = sanitized.Substring(0, 20);
            if (string.IsNullOrEmpty(sanitized)) sanitized = GenerateRandomName();

            ApplyName(sanitized);
            if (SafetyConfig.ColorChangeEnabled) ApplyRandomColor();
        }

        private static void ApplyName(string name)
        {
            try
            {
                if (originalName == null && PhotonNetwork.LocalPlayer != null)
                    originalName = PhotonNetwork.LocalPlayer.NickName;

                currentName = name;

                if (PhotonNetwork.LocalPlayer != null)
                {
                    PhotonNetwork.LocalPlayer.NickName = name;
                    Plugin.Instance?.Log($"[Identity] Name changed to: {name}");
                }

                if (GorillaNetworking.GorillaComputer.instance != null)
                {
                    GorillaNetworking.GorillaComputer.instance.savedName = name;
                    GorillaNetworking.GorillaComputer.instance.currentName = name;
                }

                if (Time.time - _lastSoundTime >= SOUND_COOLDOWN)
                {
                    AudioManager.Play("identity_changed", AudioManager.AudioCategory.Protection);
                    _lastSoundTime = Time.time;
                }
            }
            catch (Exception ex)
            {
                Plugin.Instance?.Log($"[Identity] Error: {ex.Message}");
            }
        }

        public static void ApplyRandomColor()
        {
            try
            {
                byte[] buf = new byte[12];
                using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(buf);
                float r = BitConverter.ToUInt32(buf, 0) / (float)uint.MaxValue;
                float g = BitConverter.ToUInt32(buf, 4) / (float)uint.MaxValue;
                float b = BitConverter.ToUInt32(buf, 8) / (float)uint.MaxValue;
                GorillaTagger.Instance.UpdateColor(r, g, b);
                PlayerPrefs.SetFloat("redValue", r);
                PlayerPrefs.SetFloat("greenValue", g);
                PlayerPrefs.SetFloat("blueValue", b);
                PlayerPrefs.Save();
                Plugin.Instance?.Log($"[Identity] Color changed: ({r:F2}, {g:F2}, {b:F2})");
            }
            catch { }
        }
        public static void CheckDisconnect()
        {
            bool inRoom = false;
            try { inRoom = NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom; } catch { }

            if (!SafetyConfig.ChangeIdentityOnDisconnect || !SafetyConfig.IdentityChangeEnabled)
            {
                _wasInRoom = inRoom;
                return;
            }

            if (_wasInRoom && !inRoom)
            {
                try
                {
                    if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected ||
                        PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.PeerCreated)
                    {
                        ApplyRandomName();
                        Plugin.Instance?.Log("[Identity] Auto-changed identity on disconnect");
                    }
                    else { return; }
                }
                catch
                {
                    ApplyRandomName();
                }
            }
            _wasInRoom = inRoom;
        }

        public static void RestoreOriginalName()
        {
            if (!string.IsNullOrEmpty(originalName))
            {
                ApplyName(originalName);
                currentName = originalName;
            }
        }

        public static string GetCurrentName()
        {
            if (!string.IsNullOrEmpty(currentName)) return currentName;
            if (PhotonNetwork.LocalPlayer != null) return PhotonNetwork.LocalPlayer.NickName;
            return "Unknown";
        }
    }
}