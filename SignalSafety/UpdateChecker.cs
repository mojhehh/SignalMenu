using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace SignalMenu.SignalSafety
{
    public static class UpdateChecker
    {
        public static string CurrentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
        private const string VersionURL = "https://mojhehh.github.io/Signal-Menu/version.txt";
        private const string FallbackURL = "https://raw.githubusercontent.com/mojhehh/Signal-Menu/main/version.txt";

        public static bool CheckComplete { get; private set; } = false;
        public static bool UpdateAvailable { get; private set; } = false;
        public static string LatestVersion { get; private set; } = "";
        public static bool NotificationShown { get; set; } = false;

        public static IEnumerator CheckForUpdate()
        {
            CheckComplete = false;
            UpdateAvailable = false;

            string[] urls = { VersionURL, FallbackURL };
            bool success = false;

            foreach (string url in urls)
            {
                if (success) break;

                var request = UnityWebRequest.Get(url);
                request.timeout = 8;
                yield return request.SendWebRequest();

                try
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string remote = request.downloadHandler.text.Trim();
                        if (string.IsNullOrEmpty(remote) || remote.Length > 20 || remote.Contains("<")) continue;

                        LatestVersion = remote;
                        success = true;

                        if (remote != CurrentVersion && IsNewer(remote, CurrentVersion))
                        {
                            UpdateAvailable = true;
                            Plugin.Instance?.Log($"[Update] New version available: {remote} (current: {CurrentVersion})");
                            AudioManager.Play("update_available", AudioManager.AudioCategory.Warning);
                        }
                        else
                        {
                            Plugin.Instance?.Log("[Update] Up to date");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Instance?.Log($"[Update] Error checking {url}: {ex.Message}");
                }
            }

            if (!success)
            {
                Plugin.Instance?.Log("[Update] All version check URLs failed");
            }

            CheckComplete = true;
        }

        private static bool IsNewer(string remote, string local)
        {
            try
            {
                var rv = new Version(remote);
                var lv = new Version(local);
                return rv > lv;
            }
            catch
            {
                return remote != local;
            }
        }
    }
}
