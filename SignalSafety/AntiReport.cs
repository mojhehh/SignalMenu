using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using SignalMenu.Managers;

namespace SignalMenu.SignalSafety
{
    public static class AntiReport
    {
        public static int NearbyCount { get; private set; } = 0;

        public static int RangeIndex { get => SafetyConfig.AntiReportRangeIndex; set => SafetyConfig.AntiReportRangeIndex = value; }
        private static readonly string[] RangeNames = { "Default", "Large", "Massive" };
        private static readonly float[] RangeValues = { 0.6f, 1.0f, 2.0f };
        public static float Threshold => RangeValues[Mathf.Clamp(RangeIndex, 0, RangeValues.Length - 1)];
        public static string RangeName => RangeNames[Mathf.Clamp(RangeIndex, 0, RangeNames.Length - 1)];

        private static float antiReportDelay = 0f;
        private static float antiReportNotifyDelay = 0f;
        private static float _smartFilterLogTime = 0f;

        private static VRRig reportRig = null;

        public static bool AntiMute { get => SafetyConfig.AntiReportMuteDetect; set => SafetyConfig.AntiReportMuteDetect = value; }

        public static bool SmartMode { get => SafetyConfig.AntiReportSmartMode; set => SafetyConfig.AntiReportSmartMode = value; }
        private static readonly object _clickLock = new object();
        private static float _buttonClickTime = 0f;
        private static string _buttonClickPlayer = null;
        private const float SmartWindowSeconds = 2.5f;

        private static void SetClickData(float time, string player)
        {
            lock (_clickLock) { _buttonClickTime = time; _buttonClickPlayer = player; }
        }

        private static (float time, string player) GetClickData()
        {
            lock (_clickLock) { return (_buttonClickTime, _buttonClickPlayer); }
        }

        public static bool VisualizerEnabled { get => SafetyConfig.AntiReportVisualizerEnabled; set => SafetyConfig.AntiReportVisualizerEnabled = value; }
        private static Dictionary<string, GameObject> auraPool = new Dictionary<string, GameObject>();

        private static Action<EventData> oculusReportHandler = null;
        private static Action<EventData> smartReportHandler = null;
        private static bool oculusReportEnabled = false;
        private static bool smartReportEnabled = false;

        public static string LastReporter { get; private set; } = "";

        private struct HandSnapshot
        {
            public Vector3 rightPos;
            public Vector3 leftPos;
            public float time;
        }

        private static Dictionary<string, HandSnapshot> _prevHands = new Dictionary<string, HandSnapshot>();

        private const float PingRadiusFactor  = 0.6f;
        private const float MinPingScale      = 1.0f;
        private const float MaxPingScale      = 3.0f;
        private const float VelocityThreshold = 0.8f;
        private static float GetPingScaledThreshold(VRRig vrrig)
        {
            float baseThr = Threshold;
            try
            {
                var player = GetPhotonPlayerFromRig(vrrig);
                if (player == null) return baseThr;

                int pingMs = 0;
                try
                {
                    var props = player.CustomProperties;
                    if (props != null && props.ContainsKey("ping"))
                        pingMs = (int)props["ping"];
                    else
                        pingMs = PhotonNetwork.GetPing();
                }
                catch { pingMs = PhotonNetwork.GetPing(); }

                float oneWaySec = (pingMs * 0.5f) / 1000f;
                float scale = Mathf.Clamp(1f + oneWaySec * PingRadiusFactor, MinPingScale, MaxPingScale);
                return baseThr * scale;
            }
            catch { return baseThr; }
        }
        private static Vector3 PredictHandPosition(Vector3 currentPos, Vector3 velocity, VRRig vrrig)
        {
            try
            {
                int pingMs = PhotonNetwork.GetPing();
                var player = GetPhotonPlayerFromRig(vrrig);
                if (player != null)
                {
                    var props = player.CustomProperties;
                    if (props != null && props.ContainsKey("ping"))
                        pingMs = (int)props["ping"];
                }
                float extrapolateSec = (pingMs * 0.5f) / 1000f;
                return currentPos + velocity * extrapolateSec;
            }
            catch { return currentPos; }
        }
        private static bool PredictiveCheck(VRRig vrrig, Vector3 buttonPos)
        {
            if (vrrig == null || vrrig.rightHandTransform == null || vrrig.leftHandTransform == null)
                return false;

            string uid = null;
            try { uid = vrrig.Creator?.UserId; } catch { }
            if (string.IsNullOrEmpty(uid)) return false;

            Vector3 rNow = vrrig.rightHandTransform.position;
            Vector3 lNow = vrrig.leftHandTransform.position;
            float now = Time.time;

            HandSnapshot prev;
            if (!_prevHands.TryGetValue(uid, out prev))
            {
                _prevHands[uid] = new HandSnapshot { rightPos = rNow, leftPos = lNow, time = now };
                return false;
            }

            float dt = now - prev.time;
            if (dt < 0.02f) return false;

            Vector3 rVel = (rNow - prev.rightPos) / dt;
            Vector3 lVel = (lNow - prev.leftPos) / dt;

            _prevHands[uid] = new HandSnapshot { rightPos = rNow, leftPos = lNow, time = now };

            float thr = GetPingScaledThreshold(vrrig);

            float rDist = Vector3.Distance(rNow, buttonPos);
            float lDist = Vector3.Distance(lNow, buttonPos);
            if (rDist < thr || lDist < thr) return true;

            Vector3 rPred = PredictHandPosition(rNow, rVel, vrrig);
            Vector3 lPred = PredictHandPosition(lNow, lVel, vrrig);
            if (Vector3.Distance(rPred, buttonPos) < thr || Vector3.Distance(lPred, buttonPos) < thr)
                return true;

            Vector3 rDir = (buttonPos - rNow).normalized;
            Vector3 lDir = (buttonPos - lNow).normalized;
            float rApproach = Vector3.Dot(rVel, rDir);
            float lApproach = Vector3.Dot(lVel, lDir);

            if ((rApproach > VelocityThreshold && rDist < thr * 2.5f) ||
                (lApproach > VelocityThreshold && lDist < thr * 2.5f))
                return true;

            return false;
        }
        private static Player GetPhotonPlayerFromRig(VRRig rig)
        {
            try
            {
                string uid = rig?.Creator?.UserId;
                if (string.IsNullOrEmpty(uid) || PhotonNetwork.CurrentRoom == null) return null;
                foreach (var p in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (p.UserId == uid) return p;
                }
            }
            catch { }
            return null;
        }
        public static void CycleRange()
        {
            RangeIndex = (RangeIndex + 1) % RangeNames.Length;
        }
        private static bool OverlappingButton(VRRig vrrig, Vector3 position)
        {
            if (vrrig == null || vrrig.rightHandTransform == null || vrrig.leftHandTransform == null) return false;

            if (PredictiveCheck(vrrig, position)) return true;

            float thr = GetPingScaledThreshold(vrrig);
            float rightDist = Vector3.Distance(vrrig.rightHandTransform.position, position);
            float leftDist = Vector3.Distance(vrrig.leftHandTransform.position, position);
            return rightDist < thr || leftDist < thr;
        }
        private static bool SmartCheck(VRRig vrrig)
        {
            if (!SmartMode) return true;

            try
            {
                string rigUserId = vrrig?.Creator?.UserId;
                if (rigUserId == null) return true;

                var click = GetClickData();
                bool recentClick = rigUserId == click.player
                    && (Time.unscaledTime - click.time) < SmartWindowSeconds;

                return recentClick && PhotonNetwork.CurrentRoom != null;
            }
            catch { return true; }
        }
        public static void CheckAntiReport(Action<VRRig, Vector3> onReport)
        {
            if (!SafetyConfig.AntiReportEnabled) return;
            if (!NetworkSystem.Instance.InRoom) return;

            if (reportRig != null)
            {
                Plugin.Instance?.Log("[ANTI-REPORT] FRT triggered by rig");
                if (onReport != null)
                {
                    onReport.Invoke(reportRig, reportRig.transform.position);
                }
                reportRig = null;
                return;
            }

            int lineCount = 0;
            int myLineCount = 0;

            List<Vector3> buttonPositions = new List<Vector3>();

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                try
                {
                    lineCount++;

                    string localUserId = NetworkSystem.Instance.LocalPlayer?.UserId;
                    if (localUserId == null || line.linePlayer?.UserId != localUserId) continue;
                    myLineCount++;

                    if (line.reportButton?.gameObject?.transform != null)
                        buttonPositions.Add(line.reportButton.gameObject.transform.position);
                    if (line.cheatingButton?.transform != null)
                        buttonPositions.Add(line.cheatingButton.transform.position);
                    if (line.hateSpeechButton?.transform != null)
                        buttonPositions.Add(line.hateSpeechButton.transform.position);
                    if (line.toxicityButton?.transform != null)
                        buttonPositions.Add(line.toxicityButton.transform.position);
                    if (line.cancelButton?.transform != null)
                        buttonPositions.Add(line.cancelButton.transform.position);
                    if (AntiMute && line.muteButton?.gameObject?.transform != null)
                        buttonPositions.Add(line.muteButton.gameObject.transform.position);
                }
                catch { }
            }

            if (buttonPositions.Count == 0)
            {
                if (lineCount > 0 && myLineCount == 0 && Time.frameCount % 300 == 0)
                {
                    Plugin.Instance?.Log($"[ANTI-REPORT] WARNING: {lineCount} scoreboard lines but 0 belong to local player!");
                }
                return;
            }

            var remoteRigs = GorillaParent.instance.vrrigs
                .Where(vrrig => !vrrig.isLocal && !vrrig.isOfflineVRRig);

            foreach (var vrrig in remoteRigs)
            {
                try
                {
                    bool nearAny = false;
                    foreach (var btnPos in buttonPositions)
                    {
                        if (OverlappingButton(vrrig, btnPos))
                        {
                            nearAny = true;
                            break;
                        }
                    }

                    if (!nearAny) continue;

                    if (SmartMode && !SmartCheck(vrrig))
                    {
                        if (Time.unscaledTime > _smartFilterLogTime)
                        {
                            float elapsed = Time.unscaledTime - GetClickData().time;
                            Plugin.Instance?.Log($"[ANTI-REPORT] Rig near button but SmartMode filtered: lastClick={elapsed:F2}s ago");
                            _smartFilterLogTime = Time.unscaledTime + 5f;
                        }
                        continue;
                    }

                    if (onReport != null)
                    {
                        onReport.Invoke(vrrig, vrrig.transform.position);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Instance?.Log($"[ANTI-REPORT] Error checking rig: {ex.Message}");
                }
            }
        }
        public static void AntiReportDisconnect()
        {
            CheckAntiReport((vrrig, position) =>
            {
                if (Time.time <= antiReportDelay) return;
                antiReportDelay = Time.time + 1f;

                string reporterName = "Unknown";
                try
                {
                    var player = GetPlayerFromVRRig(vrrig);
                    if (player != null) reporterName = player.NickName;
                }
                catch { }

                LastReporter = reporterName;
                _prevHands.Clear();
                AudioManager.Play("reported", AudioManager.AudioCategory.Warning);
                Plugin.Instance?.Log($"[ANTI-REPORT] {reporterName} attempted report — disconnected");
                try { NotificationManager.SendNotification($"<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> {reporterName} attempted to report you, you have been disconnected."); } catch { }

                Patches.SafetyPatches.SafetyDisconnect($"Anti-Report: attempted report");
            });
        }
        public static void AntiReportReconnect()
        {
            CheckAntiReport((vrrig, position) =>
            {
                if (Time.time <= antiReportDelay) return;
                antiReportDelay = Time.time + 1f;

                string reporterName = "Unknown";
                try
                {
                    var player = GetPlayerFromVRRig(vrrig);
                    if (player != null) reporterName = player.NickName;
                }
                catch { }

                LastReporter = reporterName;
                Plugin.Instance?.Log(reporterName + " attempted report - reconnecting");
                AudioManager.Play("reported", AudioManager.AudioCategory.Warning);
                _prevHands.Clear();
                try { NotificationManager.SendNotification($"<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> {reporterName} attempted to report you, reconnecting..."); } catch { }

                Patches.SafetyPatches.SafetyDisconnect($"Anti-Report reconnect: attempted report");
                Patches.LobbyFixer.Fix();
            });
        }
        public static void RunAntiReport()
        {
            switch (SafetyConfig.AntiReportMode)
            {
                case 0:
                    AntiReportDisconnect();
                    break;
                case 1:
                    AntiReportReconnect();
                    break;
                case 2:
                    AntiReportNotify();
                    break;
                default:
                    AntiReportDisconnect();
                    break;
            }
        }
        public static void AntiReportNotify()
        {
            if (Time.time <= antiReportNotifyDelay) return;

            string notifyText = "";

            CheckAntiReport((vrrig, position) =>
            {
                antiReportNotifyDelay = Time.time + 0.1f;

                string name = "Unknown";
                try
                {
                    var player = GetPlayerFromVRRig(vrrig);
                    if (player != null) name = player.NickName;
                }
                catch { }

                if (notifyText == "")
                {
                    notifyText = name;
                }
                else
                {
                    notifyText = notifyText + ", " + name;
                }
            });

            if (!string.IsNullOrEmpty(notifyText))
            {
                bool isNew = LastReporter != notifyText;
                LastReporter = notifyText;
                NearbyCount = notifyText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (isNew)
                {
                    AudioManager.Play("report_nearby", AudioManager.AudioCategory.Warning);
                    try { NotificationManager.SendNotification($"<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> {notifyText} {(NearbyCount > 1 ? "are" : "is")} near your report button."); } catch { }
                }
            }
            else
            {
                NearbyCount = 0;
                LastReporter = "";
            }
        }
        public static void VisualizeAntiReport()
        {
            if (!VisualizerEnabled || !SafetyConfig.AntiReportEnabled)
            {
                if (auraPool.Count > 0)
                {
                    foreach (var kvp in auraPool)
                    {
                        if (kvp.Value != null) UnityEngine.Object.Destroy(kvp.Value);
                    }
                    auraPool.Clear();
                }
                return;
            }

            if (!NetworkSystem.Instance.InRoom) return;

            HashSet<string> activeKeys = new HashSet<string>();

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                try
                {
                    string localUserId = NetworkSystem.Instance.LocalPlayer?.UserId;
                    if (localUserId == null || line.linePlayer?.UserId != localUserId) continue;

                    float visRange = Threshold;

                    if (line.reportButton != null)
                    {
                        Vector3 pos = line.reportButton.gameObject.transform.position;
                        string key = "report_" + line.GetHashCode();
                        activeKeys.Add(key);
                        DrawAura(key, pos, visRange, Color.red);
                    }

                    if (AntiMute && line.muteButton != null)
                    {
                        Vector3 mutePos = line.muteButton.gameObject.transform.position;
                        string muteKey = "mute_" + line.GetHashCode();
                        activeKeys.Add(muteKey);
                        DrawAura(muteKey, mutePos, visRange, new Color(1f, 0.5f, 0f));
                    }
                }
                catch { }
            }

            var keys = new System.Collections.Generic.List<string>(auraPool.Keys);
            foreach (var k in keys)
            {
                GameObject auraObj;
                if (auraPool.TryGetValue(k, out auraObj) && !activeKeys.Contains(k) && auraObj != null)
                {
                    auraObj.SetActive(false);
                }
            }
        }
        private static Shader _cachedShader = null;

        private static void DrawAura(string key, Vector3 position, float range, Color color)
        {
            GameObject sphere;
            if (!auraPool.TryGetValue(key, out sphere) || sphere == null)
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.Destroy(sphere.GetComponent<Collider>());

                if (_cachedShader == null)
                {
                    _cachedShader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (_cachedShader == null) _cachedShader = Shader.Find("Unlit/Transparent");
                    if (_cachedShader == null) _cachedShader = Shader.Find("Sprites/Default");
                }
                Renderer renderer = sphere.GetComponent<Renderer>();
                if (_cachedShader != null) renderer.material.shader = _cachedShader;
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.renderQueue = 3000;

                auraPool[key] = sphere;
            }

            sphere.SetActive(true);
            sphere.transform.position = position;
            float diameter = range * 2f;
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);

            Renderer rend = sphere.GetComponent<Renderer>();
            Color c = color;
            c.a = 0.55f;
            rend.material.color = c;
        }
        public static bool IsNearButton(VRRig vrrig, Vector3 buttonPos)
        {
            return OverlappingButton(vrrig, buttonPos);
        }

        public static void SetReportRig(VRRig rig)
        {
            reportRig = rig;
        }
        private static bool _smartHandledThisFrame = false;
        private static void EventReceived_SmartAntiReport(EventData data)
        {
            try
            {
                if (data.Code == 200)
                {
                    var customData = data.CustomData as ExitGames.Client.Photon.Hashtable;
                    if (customData == null) return;

                    string rpcName = PhotonNetwork.PhotonServerSettings.RpcList[int.Parse(customData[5].ToString())];
                    object[] args = (object[])customData[4];

                    if (rpcName == "RPC_PlayHandTap" && (int)args[0] == 67)
                    {
                        if (PhotonNetwork.NetworkingClient?.CurrentRoom == null) return;
                        var clicker = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender, false);
                        if (clicker == null) return;
                        SetClickData(Time.unscaledTime, clicker.UserId);
                        _smartHandledThisFrame = true;
                    }
                }
            }
            catch { }
        }
        private static void EventReceived_AntiOculusReport(EventData data)
        {
            try
            {
                if (_smartHandledThisFrame) { _smartHandledThisFrame = false; return; }
                if (data.Code == 200)
                {
                    var customData = data.CustomData as ExitGames.Client.Photon.Hashtable;
                    if (customData == null) return;

                    string rpcName = PhotonNetwork.PhotonServerSettings.RpcList[int.Parse(customData[5].ToString())];
                    object[] args = (object[])customData[4];

                    if (rpcName == "RPC_PlayHandTap" && (int)args[0] == 67)
                    {
                        if (PhotonNetwork.NetworkingClient?.CurrentRoom == null) return;
                        var sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender, false);
                        if (sender == null) return;
                        VRRig senderRig = null;

                        foreach (var rig in GorillaParent.instance.vrrigs)
                        {
                            try
                            {
                                if (rig.Creator?.UserId == sender.UserId)
                                {
                                    senderRig = rig;
                                    break;
                                }
                            }
                            catch { }
                        }

                        if (senderRig != null)
                        {
                            float handDist = Vector3.Distance(
                                senderRig.leftHandTransform.position,
                                senderRig.rightHandTransform.position
                            );

                            if (handDist < 0.1f)
                            {
                                SetReportRig(senderRig);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public static void EnableSmartAntiReport()
        {
            if (smartReportEnabled) return;
            smartReportHandler = new Action<EventData>(EventReceived_SmartAntiReport);
            PhotonNetwork.NetworkingClient.EventReceived += smartReportHandler;
            smartReportEnabled = true;
        }
        public static void DisableSmartAntiReport()
        {
            if (!smartReportEnabled || smartReportHandler == null) return;
            PhotonNetwork.NetworkingClient.EventReceived -= smartReportHandler;
            smartReportEnabled = false;
            smartReportHandler = null;
        }
        public static void EnableAntiOculusReport()
        {
            if (oculusReportEnabled) return;
            oculusReportHandler = new Action<EventData>(EventReceived_AntiOculusReport);
            PhotonNetwork.NetworkingClient.EventReceived += oculusReportHandler;
            oculusReportEnabled = true;
        }
        public static void DisableAntiOculusReport()
        {
            if (!oculusReportEnabled || oculusReportHandler == null) return;
            PhotonNetwork.NetworkingClient.EventReceived -= oculusReportHandler;
            oculusReportEnabled = false;
            oculusReportHandler = null;
        }
        private static NetPlayer GetPlayerFromVRRig(VRRig rig)
        {
            if (rig == null) return null;

            try
            {
                if (rig.Creator != null) return rig.Creator;
            }
            catch { }

            return null;
        }
        public static void Initialize()
        {
            if (SafetyConfig.AntiReportEnabled)
            {
                EnableSmartAntiReport();
                EnableAntiOculusReport();
            }
        }
        public static void Shutdown()
        {
            DisableSmartAntiReport();
            DisableAntiOculusReport();

            foreach (var kvp in auraPool)
            {
                if (kvp.Value != null) UnityEngine.Object.Destroy(kvp.Value);
            }
            auraPool.Clear();
            _prevHands.Clear();
        }

        public static void OnRoomLeft()
        {
            _prevHands.Clear();

            var staleKeys = new List<string>(auraPool.Keys);
            foreach (var k in staleKeys)
            {
                if (auraPool[k] != null) UnityEngine.Object.Destroy(auraPool[k]);
            }
            auraPool.Clear();
        }

        public static void OnRoomJoined()
        {
            _prevHands.Clear();

            if (SafetyConfig.AntiReportEnabled)
            {
                if (!smartReportEnabled) EnableSmartAntiReport();
                if (!oculusReportEnabled) EnableAntiOculusReport();
            }
        }
    }
}