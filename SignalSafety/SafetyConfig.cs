using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace SignalMenu.SignalSafety
{
    public static class SafetyConfig
    {
        private static string ConfigPath => Path.Combine(Application.persistentDataPath, ".gl_settings");
        private static string ThemeConfigPath => Path.Combine(Application.persistentDataPath, ".gl_theme");

        public static bool AudioEnabled = true;
        public static bool PlayProtectionAudio = true;
        public static bool PlayWarningAudio = true;
        public static bool PlayBanAudio = true;
        public static bool PlayToggleAudio = true;
        public static bool PlayMenuDetectionAudio = true;
        public static bool PlayPatchOverrideAudio = true;
        public static float AudioVolume = 1.0f;
        public static float ProtectionVolume = 1.0f;
        public static float WarningVolume = 1.0f;
        public static float BanVolume = 1.0f;

        public static ButtonMapper.MenuButton MenuOpenButton = ButtonMapper.MenuButton.B_Right;

        public static bool CoreProtectionEnabled = true;

        public static bool AntiReportEnabled = true;
        public static bool IdentityChangeEnabled = false;
        public static bool TelemetryBlockEnabled = true;
        public static bool PlayFabBlockEnabled = true;
        public static bool DeviceSpoofEnabled = true;
        public static bool NetworkEventBlockEnabled = true;
        public static bool RPCLimitBypassEnabled = true;
        public static bool GraceBypassEnabled = true;
        public static bool KIDBypassEnabled = true;
        public static bool NameBanBypassEnabled = true;
        public static bool ErrorLoggingEnabled = false;

        public static string CustomName = "";

        public static bool AntiContentCreatorEnabled = true;
        public static bool CosmeticNotificationsEnabled = true;
        public static bool AutomodBypassEnabled = true;
        public static bool AntiPredictionsEnabled = false;
        public static bool AntiLurkerEnabled = true;
        public static bool AutoGCEnabled = true;
        public static bool SupportPageSpoofEnabled = true;
        public static bool RankedSpoofEnabled = false;
        public static bool ChangeIdentityOnDisconnect = false;
        public static bool ColorChangeEnabled = false;

        public static bool FPSSpoofEnabled = false;
        public static int SpoofedFPS = 72;
        public static bool TOSBypassEnabled = true;
        public static bool AntiNameBanEnabled = true;

        public static bool AntiReportSmartMode = true;
        public static bool AntiReportVisualizerEnabled = true;
        public static bool AntiReportMuteDetect = false;
        public static int AntiReportRangeIndex = 0;

        public static bool ModeratorDetectorEnabled = true;
        public static bool FakeOculusMenuEnabled = false;
        public static bool FakeBrokenControllerEnabled = false;
        public static bool FakeReportMenuEnabled = false;
        public static bool FakeValveTrackingEnabled = false;

        public static bool AntiCrashEnabled = true;

        public static bool AntiKickEnabled = true;

        public static bool ShowACReportsEnabled = true;

        public static bool AntiAFKKickEnabled = true;
        public static bool AntiPauseDisconnectEnabled = true;
        public static bool VersionBypassEnabled = true;
        public static bool BlockModAccountSave = true;

        public static bool AntiBanEnabled = false;

        public static int AntiReportMode = 0;

        public static bool MenuDetectionEnabled = true;
        public static bool MenuDetectionAlertEnabled = true;
        public static bool AutoOverrideOnDetection = true;

        public static int ThemeIndex = 0;
        public static bool UseCustomTheme = false;
        public static Color CustomPanelColor = new Color(0.06f, 0.07f, 0.12f, 0.94f);
        public static Color CustomAccentColor = new Color(0.2f, 0.75f, 0.95f, 1f);
        public static Color CustomTextColor = new Color(0.92f, 0.94f, 0.96f);

        public static bool PatchSendReport = true;
        public static bool PatchDispatchReport = true;
        public static bool PatchCheckReports = true;
        public static bool PatchTelemetry = true;
        public static bool PatchPlayFabReport = true;
        public static bool PatchCloseInvalidRoom = true;
        public static bool PatchGracePeriod = true;
        public static bool PatchRPCLimits = true;
        public static bool PatchQuitDelay = true;
        public static bool PatchModCheckers = true;
        public static bool PatchBanDetection = true;
        public static bool PatchFailureText = true;
        public static bool PatchBadNameCheck = true;
        public static bool PatchAutoBanList = true;

        private static bool _firstOpen = false;
        public static bool IsFirstOpen => _firstOpen;

        private static byte[] GetEncryptionKey()
        {
            string seed = "SC_" + Environment.UserName + "_SSM";
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));
            }
        }

        private static byte[] GetLegacyEncryptionKey()
        {
            string seed = "SC_" + Environment.UserName + "_" + Environment.MachineName;
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));
            }
        }

        private static string EncryptConfig(string plainText)
        {
            try
            {
                byte[] key = GetEncryptionKey();
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();
                    using (var enc = aes.CreateEncryptor())
                    {
                        byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                        byte[] cipherBytes = enc.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
                        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                        Array.Copy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch { return plainText; }
        }

        private static string DecryptConfig(string cipherText, byte[] key = null)
        {
            try
            {
                if (key == null) key = GetEncryptionKey();
                byte[] fullCipher = Convert.FromBase64String(cipherText);
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    byte[] iv = new byte[16];
                    byte[] cipher = new byte[fullCipher.Length - 16];
                    Array.Copy(fullCipher, 0, iv, 0, 16);
                    Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
                    aes.IV = iv;
                    using (var dec = aes.CreateDecryptor())
                    {
                        byte[] plainBytes = dec.TransformFinalBlock(cipher, 0, cipher.Length);
                        return System.Text.Encoding.UTF8.GetString(plainBytes);
                    }
                }
            }
            catch { return null; }
        }

        static SafetyConfig()
        {
            try
            {
                string testPath = Application.persistentDataPath;
                if (!string.IsNullOrEmpty(testPath))
                    Load();
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                string config = string.Join("|", new string[]
                {
                    AudioEnabled.ToString(),
                    PlayProtectionAudio.ToString(),
                    PlayWarningAudio.ToString(),
                    PlayBanAudio.ToString(),
                    PlayToggleAudio.ToString(),
                    AntiReportEnabled.ToString(),
                    IdentityChangeEnabled.ToString(),
                    TelemetryBlockEnabled.ToString(),
                    PlayFabBlockEnabled.ToString(),
                    DeviceSpoofEnabled.ToString(),
                    NetworkEventBlockEnabled.ToString(),
                    RPCLimitBypassEnabled.ToString(),
                    GraceBypassEnabled.ToString(),
                    CustomName.Replace("|", ""),
                    KIDBypassEnabled.ToString(),
                    NameBanBypassEnabled.ToString(),
                    AntiContentCreatorEnabled.ToString(),
                    CosmeticNotificationsEnabled.ToString(),
                    AutomodBypassEnabled.ToString(),
                    AntiPredictionsEnabled.ToString(),
                    AntiLurkerEnabled.ToString(),
                    AutoGCEnabled.ToString(),
                    SupportPageSpoofEnabled.ToString(),
                    RankedSpoofEnabled.ToString(),
                    ChangeIdentityOnDisconnect.ToString(),
                    ColorChangeEnabled.ToString()
                });
                config += "|" + string.Join("|", new string[]
                {
                    FPSSpoofEnabled.ToString(),
                    SpoofedFPS.ToString(),
                    TOSBypassEnabled.ToString(),
                    AntiNameBanEnabled.ToString(),
                    AntiReportSmartMode.ToString(),
                    AntiReportVisualizerEnabled.ToString(),
                    AntiReportMuteDetect.ToString(),
                    AntiReportRangeIndex.ToString(),
                    ModeratorDetectorEnabled.ToString(),
                    FakeOculusMenuEnabled.ToString(),
                    FakeBrokenControllerEnabled.ToString(),
                    CoreProtectionEnabled.ToString(),
                    ErrorLoggingEnabled.ToString(),
                    FakeReportMenuEnabled.ToString(),
                    FakeValveTrackingEnabled.ToString(),
                    AntiCrashEnabled.ToString(),
                    AntiReportMode.ToString(),
                    AntiKickEnabled.ToString(),
                    ShowACReportsEnabled.ToString()
                });

                config += "|" + string.Join("|", new string[]
                {
                    MenuDetectionEnabled.ToString(),
                    MenuDetectionAlertEnabled.ToString(),
                    AutoOverrideOnDetection.ToString(),
                    ThemeIndex.ToString(),
                    UseCustomTheme.ToString(),
                    PlayMenuDetectionAudio.ToString(),
                    PlayPatchOverrideAudio.ToString(),
                    AudioVolume.ToString("F2"),
                    ProtectionVolume.ToString("F2"),
                    WarningVolume.ToString("F2"),
                    BanVolume.ToString("F2"),
                    PatchSendReport.ToString(),
                    PatchDispatchReport.ToString(),
                    PatchCheckReports.ToString(),
                    PatchTelemetry.ToString(),
                    PatchPlayFabReport.ToString(),
                    PatchCloseInvalidRoom.ToString(),
                    PatchGracePeriod.ToString(),
                    PatchRPCLimits.ToString(),
                    PatchQuitDelay.ToString(),
                    PatchModCheckers.ToString(),
                    PatchBanDetection.ToString(),
                    PatchFailureText.ToString(),
                    PatchBadNameCheck.ToString(),
                    PatchAutoBanList.ToString()
                });

                config += "|" + string.Join("|", new string[]
                {
                    AntiAFKKickEnabled.ToString(),
                    AntiPauseDisconnectEnabled.ToString(),
                    VersionBypassEnabled.ToString(),
                    BlockModAccountSave.ToString()
                });

                config += "|" + AntiBanEnabled.ToString();
                config += "|" + ((int)MenuOpenButton).ToString();

                File.WriteAllText(ConfigPath, EncryptConfig(config));

                SaveThemeColors();
            }
            catch { }
        }

        private static void SaveThemeColors()
        {
            try
            {
                var inv = System.Globalization.CultureInfo.InvariantCulture;
                string data = string.Join("|", new string[]
                {
                    CustomPanelColor.r.ToString("F3", inv), CustomPanelColor.g.ToString("F3", inv),
                    CustomPanelColor.b.ToString("F3", inv), CustomPanelColor.a.ToString("F3", inv),
                    CustomAccentColor.r.ToString("F3", inv), CustomAccentColor.g.ToString("F3", inv),
                    CustomAccentColor.b.ToString("F3", inv), CustomAccentColor.a.ToString("F3", inv),
                    CustomTextColor.r.ToString("F3", inv), CustomTextColor.g.ToString("F3", inv),
                    CustomTextColor.b.ToString("F3", inv), CustomTextColor.a.ToString("F3", inv),
                });
                File.WriteAllText(ThemeConfigPath, data);
            }
            catch { }
        }

        private static void LoadThemeColors()
        {
            try
            {
                if (!File.Exists(ThemeConfigPath)) return;
                string data = File.ReadAllText(ThemeConfigPath).Trim();
                string[] p = data.Split('|');
                if (p.Length >= 12)
                {
                    var inv = System.Globalization.CultureInfo.InvariantCulture;
                    CustomPanelColor = new Color(float.Parse(p[0], inv), float.Parse(p[1], inv), float.Parse(p[2], inv), float.Parse(p[3], inv));
                    CustomAccentColor = new Color(float.Parse(p[4], inv), float.Parse(p[5], inv), float.Parse(p[6], inv), float.Parse(p[7], inv));
                    CustomTextColor = new Color(float.Parse(p[8], inv), float.Parse(p[9], inv), float.Parse(p[10], inv), float.Parse(p[11], inv));
                }
            }
            catch { }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    string oldPath = Path.Combine(Application.persistentDataPath, "gl_config_legacy.txt");
                    if (File.Exists(oldPath))
                    {
                        LoadFromString(File.ReadAllText(oldPath));
                        Save();
                        try { File.Delete(oldPath); } catch { }
                        return;
                    }
                    _firstOpen = true;
                    return;
                }

                string raw = File.ReadAllText(ConfigPath).Trim();
                if (string.IsNullOrEmpty(raw)) { _firstOpen = true; return; }

                string decrypted = DecryptConfig(raw);
                if (decrypted == null)
                {
                    decrypted = DecryptConfig(raw, GetLegacyEncryptionKey());
                }
                if (decrypted != null)
                {
                    LoadFromString(decrypted);
                    Save();
                }
                else
                {
                    LoadFromString(raw);
                    Save();
                }

                LoadThemeColors();
            }
            catch
            {
                _firstOpen = true;
            }
        }

        private static void LoadFromString(string data)
        {
            try
            {
                string[] parts = data.Split('|');
                if (parts.Length >= 5)
                {
                    if (bool.TryParse(parts[0], out bool a)) AudioEnabled = a;
                    if (bool.TryParse(parts[1], out bool b)) PlayProtectionAudio = b;
                    if (bool.TryParse(parts[2], out bool c)) PlayWarningAudio = c;
                    if (bool.TryParse(parts[3], out bool d)) PlayBanAudio = d;
                    if (bool.TryParse(parts[4], out bool e)) PlayToggleAudio = e;
                }
                if (parts.Length >= 13)
                {
                    if (bool.TryParse(parts[5], out bool f)) AntiReportEnabled = f;
                    if (bool.TryParse(parts[6], out bool g)) IdentityChangeEnabled = g;
                    if (bool.TryParse(parts[7], out bool h)) TelemetryBlockEnabled = h;
                    if (bool.TryParse(parts[8], out bool i)) PlayFabBlockEnabled = i;
                    if (bool.TryParse(parts[9], out bool j)) DeviceSpoofEnabled = j;
                    if (bool.TryParse(parts[10], out bool k)) NetworkEventBlockEnabled = k;
                    if (bool.TryParse(parts[11], out bool l)) RPCLimitBypassEnabled = l;
                    if (bool.TryParse(parts[12], out bool m)) GraceBypassEnabled = m;
                }
                if (parts.Length >= 14)
                {
                    CustomName = parts[13] ?? "";
                }
                if (parts.Length >= 16)
                {
                    if (bool.TryParse(parts[14], out bool n)) KIDBypassEnabled = n;
                    if (bool.TryParse(parts[15], out bool o)) NameBanBypassEnabled = o;
                }
                if (parts.Length >= 26)
                {
                    if (bool.TryParse(parts[16], out bool p)) AntiContentCreatorEnabled = p;
                    if (bool.TryParse(parts[17], out bool q)) CosmeticNotificationsEnabled = q;
                    if (bool.TryParse(parts[18], out bool r)) AutomodBypassEnabled = r;
                    if (bool.TryParse(parts[19], out bool s)) AntiPredictionsEnabled = s;
                    if (bool.TryParse(parts[20], out bool t)) AntiLurkerEnabled = t;
                    if (bool.TryParse(parts[21], out bool u)) AutoGCEnabled = u;
                    if (bool.TryParse(parts[22], out bool v)) SupportPageSpoofEnabled = v;
                    if (bool.TryParse(parts[23], out bool w)) RankedSpoofEnabled = w;
                    if (bool.TryParse(parts[24], out bool x)) ChangeIdentityOnDisconnect = x;
                    if (bool.TryParse(parts[25], out bool y)) ColorChangeEnabled = y;
                }
                if (parts.Length >= 30)
                {
                    if (bool.TryParse(parts[26], out bool fp)) FPSSpoofEnabled = fp;
                    if (int.TryParse(parts[27], out int fv)) SpoofedFPS = fv;
                    if (bool.TryParse(parts[28], out bool tb)) TOSBypassEnabled = tb;
                    if (bool.TryParse(parts[29], out bool nb)) AntiNameBanEnabled = nb;
                }
                if (parts.Length >= 39)
                {
                    if (bool.TryParse(parts[30], out bool sm)) AntiReportSmartMode = sm;
                    if (bool.TryParse(parts[31], out bool vis)) AntiReportVisualizerEnabled = vis;
                    if (bool.TryParse(parts[32], out bool md)) AntiReportMuteDetect = md;
                    if (int.TryParse(parts[33], out int ri)) AntiReportRangeIndex = ri;
                    if (bool.TryParse(parts[34], out bool mde)) ModeratorDetectorEnabled = mde;
                    if (bool.TryParse(parts[35], out bool fom)) FakeOculusMenuEnabled = fom;
                    if (bool.TryParse(parts[36], out bool fbc)) FakeBrokenControllerEnabled = fbc;
                    if (bool.TryParse(parts[37], out bool cp)) CoreProtectionEnabled = cp;
                    if (bool.TryParse(parts[38], out bool el)) ErrorLoggingEnabled = el;
                }
                if (parts.Length >= 43)
                {
                    if (bool.TryParse(parts[39], out bool frm)) FakeReportMenuEnabled = frm;
                    if (bool.TryParse(parts[40], out bool fvt)) FakeValveTrackingEnabled = fvt;
                    if (bool.TryParse(parts[41], out bool ace)) AntiCrashEnabled = ace;
                    if (int.TryParse(parts[42], out int arm)) AntiReportMode = arm;
                }
                if (parts.Length >= 45)
                {
                    if (bool.TryParse(parts[43], out bool ake)) AntiKickEnabled = ake;
                    if (bool.TryParse(parts[44], out bool sar)) ShowACReportsEnabled = sar;
                }

                if (parts.Length >= 48)
                {
                    if (bool.TryParse(parts[45], out bool mde2)) MenuDetectionEnabled = mde2;
                    if (bool.TryParse(parts[46], out bool mda)) MenuDetectionAlertEnabled = mda;
                    if (bool.TryParse(parts[47], out bool aoo)) AutoOverrideOnDetection = aoo;
                }
                if (parts.Length >= 50)
                {
                    if (int.TryParse(parts[48], out int ti)) ThemeIndex = ti;
                    if (bool.TryParse(parts[49], out bool uct)) UseCustomTheme = uct;
                }
                if (parts.Length >= 52)
                {
                    if (bool.TryParse(parts[50], out bool pmda)) PlayMenuDetectionAudio = pmda;
                    if (bool.TryParse(parts[51], out bool ppoa)) PlayPatchOverrideAudio = ppoa;
                }
                if (parts.Length >= 56)
                {
                    if (float.TryParse(parts[52], out float av)) AudioVolume = av;
                    if (float.TryParse(parts[53], out float pv)) ProtectionVolume = pv;
                    if (float.TryParse(parts[54], out float wv)) WarningVolume = wv;
                    if (float.TryParse(parts[55], out float bv)) BanVolume = bv;
                }
                if (parts.Length >= 70)
                {
                    if (bool.TryParse(parts[56], out bool psr)) PatchSendReport = psr;
                    if (bool.TryParse(parts[57], out bool pdr)) PatchDispatchReport = pdr;
                    if (bool.TryParse(parts[58], out bool pcr)) PatchCheckReports = pcr;
                    if (bool.TryParse(parts[59], out bool pt)) PatchTelemetry = pt;
                    if (bool.TryParse(parts[60], out bool ppr)) PatchPlayFabReport = ppr;
                    if (bool.TryParse(parts[61], out bool pcir)) PatchCloseInvalidRoom = pcir;
                    if (bool.TryParse(parts[62], out bool pgp)) PatchGracePeriod = pgp;
                    if (bool.TryParse(parts[63], out bool prl)) PatchRPCLimits = prl;
                    if (bool.TryParse(parts[64], out bool pqd)) PatchQuitDelay = pqd;
                    if (bool.TryParse(parts[65], out bool pmc)) PatchModCheckers = pmc;
                    if (bool.TryParse(parts[66], out bool pbd)) PatchBanDetection = pbd;
                    if (bool.TryParse(parts[67], out bool pft)) PatchFailureText = pft;
                    if (bool.TryParse(parts[68], out bool pbnc)) PatchBadNameCheck = pbnc;
                    if (bool.TryParse(parts[69], out bool pabl)) PatchAutoBanList = pabl;
                }
                if (parts.Length >= 74)
                {
                    if (bool.TryParse(parts[70], out bool aafk)) AntiAFKKickEnabled = aafk;
                    if (bool.TryParse(parts[71], out bool apd)) AntiPauseDisconnectEnabled = apd;
                    if (bool.TryParse(parts[72], out bool vbe)) VersionBypassEnabled = vbe;
                    if (bool.TryParse(parts[73], out bool bmas)) BlockModAccountSave = bmas;
                }
                if (parts.Length >= 75)
                {
                    if (bool.TryParse(parts[74], out bool abe)) AntiBanEnabled = abe;
                }
                if (parts.Length >= 76)
                {
                    if (int.TryParse(parts[75], out int mob) && mob >= 0 && mob <= 5)
                        MenuOpenButton = (ButtonMapper.MenuButton)mob;
                }
            }
            catch { }
        }

        public static void ResetToDefaults()
        {
            AudioEnabled = true;
            PlayProtectionAudio = true;
            PlayWarningAudio = true;
            PlayBanAudio = true;
            PlayToggleAudio = true;
            AntiReportEnabled = true;
            IdentityChangeEnabled = false;
            TelemetryBlockEnabled = true;
            PlayFabBlockEnabled = true;
            DeviceSpoofEnabled = true;
            NetworkEventBlockEnabled = true;
            RPCLimitBypassEnabled = true;
            GraceBypassEnabled = true;
            KIDBypassEnabled = true;
            NameBanBypassEnabled = true;
            CustomName = "";
            AntiContentCreatorEnabled = true;
            CosmeticNotificationsEnabled = true;
            AutomodBypassEnabled = true;
            AntiPredictionsEnabled = false;
            AntiLurkerEnabled = true;
            AutoGCEnabled = true;
            SupportPageSpoofEnabled = true;
            RankedSpoofEnabled = false;
            ChangeIdentityOnDisconnect = false;
            ColorChangeEnabled = false;
            FPSSpoofEnabled = false;
            SpoofedFPS = 72;
            TOSBypassEnabled = true;
            AntiNameBanEnabled = true;
            AntiReportSmartMode = true;
            AntiReportVisualizerEnabled = true;
            AntiReportMuteDetect = false;
            AntiReportRangeIndex = 0;
            ModeratorDetectorEnabled = true;
            FakeOculusMenuEnabled = false;
            FakeBrokenControllerEnabled = false;
            CoreProtectionEnabled = true;
            ErrorLoggingEnabled = false;
            FakeReportMenuEnabled = false;
            FakeValveTrackingEnabled = false;
            AntiCrashEnabled = true;
            AntiKickEnabled = true;
            ShowACReportsEnabled = true;
            AntiReportMode = 0;
            AntiAFKKickEnabled = true;
            AntiPauseDisconnectEnabled = true;
            VersionBypassEnabled = true;
            BlockModAccountSave = true;
            AntiBanEnabled = false;
            MenuOpenButton = ButtonMapper.MenuButton.B_Right;

            MenuDetectionEnabled = true;
            MenuDetectionAlertEnabled = true;
            AutoOverrideOnDetection = true;
            ThemeIndex = 0;
            UseCustomTheme = false;
            CustomPanelColor = new Color(0.06f, 0.07f, 0.12f, 0.94f);
            CustomAccentColor = new Color(0.2f, 0.75f, 0.95f, 1f);
            CustomTextColor = new Color(0.92f, 0.94f, 0.96f);
            PlayMenuDetectionAudio = true;
            PlayPatchOverrideAudio = true;
            AudioVolume = 1.0f;
            ProtectionVolume = 1.0f;
            WarningVolume = 1.0f;
            BanVolume = 1.0f;
            PatchSendReport = true;
            PatchDispatchReport = true;
            PatchCheckReports = true;
            PatchTelemetry = true;
            PatchPlayFabReport = true;
            PatchCloseInvalidRoom = true;
            PatchGracePeriod = true;
            PatchRPCLimits = true;
            PatchQuitDelay = true;
            PatchModCheckers = true;
            PatchBanDetection = true;
            PatchFailureText = true;
            PatchBadNameCheck = true;
            PatchAutoBanList = true;

            Save();
        }
    }
}
