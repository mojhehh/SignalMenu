using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using SignalMenu.Managers;
using GorillaNetworking;

namespace SignalMenu.SignalSafety
{
    public static class AntiBan
    {
        public static bool IsActive { get; private set; } = false;
        public static bool IsRunning { get; private set; } = false;
        public static string Status { get; private set; } = "Idle";
        public static int PlayersKicked { get; private set; } = 0;
        public static int PlayersInRoom { get; private set; } = 0;

        private static Coroutine _antiBanCoroutine;
        private static float _lastStatusUpdate;
        private static byte _savedMaxPlayers = 10;


        public static void RunAntiBan()
        {
            if (IsRunning)
            {
                Log("[AntiBan] Already running, ignoring duplicate call.");
                return;
            }

            if (!PhotonNetwork.InRoom)
            {
                Log("[AntiBan] Not in a room.");
                Status = "Error: Not in room";
                return;
            }

            try { _savedMaxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers; }
            catch { _savedMaxPlayers = 10; }
            if (_savedMaxPlayers <= 0) _savedMaxPlayers = 10;

            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            {
                Log("[AntiBan] Room is empty, locking down immediately.");
                SetRoomPrivate(true);
                EnsureDefaultQueue();
                IsActive = true;
                Status = "Active (room was empty)";
                return;
            }

            _antiBanCoroutine = Plugin.Instance.StartCoroutine(AntiBanSequence());
        }

        public static void SetMasterClientToSelf()
        {
            if (!PhotonNetwork.InRoom)
            {
                Log("[AntiBan] Not in a room.");
                Status = "Error: Not in room";
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Log("[AntiBan] Already master client.");
                Status = "Already master";
                return;
            }

            try
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                Log("[AntiBan] Set master client to self.");
                Status = "Master client set";
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] SetMasterClient failed: {ex.Message}");
                Status = "Error: SetMaster failed";
            }
        }

        public static void SetRoomPrivate(bool makePrivate)
        {
            if (!PhotonNetwork.InRoom)
            {
                Log("[AntiBan] Not in a room.");
                return;
            }

            try
            {
                ExitGames.Client.Photon.Hashtable props;

                if (makePrivate)
                {
                    props = new ExitGames.Client.Photon.Hashtable
                    {
                        { 254, false }
                    };
                    Log("[AntiBan] Setting IsVisible=false (SessionIsPrivate=true for server)");
                }
                else
                {
                    byte maxP = _savedMaxPlayers;
                    if (maxP <= 0) maxP = 10;
                    props = new ExitGames.Client.Photon.Hashtable
                    {
                        { 253, true },
                        { 254, true },
                        { 255, maxP }
                    };
                    Log("[AntiBan] Restoring room to PUBLIC");
                }

                Dictionary<byte, object> opData = new Dictionary<byte, object>
                {
                    { 251, props },
                    { 250, true },
                    { 231, null }
                };

                PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(
                    252, opData, SendOptions.SendReliable
                );

                try { GorillaScoreboardTotalUpdater.instance?.UpdateActiveScoreboards(); } catch { }

                if (makePrivate)
                {
                    Log("[AntiBan] Room privacy applied — server will now discount auto-generated reports");
                }
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] SetRoomPrivate failed: {ex.Message}");
            }
        }

        public static void Disable()
        {
            if (_antiBanCoroutine != null)
            {
                try { Plugin.Instance.StopCoroutine(_antiBanCoroutine); } catch { }
                _antiBanCoroutine = null;
            }

            IsRunning = false;
            IsActive = false;
            PlayersKicked = 0;
            Status = "Disabled";

            if (PhotonNetwork.InRoom)
            {
                SetRoomPrivate(false);
                Log("[AntiBan] Disabled — room re-opened.");
            }
        }

        public static void Update()
        {
            if (!SafetyConfig.AntiBanEnabled) return;
            if (!PhotonNetwork.InRoom)
            {
                if (IsActive || IsRunning)
                {
                    IsActive = false;
                    IsRunning = false;
                    Status = "Idle (left room)";
                    PlayersKicked = 0;
                }
                return;
            }

            if (IsActive && Time.time - _lastStatusUpdate > 2f)
            {
                _lastStatusUpdate = Time.time;
                PlayersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;

                try
                {
                    if (PhotonNetwork.CurrentRoom.IsVisible)
                    {
                        Log("[AntiBan] ALERT: Room became visible! Re-applying privacy immediately...");
                        SetRoomPrivate(true);
                        Status = "Re-securing room...";
                        return;
                    }
                }
                catch { }

                try
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        Log("[AntiBan] ALERT: Lost master status! Attempting recovery...");
                        PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                        Status = "Recovering master...";
                        return;
                    }
                }
                catch { }

                try
                {
                    if (PhotonNetwork.PlayerListOthers.Length > 0)
                    {
                        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
                        bool iLowest = true;
                        foreach (var p in PhotonNetwork.PlayerListOthers)
                        {
                            if (p.ActorNumber < myActor)
                            {
                                iLowest = false;
                                break;
                            }
                        }

                        if (!iLowest)
                        {
                            Log("[AntiBan] WARNING: Another player has lower actor number! Finding new master...");
                            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                            Status = "Fixing master...";
                            return;
                        }
                    }
                }
                catch { }

                Status = $"Active | {PlayersInRoom} in room | Secured";
            }
        }


        private static IEnumerator AntiBanSequence()
        {
            IsRunning = true;
            IsActive = false;
            PlayersKicked = 0;
            Status = "Starting...";

            Log("[AntiBan] Starting anti-ban sequence...");
            try { NotificationManager.SendNotification("<color=grey>[</color><color=green>ANTI-BAN</color><color=grey>]</color> Anti-ban sequence started. Securing room..."); } catch { }
            
            Log("[AntiBan] Phase 0: Making room invisible FIRST (before kicking)...");
            Status = "Securing room...";
            SetRoomPrivate(true);
            yield return new WaitForSeconds(1f);

            Status = "Kicking players...";
            int totalOthers = PhotonNetwork.PlayerListOthers.Length;

            while (PhotonNetwork.PlayerListOthers.Length > 0)
            {
                if (!PhotonNetwork.InRoom)
                {
                    Log("[AntiBan] Disconnected during kick sequence.");
                    Status = "Failed: Disconnected";
                    IsRunning = false;
                    yield break;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    Log($"[AntiBan] We are master, flooding {PhotonNetwork.PlayerListOthers.Length} remaining player(s)...");
                    Status = $"Master — kicking remaining ({PhotonNetwork.PlayerListOthers.Length} left)";

                    Player[] others = PhotonNetwork.PlayerListOthers;
                    foreach (var player in others)
                    {
                        try { FloodKickPlayer(player); } catch { }
                    }

                    yield return new WaitForSeconds(5f);
                    continue;
                }

                Player currentMaster = PhotonNetwork.MasterClient;
                if (currentMaster == null) break;

                string masterName = currentMaster.NickName ?? "???";
                Log($"[AntiBan] Kicking master: {masterName}");
                Status = $"Kicking: {masterName}";

                float burstTime = Time.time;
                SendKickBurst();

                float kickTimeout = Time.time + 10f;
                while (PhotonNetwork.InRoom && PhotonNetwork.MasterClient == currentMaster)
                {
                    if (Time.time > kickTimeout)
                    {
                        Log($"[AntiBan] Kick timeout on {masterName}, retrying burst...");
                        Status = $"Retrying: {masterName}";
                        SendKickBurst();
                        kickTimeout = Time.time + 10f;
                    }
                    yield return null;
                }

                if (!PhotonNetwork.InRoom)
                {
                    Log("[AntiBan] Disconnected during kick sequence.");
                    Status = "Failed: Disconnected";
                    IsRunning = false;
                    yield break;
                }

                if (PhotonNetwork.MasterClient != currentMaster)
                {
                    PlayersKicked++;
                    float elapsed = Time.time - burstTime;
                    Log($"[AntiBan] Kicked {masterName} in {elapsed:F1}s. ({PlayersKicked}/{totalOthers})");
                    Status = $"Kicked {PlayersKicked}/{totalOthers}";

                    int waitTime = elapsed < 2.5f ? 10 : 5;
                    Log($"[AntiBan] Waiting {waitTime}s before next kick...");
                    yield return new WaitForSeconds(waitTime);
                }
            }

            if (!PhotonNetwork.InRoom)
            {
                Status = "Failed: Disconnected";
                IsRunning = false;
                yield break;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                Log("[AntiBan] Not master after kicking — attempting SetMasterClient...");
                Status = "Verifying master status...";
                try { PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer); } catch { }
                yield return new WaitForSeconds(1f);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Log($"[AntiBan] Confirmed as master client. Actor: {PhotonNetwork.LocalPlayer.ActorNumber}, " +
                    $"Others: {PhotonNetwork.PlayerListOthers.Length}");
            }
            else
            {
                Log("[AntiBan] WARNING: Could not become master! This could allow bans. Retrying...");
                Status = "Master retry...";
                try { PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer); } catch { }
                yield return new WaitForSeconds(2f);
            }

            Log("[AntiBan] Phase 3: Verifying room privacy...");
            Status = "Verifying privacy...";
            try
            {
                if (PhotonNetwork.CurrentRoom.IsVisible)
                {
                    Log("[AntiBan] WARNING: Room is still visible! Re-applying privacy...");
                    SetRoomPrivate(true);
                }
                else
                {
                    Log("[AntiBan] Room privacy confirmed (IsVisible=false → SessionIsPrivate=true)");
                }
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] Privacy check failed: {ex.Message}");
            }
            yield return new WaitForSeconds(1f);

            try
            {
                EnsureDefaultQueue();
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] Gamemode set warning: {ex.Message}");
            }

            if (PhotonNetwork.PlayerListOthers.Length > 0)
            {
                Log($"[AntiBan] WARNING: {PhotonNetwork.PlayerListOthers.Length} players joined during sequence! Mopping up...");
                Status = "Mopping up late joiners...";
                foreach (var player in PhotonNetwork.PlayerListOthers)
                {
                    try { FloodKickPlayer(player); } catch { }
                }
                yield return new WaitForSeconds(3f);
            }

            IsRunning = false;
            IsActive = true;
            PlayersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
            Status = "Active — room anti-banned";
            _antiBanCoroutine = null;

            Log($"[AntiBan] Anti-ban ACTIVE! Kicked {PlayersKicked} players. Room is private & secure.");
            Log("[AntiBan] Server webhook will discount all auto-generated reports from this room.");
            Log("[AntiBan] Friends can rejoin with the room code.");
            try { NotificationManager.SendNotification($"<color=grey>[</color><color=green>ANTI-BAN</color><color=grey>]</color> Room secured! Kicked {PlayersKicked} players. Room is private."); } catch { }
        }


        private static void SendKickBurst()
        {
            try
            {
                int fakeViewId = 0;
                int timestamp = PhotonNetwork.ServerTimestamp;
                for (int i = 0; i < 3965; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new ExitGames.Client.Photon.Hashtable
                    {
                        { 0, "GameMode" },
                        { 6, timestamp },
                        { 7, fakeViewId }
                    }, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    }, SendOptions.SendReliable);
                }
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] SendKickBurst error: {ex.Message}");
            }
        }

        private static void FloodKickPlayer(Player target)
        {
            if (target == null || target.IsLocal) return;

            try
            {
                int fakeViewId = 0;
                int timestamp = PhotonNetwork.ServerTimestamp;
                for (int i = 0; i < 3965; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new ExitGames.Client.Photon.Hashtable
                    {
                        { 0, "GameMode" },
                        { 6, timestamp },
                        { 7, fakeViewId }
                    }, new RaiseEventOptions
                    {
                        TargetActors = new int[] { target.ActorNumber }
                    }, SendOptions.SendReliable);
                }
            }
            catch { }
        }

        private static void EnsureDefaultQueue()
        {
            if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null) return;

            try
            {
                string currentGameMode = "";
                try
                {
                    var props = PhotonNetwork.CurrentRoom.CustomProperties;
                    if (props != null && props.ContainsKey("gameMode"))
                        currentGameMode = props["gameMode"]?.ToString() ?? "";
                }
                catch { }

                if (string.IsNullOrEmpty(currentGameMode))
                {
                    Log("[AntiBan] No gameMode property found, skipping.");
                    return;
                }

                string[] parts = currentGameMode.Split('|');

                string zone;
                string gameType;

                if (parts.Length >= 3)
                {
                    zone = parts[0];
                    gameType = parts[2];
                }
                else if (parts.Length == 2)
                {
                    zone = parts[0];
                    gameType = parts[1];
                }
                else
                {
                    try { zone = PhotonNetworkController.Instance.currentJoinTrigger.networkZone; }
                    catch { zone = "forest"; }
                    gameType = currentGameMode;
                }

                string newGameMode = zone + "|DEFAULT|" + gameType;

                if (newGameMode == currentGameMode)
                {
                    Log($"[AntiBan] Gamemode already correct: {currentGameMode}");
                    return;
                }

                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable
                {
                    { "gameMode", newGameMode }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

                Log($"[AntiBan] Gamemode set: {currentGameMode} → {newGameMode}");
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] EnsureDefaultQueue error: {ex.Message}");
            }
        }

        private static void Log(string message)
        {
            try { Plugin.Instance?.Log(message); } catch { }
        }

        public static void MakeNewPublicRoom()
        {
            if (IsRunning)
            {
                Log("[AntiBan] Can't create room while anti-ban is running.");
                Status = "Error: AntiBan running";
                return;
            }

            try
            {
                Plugin.Instance?.StartCoroutine(CreatePublicRoomCoroutine());
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] MakeNewPublicRoom error: {ex.Message}");
            }
        }

        private static IEnumerator CreatePublicRoomCoroutine()
        {
            if (PhotonNetwork.InRoom || NetworkSystem.Instance.InRoom)
            {
                Log("[AntiBan] Leaving current room...");
                try { NetworkSystem.Instance.ReturnToSinglePlayer(); } catch { }
                
                float timeout = Time.time + 8f;
                while ((PhotonNetwork.InRoom || NetworkSystem.Instance.InRoom) && Time.time < timeout)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(1.5f);
            }

            GorillaNetworkJoinTrigger trigger = null;
            try
            {
                trigger = PhotonNetworkController.Instance.currentJoinTrigger;
                if (trigger == null)
                    trigger = GorillaComputer.instance.GetJoinTriggerForZone("forest");
            }
            catch
            {
                try { trigger = GorillaComputer.instance.GetJoinTriggerForZone("forest"); } catch { }
            }

            if (trigger == null)
            {
                Log("[AntiBan] ERROR: No join trigger found, can't create room.");
                Status = "Error: No join trigger";
                yield break;
            }

            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] code = new char[4];
            for (int i = 0; i < 4; i++)
                code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            string roomName = new string(code);

            string queue = "DEFAULT";
            try { queue = GorillaComputer.instance.currentQueue ?? "DEFAULT"; } catch { }

            string platform = "STEAM";
            try { platform = PhotonNetworkController.Instance.platformTag ?? "STEAM"; } catch { }

            string gameMode = "";
            try { gameMode = trigger.GetFullDesiredGameModeString(); } catch { gameMode = "forest|DEFAULT|INFECTION"; }

            byte maxPlayers = 10;

            RoomConfig roomConfig = new RoomConfig
            {
                createIfMissing = true,
                isJoinable = true,
                isPublic = true,
                MaxPlayers = maxPlayers,
                CustomProps = new ExitGames.Client.Photon.Hashtable
                {
                    { "gameMode", gameMode },
                    { "platform", platform },
                    { "queueName", queue }
                }
            };

            Log($"[AntiBan] Creating new public room: {roomName} | {gameMode} | {queue}");

            try
            {
                PhotonNetworkController.Instance.currentJoinType = GorillaNetworking.JoinType.Solo;
                NetworkSystem.Instance.ConnectToRoom(roomName, roomConfig);
            }
            catch (Exception ex)
            {
                Log($"[AntiBan] ConnectToRoom failed: {ex.Message}");
                try
                {
                    PhotonNetworkController.Instance.AttemptToJoinPublicRoom(trigger);
                    Log("[AntiBan] Fallback: AttemptToJoinPublicRoom called.");
                }
                catch (Exception ex2)
                {
                    Log($"[AntiBan] Fallback also failed: {ex2.Message}");
                    Status = "Error: Can't create room";
                }
            }
        }
    }
}
