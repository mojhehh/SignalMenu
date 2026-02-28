/*
 * Signal Safety Menu  Mods/Safety.cs
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

using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using SignalMenu.Classes;
using SignalMenu.Extensions;
using SignalMenu.Managers;
using SignalMenu.Menu;
using SignalMenu.Patches.Menu;
using SignalMenu.Patches.Safety;
using SignalMenu.Utilities;
using Photon.Pun;
using Photon.Realtime;
using SignalMenu.SignalSafety;
using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static SignalMenu.Menu.Main;
using static SignalMenu.Utilities.RigUtilities;
using Random = UnityEngine.Random;

namespace SignalMenu.Mods
{
    public static class Safety
    {
        private static bool antiOculusReportHooked;
        public static void GeneralSafety()
        {
            if (!Buttons.GetIndex("Anti Report <color=grey>[</color><color=green>Disconnect</color><color=grey>]</color>").enabled) AntiReportDisconnect();
            if (!Buttons.GetIndex("Anti Report <color=grey>[</color><color=green>Anti Cheat</color><color=grey>]</color>").enabled) AntiCheatPatches.SendReportPatch.AntiACReport = true;
            if (!Buttons.GetIndex("Anti Moderator").enabled) AntiModerator();
            if (!Buttons.GetIndex("Anti Report <color=grey>[</color><color=green>Oculus</color><color=grey>]</color>").enabled && !antiOculusReportHooked) { antiOculusReportHooked = true; EnableAntiOculusReport(); }
        }

        public static void DisableGeneral()
        {
            if (!Buttons.GetIndex("Anti Report <color=grey>[</color><color=green>Anti Cheat</color><color=grey>]</color>").enabled) AntiCheatPatches.SendReportPatch.AntiACReport = false;
            if (!Buttons.GetIndex("Anti Report <color=grey>[</color><color=green>Oculus</color><color=grey>]</color>").enabled) DisableAntiOculusReport();
        }

        public static void NoFinger()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.rightControllerPrimaryButton = false;
            ControllerInputPoller.instance.rightControllerSecondaryButton = false;
            ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = false;
        }

        public static void SetGamemodeButtonActive(bool active = true) =>
            GetObject("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/ModeSelector_Group").SetActive(active);

        public static void FakeOculusMenu()
        {
            if (leftPrimary)
            {
                NoFinger();
                GTPlayer.Instance.inOverlay = true;
                GTPlayer.Instance.GetControllerTransform(true).localPosition = new Vector3(238f, -90f, 0f);
                GTPlayer.Instance.GetControllerTransform(false).localPosition = new Vector3(-190f, 90f, 0f);
                GTPlayer.Instance.GetControllerTransform(true).rotation = Camera.main.transform.rotation * Quaternion.Euler(-55f, 90f, 0f);
                GTPlayer.Instance.GetControllerTransform(false).rotation = Camera.main.transform.rotation * Quaternion.Euler(-55f, -49f, 0f);
            }

            Movement.SetHandEnabled(!leftPrimary);
        }

        public static void FakeReportMenu()
        {
            if (leftSecondary)
                NoFinger();

            GTPlayer.Instance.inOverlay = leftPrimary;
        }

        public static void FakeBrokenController()
        {
            Vector3 Position = leftPrimary ? GorillaTagger.Instance.leftHandTransform.position : GorillaTagger.Instance.rightHandTransform.position;
            Quaternion Rotation = leftPrimary ? GorillaTagger.Instance.leftHandTransform.rotation : GorillaTagger.Instance.rightHandTransform.rotation;

            GTPlayer.Instance.GetControllerTransform(true).position = GTPlayer.Instance.headCollider.transform.position + GTPlayer.Instance.headCollider.transform.up * (-0.5f * GTPlayer.Instance.scale);
            GTPlayer.Instance.GetControllerTransform(true).rotation = Camera.main.transform.rotation * Quaternion.Euler(-55f, 90f, 0f);

            GTPlayer.Instance.GetControllerTransform(false).position = Position;
            GTPlayer.Instance.GetControllerTransform(false).rotation = Rotation;

            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = false;
        }

        public static Vector3 deadPosition = Vector3.zero;
        public static Vector3 lvel = Vector3.zero;
        public static void FakePowerOff()
        {
            if (leftJoystickClick)
            {
                if (deadPosition == Vector3.zero)
                {
                    deadPosition = GorillaTagger.Instance.rigidbody.transform.position;
                    lvel = GorillaTagger.Instance.rigidbody.linearVelocity;
                }
                VRRig.LocalRig.enabled = false;
                GorillaTagger.Instance.rigidbody.transform.position = deadPosition;
                GorillaTagger.Instance.rigidbody.linearVelocity = lvel;
            }
            else
            {
                deadPosition = Vector3.zero;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void FakeValveTracking()
        {
            if (rightJoystickClick)
                VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.identity;
        }

        public static void SpoofSupportPage() =>
            GorillaComputer.instance.screenText.Set(GorillaComputer.instance.screenText.currentText.Replace("STEAM", "QUEST").Replace(GorillaComputer.instance.buildDate, "05/30/2024 16:50:12\nBUILD CODE 4893\nMANAGED ACCOUNT: NO"));

        private static string previousNickName;
        public static void AntiNameBan()
        {
            if (previousNickName != PhotonNetwork.LocalPlayer.NickName)
            {
                if (!BanPatches.CheckAutoBanListForName.CheckBanList(PhotonNetwork.LocalPlayer.NickName))
                {
                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>WARNING</color><color=grey>]</color> Your name, {PhotonNetwork.LocalPlayer.NickName}, is not allowed. It has been reset for your safety.");
                    ChangeName(RandomUtilities.RandomString(8));
                }
            }

            previousNickName = PhotonNetwork.LocalPlayer.NickName;
        }

        public static float flushCooldown;
        public static void FlushRPCs()
        {
            if (Time.time > flushCooldown)
            {
                RPCProtection();
                flushCooldown = Time.time + 5f;
                return;
            }
            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not meant to spam Flush RPCs. Only call it once after you are done spamming RPCs.");
        }
        public static void AntiLurker()
        {
            LurkerGhost lurker = Overpowered.Lurker;
            if (lurker.currentState == LurkerGhost.ghostState.possess && lurker.targetPlayer == NetworkSystem.Instance.LocalPlayer)
                lurker.ChangeState(LurkerGhost.ghostState.patrol);
        }

        private static float lastCacheClearedTime;
        public static void AutoClearCache()
        {
            if (Time.time > lastCacheClearedTime)
            {
                lastCacheClearedTime = Time.time + 60f;
                GC.Collect();
            }
        }

        public static int antiReportRangeIndex;
        public static float threshold = 0.35f;

        public static void ChangeAntiReportRange(bool positive = true)
        {
            if (positive)
                SignalSafety.AntiReport.RangeIndex++;
            else
                SignalSafety.AntiReport.RangeIndex--;

            if (SignalSafety.AntiReport.RangeIndex >= 3) SignalSafety.AntiReport.RangeIndex = 0;
            if (SignalSafety.AntiReport.RangeIndex < 0) SignalSafety.AntiReport.RangeIndex = 2;

            antiReportRangeIndex = SignalSafety.AntiReport.RangeIndex;
            threshold = SignalSafety.AntiReport.Threshold;

            Buttons.GetIndex("Change Anti Report Distance").overlapText = "Change Anti Report Distance <color=grey>[</color><color=green>" + SignalSafety.AntiReport.RangeName + "</color><color=grey>]</color>";
        }

        public static bool smartAntiReport;
        public static int buttonClickTime;
        public static string buttonClickPlayer;

        public static bool SmartAntiReport(NetPlayer linePlayer) =>
            SignalSafety.AntiReport.SmartMode && PhotonNetwork.CurrentRoom.IsVisible && !PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("MODDED");

        public static void EventReceived_SmartAntiReport(EventData data) { }

        public static void EnableSmartAntiReport()
        {
            SignalSafety.AntiReport.SmartMode = true;
            SignalSafety.AntiReport.EnableSmartAntiReport();
            smartAntiReport = true;
        }

        public static void DisableSmartAntiReport()
        {
            SignalSafety.AntiReport.SmartMode = false;
            SignalSafety.AntiReport.DisableSmartAntiReport();
            smartAntiReport = false;
        }

        public static void VisualizeAntiReport()
        {
            SignalSafety.AntiReport.VisualizeAntiReport();
        }

        private static bool OverlappingButton(VRRig vrrig, Vector3 position) =>
            SignalSafety.AntiReport.IsNearButton(vrrig, position);

        public static bool antiMute
        {
            get => SignalSafety.AntiReport.AntiMute;
            set => SignalSafety.AntiReport.AntiMute = value;
        }

		public static VRRig reportRig;
        public static void AntiReport(Action<VRRig, Vector3> onReport)
        {
            SignalSafety.AntiReport.CheckAntiReport((vrrig, position) => onReport?.Invoke(vrrig, position));
        }

        public static float antiReportDelay;
        public static void AntiReportDisconnect()
        {
            SafetyConfig.AntiReportMode = 0;
            SafetyConfig.AntiReportEnabled = true;
            SignalSafety.AntiReport.AntiReportDisconnect();
        }

        public static void AntiReportReconnect()
        {
            SafetyConfig.AntiReportMode = 1;
            SafetyConfig.AntiReportEnabled = true;
            SignalSafety.AntiReport.AntiReportReconnect();
        }

        public static void AntiReportJoinRandom()
        {
            SafetyConfig.AntiReportMode = 1;
            SafetyConfig.AntiReportEnabled = true;
            SignalSafety.AntiReport.AntiReportReconnect();
        }

        public static void EventReceived_AntiOculusReport(EventData data) { }

        public static void EnableAntiOculusReport() =>
            SignalSafety.AntiReport.EnableAntiOculusReport();

        public static void DisableAntiOculusReport() =>
            SignalSafety.AntiReport.DisableAntiOculusReport();

        public static float antiReportNotifyDelay;
        public static void AntiReportNotify()
        {
            SafetyConfig.AntiReportMode = 2;
            SafetyConfig.AntiReportEnabled = true;
            SignalSafety.AntiReport.AntiReportNotify();
        }

        public static void AntiReportOverlay()
        {
            SafetyConfig.AntiReportMode = 2;
            SafetyConfig.AntiReportEnabled = true;
            SignalSafety.AntiReport.AntiReportNotify();

            if (SignalSafety.AntiReport.NearbyCount == 0)
                NotificationManager.information.Remove("Anti-Report");
            else
                NotificationManager.information["Anti-Report"] = SignalSafety.AntiReport.LastReporter;
        }

        public static void AntiReportFRT(Player subject) =>
            SignalSafety.AntiReport.SetReportRig(subject.VRRig());

		public static void AntiModerator()
        {
            foreach (var vrrig in GorillaParent.instance.vrrigs.Where(vrrig => !vrrig.isOfflineVRRig && vrrig.rawCosmeticString.Contains("LBAAK") || vrrig.rawCosmeticString.Contains("LBAAD") || vrrig.rawCosmeticString.Contains("LMAPY")))
            {
                try
                {

                    VRRig plr = vrrig;
                    NetPlayer player = GetPlayerFromVRRig(plr);
                    if (player != null)
                    {
                        string text = "Room: " + PhotonNetwork.CurrentRoom.Name;
                        float r = 0f;
                        float g = 0f;
                        float b = 0f;
                        try
                        {
                                
                            r = plr.playerColor.r * 255;
                            g = plr.playerColor.r * 255;
                            b = plr.playerColor.r * 255;
                        }
                        catch { LogManager.Log("Failed to log colors, rig most likely nonexistent"); }

                        try
                        {
                            text += "\n====================================\n";
                            text += string.Concat("Player Name: \"", player.NickName, "\", Player ID: \"", player.UserId, "\", Player Color: (R: ", r.ToString(), ", G: ", g.ToString(), ", B: ", b.ToString(), ")");
                        }
                        catch { LogManager.Log("Failed to log player"); }

                        text += "\n====================================\n";
                        text += ObfStr.FileTag;
                        string fileName = $"{PluginInfo.BaseDirectory}/" + player.NickName + " - Anti Moderator.txt";

                        File.WriteAllText(fileName, text);
                    }
                }
                catch { }
                NetworkSystem.Instance.ReturnToSinglePlayer();
                NotificationManager.SendNotification($"<color=grey>[</color><color=purple>ANTI-MODERATOR</color><color=grey>]</color> {vrrig.GetName()} is a moderator, you have been disconnected. Their player ID and room code have been saved to a file.");
            }
        }

        public static void AntiContentCreator()
        {
            foreach (var vrrig in GorillaParent.instance.vrrigs.Where(vrrig => !vrrig.isOfflineVRRig && Visuals.specialCosmetics.Keys.Any(x => vrrig.rawCosmeticString.Contains(x))))
            {
                try
                {

                    VRRig plr = vrrig;
                    NetPlayer player = GetPlayerFromVRRig(plr);
                    if (player != null)
                    {
                        string text = "Room: " + PhotonNetwork.CurrentRoom.Name;
                        float r = 0f;
                        float g = 0f;
                        float b = 0f;
                        try
                        {

                            r = plr.playerColor.r * 255;
                            g = plr.playerColor.r * 255;
                            b = plr.playerColor.r * 255;
                        }
                        catch { LogManager.Log("Failed to log colors, rig most likely nonexistent"); }

                        try
                        {
                            text += "\n====================================\n";
                            text += string.Concat("Player Name: \"", player.NickName, "\", Player ID: \"", player.UserId, "\", Player Color: (R: ", r.ToString(), ", G: ", g.ToString(), ", B: ", b.ToString(), ")");
                        }
                        catch { LogManager.Log("Failed to log player"); }

                        text += "\n====================================\n";
                        text += ObfStr.FileTag;
                        string fileName = $"{PluginInfo.BaseDirectory}/" + player.NickName + " - Anti Content Creator.txt";

                        File.WriteAllText(fileName, text);
                    }
                }
                catch { }
                NetworkSystem.Instance.ReturnToSinglePlayer();
                NotificationManager.SendNotification($"<color=grey>[</color><color=purple>ANTI-CONTENT CREATOR</color><color=grey>]</color> {vrrig.GetName()} is a content creator, you have been disconnected. Their player ID and room code have been saved to a file.");
            }
        }

        private static bool previousSpecial;
        public static void CosmeticNotifications()
        {
            VRRig specialRig = null;
            string specialCosmetic = null;

            foreach (VRRig rig in GorillaParent.instance.vrrigs.Where(rig => !rig.IsLocal()))
            {
                foreach (var cosmetic in Visuals.specialCosmetics.Where(cosmetic => rig.rawCosmeticString.Contains(cosmetic.Key)))
                {
                    specialRig = rig;
                    specialCosmetic = cosmetic.Value;
                    break;
                }

                if (specialRig != null)
                    break;
            }

            if (specialRig != null && !previousSpecial)
                NotificationManager.SendNotification($"<color=grey>[</color><color=#{specialRig.GetColor().ToHex()}>COSMETIC</color><color=grey>]</color> {specialRig.GetName()} has {specialCosmetic}.");

            previousSpecial = specialRig != null;
        }

        private static float lastVol;
        private static float startSilenceTime = -1f;
        private static bool reloaded;

        public static void BypassAutomod()
        {
            GorillaTagger.moderationMutedTime = -1f;

            if (GorillaComputer.instance.autoMuteType != "OFF")
            {
                GorillaComputer.instance.autoMuteType = "OFF";
                PlayerPrefs.SetInt("autoMute", 0);
                PlayerPrefs.Save();
            }

            Recorder mic = GorillaTagger.Instance.myRecorder;
            if (mic == null)
                return;

            if (mic.SourceType == Recorder.InputSourceType.AudioClip)
                return;

            float volume = 0f;
            GorillaSpeakerLoudness recorder = VRRig.LocalRig.GetComponent<GorillaSpeakerLoudness>();
            if (recorder != null)
                volume = recorder.Loudness;

            if (volume == 0f)
            {
                if (lastVol != 0f)
                {
                    startSilenceTime = Time.time;
                    reloaded = false;
                }

                if (startSilenceTime > 0f && !reloaded && Time.time - startSilenceTime >= 0.25f)
                {
                    mic.RestartRecording(true);
                    reloaded = true;
                }
            }
            else
            {
                startSilenceTime = -1f;
                reloaded = false;
            }

            lastVol = volume;
        }

        public static void BypassModCheckers()
        {
            var player = PhotonNetwork.LocalPlayer;
            if (player == null) return;

            if (player.CustomProperties == null || player.CustomProperties.Count == 0) return;

            Hashtable toRemove = new Hashtable();

            foreach (var key in from keyObj in player.CustomProperties.Keys.ToList() select keyObj?.ToString() into key where key != null where !key.Equals("didTutorial") select key)
                toRemove[key] = null;

            if (toRemove.Count > 0)
                player.SetCustomProperties(toRemove);
        }

        private static Vector3 smoothedLeftHandPosition;
        private static Vector3 smoothedRightHandPosition;
        public static void AntiPredictions()
        {
            SerializePatch.OverrideSerialization = () =>
            {
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 leftHandPosition = VRRig.LocalRig.leftHand.rigTarget.localPosition;
                Vector3 rightHandPosition = VRRig.LocalRig.rightHand.rigTarget.localPosition;

                smoothedLeftHandPosition = Vector3.Lerp(smoothedLeftHandPosition, leftHandPosition, 0.75f);
                smoothedRightHandPosition = Vector3.Lerp(smoothedRightHandPosition, rightHandPosition, 0.75f);

                VRRig.LocalRig.leftHand.rigTarget.localPosition = smoothedLeftHandPosition;
                VRRig.LocalRig.rightHand.rigTarget.localPosition = smoothedRightHandPosition;

                SendSerialize(GorillaTagger.Instance.myVRRig.GetView);

                VRRig.LocalRig.leftHand.rigTarget.localPosition = leftHandPosition;
                VRRig.LocalRig.rightHand.rigTarget.localPosition = rightHandPosition;

                return false;
            };
        }

        public static void ChangeIdentity()
        {
            string randomName = "gorilla";
            for (var i = 0; i < 4; i++)
                randomName += Random.Range(0, 9).ToString();

            ChangeName(randomName);

            byte randA = (byte)Random.Range(0, 255);
            byte randB = (byte)Random.Range(0, 255);
            byte randC = (byte)Random.Range(0, 255);
            ChangeColor(new Color32(randA, randB, randC, 255));
        }

        public static void ChangeIdentityRegular()
        {
            string prefix = Random.Range(0, 3) == 0 ? namePrefix[Random.Range(0, namePrefix.Length)] : "";
            string suffix = Random.Range(0, 3) == 0 ? nameSuffix[Random.Range(0, nameSuffix.Length)] : "";
            string fName = prefix + names[Random.Range(0, names.Length)] + suffix;
            ChangeName(fName.Length > 12 ? fName[..12] : fName);

            Color[] colors = {
                Color.cyan,
                Color.yellow,
                Color.blue,
                Color.gray,
                Color.black,
                Color.white,
                Color.magenta,
                Color.yellow,
                Color.green,
                new Color(1f, 0.5f, 1f, 255f),
                new Color(0f, 0.5f, 0f, 255f),
                new Color32(113, 0, 198, 255),
                new Color32(170, 198, 170, 255),
                new Color32(170, 170, 170, 255),
                new Color32(227, 170, 85, 255),
                new Color32(0, 226, 255, 255)
            };
            ChangeColor(colors[Random.Range(0, colors.Length)]);
        }

        public static void ChangeIdentityCustom()
        {
            string[] names = { "goldentrophy", "me" };
            Color[] colors = { new Color32(255, 128, 0, 255), Color.white };

            string fileName = $"{PluginInfo.BaseDirectory}/CustomIdentities.txt";
            if (File.Exists(fileName))
            {
                string[] data = File.ReadAllText(fileName).Split("\n");
                names = data[0].Split(";");
                colors = data[1].Split(";").Select(HexToColor).ToArray();
            } else
                File.WriteAllText(fileName, "goldentrophy;me\nff8000;ffffff");

            string name = names[Random.Range(0, names.Length)];
            Color color = colors[Random.Range(0, colors.Length)];

            ChangeName(name.Length > 12 ? name[..12] : name);
            ChangeColor(color);
        }

        private static bool previouslyInLobby;
        public static void ChangeIdentityOnDisconnect(Action identityType)
        {
            if (!PhotonNetwork.InRoom && previouslyInLobby)
                identityType?.Invoke();
            
            previouslyInLobby = PhotonNetwork.InRoom;
        }

        private static readonly List<VRRig> nameSpoofRigs = new List<VRRig>();
        public static void NameSpoof()
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (VRRig rig in nameSpoofRigs)
            {
                if (!GorillaParent.instance.vrrigs.Contains(rig))
                    toRemove.Add(rig);
            }

            foreach (VRRig rig in toRemove)
                nameSpoofRigs.Remove(rig);

            toRemove.Clear();

            string archiveNickname = PhotonNetwork.NickName;
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig.isLocal) continue;
                if (!nameSpoofRigs.Contains(rig))
                {
                    string prefix = Random.Range(0, 3) == 0 ? namePrefix[Random.Range(0, namePrefix.Length)] : "";
                    string suffix = Random.Range(0, 3) == 0 ? nameSuffix[Random.Range(0, nameSuffix.Length)] : "";
                    string fName = prefix + names[Random.Range(0, names.Length)] + suffix;
                    ChangeName(fName.EnforceLength(12), true);

                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", GetPlayerFromVRRig(rig), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    nameSpoofRigs.Add(rig);
                }
            }

            if (PhotonNetwork.NickName != archiveNickname)
                PhotonNetwork.NickName = archiveNickname;
        }

        private static readonly List<VRRig> colorSpoofRigs = new List<VRRig>();
        public static void ColorSpoof()
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (VRRig rig in colorSpoofRigs)
            {
                if (!GorillaParent.instance.vrrigs.Contains(rig))
                    toRemove.Add(rig);
            }

            foreach (VRRig rig in toRemove)
                colorSpoofRigs.Remove(rig);

            toRemove.Clear();

            foreach (var rig in GorillaParent.instance.vrrigs.Where(rig => !rig.isLocal).Where(rig => !colorSpoofRigs.Contains(rig)))
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", GetPlayerFromVRRig(rig), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                colorSpoofRigs.Add(rig);
            }
        }

        public static int fpsSpoofValue = 90;
        public static void FPSSpoof()
        {
            FPSPatch.enabled = true;
            FPSPatch.spoofFPSValue = Random.Range(fpsSpoofValue - 10, fpsSpoofValue + 10);
        }

        public static int pingSpoofValue = 200;
        public static void PingSpoof()
        {
            SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(timeOffset: pingSpoofValue);
                return false;
            };
        }

        public static void ChangeFPSSpoofValue(bool positive = true)
        {
            if (positive)
                fpsSpoofValue += 5;
            else
                fpsSpoofValue -= 5;

            if (fpsSpoofValue > 140)
                fpsSpoofValue = 5;
            if (fpsSpoofValue < 5)
                fpsSpoofValue = 140;

            Buttons.GetIndex("Change FPS Spoof Value").overlapText = "Change FPS Spoof Value <color=grey>[</color><color=green>" + fpsSpoofValue + "</color><color=grey>]</color>";
        }

        public static void ChangePingSpoofValue(bool positive = true)
        {
            if (positive)
                pingSpoofValue += 100;
            else
                pingSpoofValue -= 100;

            if (pingSpoofValue > 10000)
                pingSpoofValue = 100;
            if (pingSpoofValue < 100)
                pingSpoofValue = 10000;

            Buttons.GetIndex("Change Ping Spoof Value").overlapText = "Change Ping Spoof Value <color=grey>[</color><color=green>" + pingSpoofValue + "</color><color=grey>]</color>";
        }

        public static readonly string[] namePrefix = {
            "EPIC", "EPIK", "REAL", "NOT", "SILLY", "LITTLE", "BIG", "MAYBE", "MONKE", "SUB2", "OG", "FUN", "FR", "NOT", "NOTA"
        };
        public static readonly string[] nameSuffix = {
            "GT", "VR", "LOL", "GTVR", "FAN", "XD", "LOL", "MONK", "YT", "NOT", "FR"
        };
        public static readonly string[] names = {
            "0", "SHIBA", "PBBV", "J3VU", "BEES", "NAMO", "MANGO", "FROSTY", "FRISH", "LEMMING", 
            "BILLY", "TIMMY", "MINIGAMES", "JMANCURLY", "VMT", "ELLIOT", "POLAR", "3CLIPCE", "DAISY09",
            "SHARKPUPPET", "DUCKY", "EDDIE", "EDDY", "RAKZZ", "CASEOH", "SKETCH", "SKY", "RETURN",
            "WATERMELON", "CRAZY", "MONK", "MONKE", "MONKI", "MONKEY", "MONKIY", "GORILL", "GOORILA", "GORILLA",
            "REDBERRY", "FOX", "RUFUS", "TTT", "TTTPIG", "PPPTIG", "K9", "BTC", "TICKLETIPJR", "BANANA",
            "PEANUTBUTTER", "GHOSTMONKE", "STATUE", "TURBOALLEN", "NOVA", "LUNAR", "MOON", "SUN", "RANDOM", "UNKNOWN",
            "GLITCH", "BUG", "ERROR", "CODE", "HACKER", "MODDER", "INVIS", "INVISIBLE", "TAGGER", "UNTAGGED",
            "BLUE", "RED", "GREEN", "PURPLE", "YELLOW", "BLACK", "WHITE", "BROWN", "CYAN", "GRAY",
            "GREY", "BANNED", "LEMON", "PLUSHIE", "CHEETO", "TIKTOK", "YOUTUBE", "TWITCH", "DISCORD", "MODDER", "HACKER"
        };

        public static string targetRank = "High";
        public static int rankIndex = 2;

        public static void ChangeRankedTier(bool positive = true)
        {
            if (positive)
                rankIndex++;
            else
                rankIndex--;

            rankIndex %= 3;
            if (rankIndex < 0)
                rankIndex = 2;

            targetRank = ((RankedProgressionManager.ERankedMatchmakingTier)rankIndex).ToString();
            Buttons.GetIndex("Change Ranked Tier").overlapText = "Change Matchmaking Tier <color=grey>[</color><color=green>" + targetRank + "</color><color=grey>]</color>";
        }

        public static void ChangeELOValue(bool positive = true)
        {
            if (positive)
                targetElo += 100;
            else
                targetElo -= 100;

            if (targetElo > 4000)
                targetElo = 0;
            if (targetElo < 0)
                targetElo = 4000;

            Buttons.GetIndex("Change ELO Value").overlapText = "Change ELO Value <color=grey>[</color><color=green>" + targetElo + "</color><color=grey>]</color>";
        }

        public static void ChangeBadgeTier(bool positive = true)
        {
            string[] badgeNames = {
                "Wood",
                "Rock",
                "Bronze",
                "Silver",
                "Gold",
                "Platinum",
                "Crystal",
                "Banana"
            };

            if (positive)
                targetBadge++;
            else
                targetBadge--;

            targetBadge %= 8;
            if (targetBadge < 0)
                targetBadge = 7;

            Buttons.GetIndex("Change Badge Tier").overlapText = "Change Badge Tier <color=grey>[</color><color=green>" + badgeNames[targetBadge] + "</color><color=grey>]</color>";
        }

        public static void SpoofRank(bool enabled, string tier = null)
        {
            RankedPatch.enabled = enabled;
            RankedPatch.targetTier = tier;
        }

        public static void SpoofPlatform(bool enabled, string target = null)
        {
            RankedPatch.enabled = enabled;
            RankedPatch.targetPlatform = target;
        }

        public static int targetElo = 4000;
        public static int targetBadge = 7;
        public static void SpoofBadge()
        {
            SetRankedPatch.enabled = true;
            if (!Mathf.Approximately(VRRig.LocalRig.currentRankedELO, targetElo) || VRRig.LocalRig.currentRankedSubTierQuest != targetBadge || VRRig.LocalRig.currentRankedSubTierPC != targetBadge)
                VRRig.LocalRig.SetRankedInfo(targetElo, targetBadge, targetBadge);
        }
    }
}