using Lib_K_Relay.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib_K_Relay;
using System.Diagnostics;
using FameBot.Data.Enums;
using FameBot.Data.Models;
using Lib_K_Relay.Networking;
using FameBot.Helpers;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Utilities;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.GameData;
using FameBot.Services;
using FameBot.UserInterface;
using FameBot.Data.Events;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Globalization;

//todo:
//
//auto server swap
//

namespace FameBot.Core
{
    public static class Global
    {
        public static Location QuestLocation = null;
        public static Location SentryLocation = null;
        public static int ValidQuestObjType = -1;
        public static int SentryHealth = -1;
        public static bool FarmingLostSentry = false;
        //public static bool FarmingCrystal = false;
        public static bool TeleportedInland = false;
    }

    public class Plugin : IPlugin
    {
        #region IPlugin
        public string GetAuthor()
        {
            return "001";
        }

        public string[] GetCommands()
        {
            return new string[]
            {
                "/bind - binds the bot to the client where the command is used.",
                "/start - starts the bot",
                "/gui - opens the gui",
                "/001"
            };
        }

        public string GetDescription()
        {
            return "A bot designed to automate the process of collecting realm whites. \n" +
                "Thanks to Gravitir3.14, Urantij, willfuttsbucks, Killed Be Killed, Jazz and 059 for their contributions to ROTMG. \n" +
                "\n" +
                "RealmQuests is a modification of Famebot (created by Killed Be Killed)";

        }
        //add mpgh and other hack forum notifications
        public string GetName()
        {
            return "RealmQuests";
        }
        #endregion

        #region Client properties.
        private IntPtr flashPtr;
        private bool followTarget;
        private List<Target> targets;
        private List<Portal> portals;
        private Dictionary<int, Target> playerPositions;
        private List<Enemy> enemies;
        private List<Obstacle> obstacles;
        private List<ushort> obstacleIds;
        private Client connectedClient;
        private Location lastLocation = null;
        private Location lastAverage;
        private bool blockNextAck = false;
        private string preferredRealmName = null;
        #endregion

        #region Config/other properties.
        private int tickCount;
        private Configuration config;
        private FameBotGUI gui;
        private bool gotoRealm;
        private bool enabled;
        private bool isInNexus;
        private string currentMapName;
        #endregion

        #region Events
        // The event which updates the health gui.
        public static event HealthEventHandler healthChanged;
        public delegate void HealthEventHandler(object sender, HealthChangedEventArgs args);

        // The event which updates the keypress gui.
        public static event KeyEventHandler keyChanged;
        public delegate void KeyEventHandler(object sender, KeyEventArgs args);

        // The event which relays gui events (like button presses) to the bot.
        private static event GuiEventHandler guiEvent;
        private delegate void GuiEventHandler(GuiEvent evt);

        // The event which sends messages to the event log.
        public static event LogEventHandler logEvent;
        public delegate void LogEventHandler(object sender, LogEventArgs args);

        // The event which triggers an in game chat message to be sent.
        private static event SendMessageEventHandler sendMessage;
        private delegate void SendMessageEventHandler(string message);

        // The event which relays in game messages to the chat gui.
        public static event ReceiveMessageEventHandler receiveMesssage;
        public delegate void ReceiveMessageEventHandler(object sender, MessageEventArgs args);

        // The event which updates the fame bar gui.
        public static event FameUpdateEventHandler fameUpdate;
        public delegate void FameUpdateEventHandler(object sender, FameUpdateEventArgs args);
        #endregion

        #region Keys
        private bool wPressed;
        private bool aPressed;
        private bool sPressed;
        private bool dPressed;

        private bool W_PRESSED
        {
            get { return wPressed; }
            set
            {
                if (wPressed == value)
                    return;
                wPressed = value;
                WinApi.SendMessage(flashPtr, value ? (uint)Key.KeyDown : (uint)Key.KeyUp, new IntPtr((int)Key.W), IntPtr.Zero);
                keyChanged?.Invoke(this, new KeyEventArgs(Key.W, value));
            }
        }
        private bool A_PRESSED
        {
            get { return aPressed; }
            set
            {
                if (aPressed == value)
                    return;
                aPressed = value;
                WinApi.SendMessage(flashPtr, value ? (uint)Key.KeyDown : (uint)Key.KeyUp, new IntPtr((int)Key.A), IntPtr.Zero);
                keyChanged?.Invoke(this, new KeyEventArgs(Key.A, value));
            }
        }
        private bool S_PRESSED
        {
            get { return sPressed; }
            set
            {
                if (sPressed == value)
                    return;
                sPressed = value;
                WinApi.SendMessage(flashPtr, value ? (uint)Key.KeyDown : (uint)Key.KeyUp, new IntPtr((int)Key.S), IntPtr.Zero);
                keyChanged?.Invoke(this, new KeyEventArgs(Key.S, value));
            }
        }
        private bool D_PRESSED
        {
            get { return dPressed; }
            set
            {
                if (dPressed == value)
                    return;
                dPressed = value;
                WinApi.SendMessage(flashPtr, value ? (uint)Key.KeyDown : (uint)Key.KeyUp, new IntPtr((int)Key.D), IntPtr.Zero);
                keyChanged?.Invoke(this, new KeyEventArgs(Key.D, value));
            }
        }
        #endregion


        private Dictionary<int, Location> ObjectLocations = new Dictionary<int, Location>();
        private Dictionary<int, Location> SeenQuestsLoc = new Dictionary<int, Location>();
        private Dictionary<int, int> SeenObjs = new Dictionary<int, int>();
        private Dictionary<int, int> SeenQuests = new Dictionary<int, int>();
        private Dictionary<int, string> PlayerNames = new Dictionary<int, string>();
        private Location target_;
        private Proxy _proxy;
        private Stopwatch GoodQuest = new Stopwatch();
        private Stopwatch BadQuest = new Stopwatch();
        private Stopwatch TimeInland = new Stopwatch();
        private Stopwatch TimeSinceMaptime = new Stopwatch();
        private Stopwatch TimeSincePentaDied = new Stopwatch();
        private Stopwatch TimeSinceRealmClosed = new Stopwatch();
        private bool CanMoveToBlueBags = true;
        private bool Debug = true;
        private bool Disabled = false;
        private bool EscapeIssued = false;
        private bool RareBagPriority = false;
        private bool ReachedRareBag = false;
        private bool SentryDied15s = false;
        private bool loopingforrealm = false;
        private float Epsilon = 0;
        private int Attempts = 0;
        private int BlockObjId = -1;
        private int CloseToQuestType = -1;
        private int MinPoints = 0;
        private int QuestId = -1;
        private int SentryObjID = -1;
        private int ValidQuestId = -1;
        private int loopingforrealmcount = 0;
        private int lootdrop = 0;
        private int rTeleportDelay = rnd.Next(100, 15000);
        private int stuckcheck = 0;
        private readonly string spampattern = @"OryxJackpot|LIFEPOT. ORG|RPGStash|Realmgold.com|ROTMG,ORG|RealmGold|RealmShop|Wh!te|RealmBags|-------------------|Rea[lI!]mK[i!]ngs|ORYXSH[O0]P|Rea[lI!]mStock|Rea[lI!]mPower";
        private readonly ushort Blue = Convert.ToUInt16(BagsX.Blue);
        private readonly ushort BlueBoosted = Convert.ToUInt16(BagsX.BlueBoosted);
        private readonly ushort Egg = Convert.ToUInt16(BagsX.Egg);
        private readonly ushort EggBoosted = Convert.ToUInt16(BagsX.EggBoosted);
        private readonly ushort Gold = Convert.ToUInt16(BagsX.Gold);
        private readonly ushort GoldBoosted = Convert.ToUInt16(BagsX.GoldBoosted);
        private readonly ushort Orange = Convert.ToUInt16(BagsX.Orange);
        private readonly ushort OrangeBoosted = Convert.ToUInt16(BagsX.OrangeBoosted);
        private readonly ushort Red = Convert.ToUInt16(BagsX.Red);
        private readonly ushort RedBoosted = Convert.ToUInt16(BagsX.RedBoosted);
        private readonly ushort White = Convert.ToUInt16(BagsX.White);
        private readonly ushort WhiteBoosted = Convert.ToUInt16(BagsX.WhiteBoosted);
        private static Random rnd = new Random();
        private string BuildVersion;
        private Dictionary<int, int> CurrPlayers = new Dictionary<int, int>();
        private string ServerName;
        private string _bestName;
        private string bestName;
        private string currRealmIP = "";
        private string currRealmName = "";
        private string currServerIP = "";

        public void Initialize(Proxy proxy)
        {
            #region DumbShit

            // Initialize lists so they are empty instead of null.
            targets = new List<Target>();
            playerPositions = new Dictionary<int, Target>();
            portals = new List<Portal>();
            enemies = new List<Enemy>();
            obstacles = new List<Obstacle>();

            // get obstacles
            obstacleIds = new List<ushort>();
            GameData.Objects.Map.ForEach((kvp) =>
            {
                if (kvp.Value.FullOccupy || kvp.Value.OccupySquare)
                {
                    obstacleIds.Add(kvp.Key);
                }
            });
            PluginUtils.Log("RealmQuests", "Found {0} obstacles.", obstacleIds.Count);

            // Initialize and display gui.
            gui = new FameBotGUI();
            PluginUtils.ShowGUI(gui);

            // Get the config.
            config = ConfigManager.GetConfiguration();

            // Look for all processes with the configured flash player name.
            Process[] processes = Process.GetProcessesByName(config.FlashPlayerName);
            if (processes.Length == 1)
            {
                // If there is one client open, bind to it.
                Log("Automatically bound to client.");
                flashPtr = processes[0].MainWindowHandle;
                gui?.SetHandle(flashPtr);
                /*PressPlay();
                if (config.AutoConnect)
                    Start();*/
            }
            // If there are multiple or no clients running, log a message.
            else if (processes.Length > 1)
            {
                Log("Multiple flash players running. Use the /bind command on the client you want to use.");
            }
            else
            {
                Log("Couldn't find flash player. Use the /bind command in game, then start the bot.");
            }

            #region Proxy Hooks
            proxy.HookCommand("bind", ReceiveCommand);
            proxy.HookCommand("start", ReceiveCommand);
            proxy.HookCommand("gui", ReceiveCommand);
            proxy.HookCommand("famebot", ReceiveCommand);
            proxy.HookCommand("fb", ReceiveCommand);
            proxy.HookCommand("001", ReceiveCommand);
            proxy.HookCommand("getdist", ReceiveCommand);

            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.NEWTICK, OnNewTick);
            proxy.HookPacket(PacketType.PLAYERHIT, OnHit);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);
            proxy.HookPacket(PacketType.TEXT, OnText);
            proxy.HookPacket(PacketType.GOTOACK, (client, packet) =>
            {
                if (blockNextAck)
                {
                    packet.Send = false;
                    blockNextAck = false;
                }
            });
            proxy.HookPacket(PacketType.PLAYERTEXT, (client, packet) =>
            {
                PlayerTextPacket p = (PlayerTextPacket)packet;
                if (p.Text.Contains("001"))
                {
                    packet.Send = false;
                    XLog("You can't say that!");
                }
            });
            proxy.HookPacket(PacketType.RECONNECT, (client, packet) =>
            {
                ReconnectPacket p = (ReconnectPacket)packet;

                XLog("Received reconnect: Host: " + p.Host + " Name: " + p.Name + " GameId: " + p.GameId);
                if (p.Name.StartsWith("NexusPortal"))
                {
                    currRealmName = p.Name.Split('.')[1];
                }
                else
                {
                }
                //Match match_realm = Regex.Match(p.Name, @"(?<=NexusPortal.).*", RegexOptions.IgnoreCase);

                /*
                Console.WriteLine("```");
                Console.WriteLine("ServerName: " + GameData.Servers.Map.Single(x => x.Value.Address == Proxy.DefaultServer).Value.Name);
                Console.WriteLine("ServerIP: " + Proxy.DefaultServer);
                Console.WriteLine("RealmName: " + currRealmName);
                Console.WriteLine("RealmIP: " + p.Host);
                Console.WriteLine("```");
                */
                currRealmIP = p.Host;

                if (loopingforrealmcount >= 50)
                {
                    loopingforrealm = false;
                }

                if (currRealmName == _bestName)
                {
                    loopingforrealm = false;
                    loopingforrealmcount = 0;
                }
                else if (loopingforrealm)
                {
                    loopingforrealmcount++;
                    XLog("Searching for " + _bestName + ". Found " + currRealmName + " instead.");

                    ReconnectPacket reconnect = (ReconnectPacket)Packet.Create(PacketType.RECONNECT);
                    reconnect.Name = "{\"text\":\"server.realm_of_the_mad_god\"}";
                    reconnect.Host = "";
                    reconnect.Stats = "";
                    reconnect.Port = -1;
                    reconnect.GameId = -3;
                    reconnect.KeyTime = -1;
                    reconnect.IsFromArena = false;
                    reconnect.Key = new byte[0];

                    connectedClient.SendToClient(reconnect);
                }
            });
            proxy.HookPacket(PacketType.QUESTOBJID, (client, packet) =>
            {
                QuestObjIdPacket p = packet as QuestObjIdPacket;
                QuestId = p.ObjectId;
                if (Debug) XLog("Quest: " + p.ObjectId);


            });
            /*
            proxy.HookPacket(PacketType.PLAYERSHOOT, (client, packet) =>
            {
                PlayerShootPacket p = packet as PlayerShootPacket;

                if (_RealmQuests.Default.OnlyShootNearQuest
                && client.PlayerData.Pos.DistanceTo(Global.QuestLocation) > 15
                && p.ContainerType == client.PlayerData.Slot[0])
                {
                    packet.Send = false;
                    XLog("Blocking shoot");

                }
            });

            proxy.HookPacket(PacketType.USEITEM, (client, packet) =>
            {
                UseItemPacket p = packet as UseItemPacket;


                if (_RealmQuests.Default.OnlyShootNearQuest
                && client.PlayerData.Pos.DistanceTo(Global.QuestLocation) > 15
                && p.SlotObject.ObjectType == client.PlayerData.Slot[1])
                {
                    packet.Send = false;
                    XLog("Blocking ability");
                }
            });
            */
            proxy.HookPacket(PacketType.FAILURE, (client, packet) =>
            {
                FailurePacket p = packet as FailurePacket;

                //if (p.ErrorId != 9) XLog("<FAILURE> ID: " + p.ErrorId + " Message: " + p.ErrorMessage);

                if (p.ErrorMessage == "{\"key\":\"server.realm_full\"}")
                {
                    Attempts++;
                    if (Attempts >= 6 && bestName != "")
                    {
                        _bestName = bestName;
                        ReconnectPacket reconnect = (ReconnectPacket)Packet.Create(PacketType.RECONNECT);
                        reconnect.Name = "{\"text\":\"server.realm_of_the_mad_god\"}";
                        reconnect.Host = "";
                        reconnect.Stats = "";
                        reconnect.Port = -1;
                        reconnect.GameId = -3;
                        reconnect.KeyTime = -1;
                        reconnect.IsFromArena = false;
                        reconnect.Key = new byte[0];

                        connectedClient.SendToClient(reconnect);
                        Attempts = 0;
                        loopingforrealm = true;
                    }
                }
            });

            proxy.HookPacket(PacketType.CREATESUCCESS, (client, packet) =>
            {
                target_ = null;
                ServerName = GameData.Servers.Map.Single(x => x.Value.Address == Proxy.DefaultServer).Value.Name;
                //if (ServerName == "USWest2") Disabled = true;
                PluginUtils.Delay(200, () => Stop());
                PluginUtils.Delay(500, () => Stop());
                PluginUtils.Delay(1300, () => Stop());
                PluginUtils.Delay(1400, () => ResetAllKeys());
                PluginUtils.Delay(2200, () => Start());
            });
            /*
            proxy.HookPacket(PacketType.ESCAPE, (client, packet) =>
            {
                EscapePacket p = packet as EscapePacket;
                if (currentMapName == "Nexus")
                {
                    p.Send = false;
                    packet.Send = false;
                }
            });
            */
            proxy.HookPacket(PacketType.PONG, (client, packet) =>
            {
                PongPacket p = packet as PongPacket;
                if (Disabled) XLog("Outdated.");
                TimeToNexusCheck(client);
                if (currentMapName == "Nexus" && (gotoRealm == true || enabled)) stuckcheck++;
                else stuckcheck = 0;
                if (stuckcheck > 25)
                {
                    XLog("Stuck! Nexusing...");

                    connectedClient.State.LastRealm = null;
                    ReconnectPacket rpacket = Packet.Create<ReconnectPacket>(PacketType.RECONNECT);
                    rpacket.GameId = -2;
                    rpacket.Host = Proxy.DefaultServer;
                    rpacket.IsFromArena = false;
                    rpacket.Key = new byte[0];
                    rpacket.KeyTime = connectedClient.Time;
                    rpacket.Name = "Nexus";
                    rpacket.Port = 2050;
                    rpacket.Stats = "";
                    ReconnectHandler.SendReconnect(connectedClient, rpacket);

                    stuckcheck = 0;
                }
            });

            /*
            if (currentMapName == "Nexus")
            { 
                if (client.PlayerData.Health == client.PlayerData.MaxHealth)
                {
                    if (client.ConnectTo(client.State.LastRealm))
                    {
                        gotoRealm = false;
                    }
                }
            }*/
            currServerIP = Proxy.DefaultServer;
            _proxy = proxy;

            Task.Run(() => { Loop(); });

            #endregion

            // Runs every time a client connects.
            proxy.ClientConnected += (client) =>
            {
                // Clear all lists and reset keys.
                MinPoints = 0;
                CanMoveToBlueBags = true;
                EscapeIssued = false;
                Epsilon = 0;
                CurrPlayers.Clear();
                Global.TeleportedInland = false;
                SeenQuests.Clear();
                SeenQuestsLoc.Clear();
                //Global.FarmingCrystal = false;
                Global.FarmingLostSentry = false;
                Global.QuestLocation = null;
                Global.SentryHealth = -1;
                Global.SentryLocation = null;
                Global.ValidQuestObjType = -1;

                SentryDied15s = false;
                ObjectLocations.Clear();
                PlayerNames.Clear();
                QuestId = -1;
                RareBagPriority = false;
                Disabled = false;
                ResetAllKeys();
                SeenObjs.Clear();
                SentryObjID = -1;
                ValidQuestId = -1;
                connectedClient = client;
                enemies.Clear();
                ReachedRareBag = false;
                followTarget = false;
                isInNexus = false;
                obstacles.Clear();
                playerPositions.Clear();
                CloseToQuestType = -1;
                TimeSincePentaDied.Reset();
                TimeSinceRealmClosed.Reset();
                GoodQuest.Reset();
                BadQuest.Reset();
                targets.Clear();
                /*if (Proxy.DefaultServer != lastserver) connectedClient.State.LastRealm = null;
                lastserver = Proxy.DefaultServer;*/
            };

            // Runs every time a client disconnects.
            proxy.ClientDisconnected += (client) =>
            {
                Log("Client disconnected. Waiting a few seconds before trying to press play...");
                PressPlay();
            };

            guiEvent += (evt) =>
            {
                switch (evt)
                {
                    case GuiEvent.StartBot:
                        Start();
                        break;
                    case GuiEvent.StopBot:
                        Stop();
                        break;
                    case GuiEvent.SettingsChanged:
                        Log("Updated config");
                        config = ConfigManager.GetConfiguration();
                        break;
                }
            };

            // Send an in game message when the gui fires the event.
            sendMessage += (message) =>
            {
                if (!(connectedClient?.Connected ?? false))
                    return;
                PlayerTextPacket packet = (PlayerTextPacket)Packet.Create(PacketType.PLAYERTEXT);
                packet.Text = message;
                connectedClient.SendToServer(packet);
            };

            #endregion


        }

        private void Loop()
        {
            while (true)
            {
                Process[] processes = Process.GetProcessesByName(config.FlashPlayerName);
                if (processes.Length == 1)
                {
                    //if processes[0].
                    // If there is one client open, bind to it.
                    Log("Automatically bound to client.");
                    flashPtr = processes[0].MainWindowHandle;
                    gui?.SetHandle(flashPtr);
                    PressPlay();
                    if (config.AutoConnect)
                        Start();
                }
                Thread.Sleep(10000);

                //auto binds to client for ahk auto restarter
            }
        }

        public void FileLog(string message)
        {
            using (StreamWriter Log = File.AppendText("RealmQuests.txt"))
            {
                Log.WriteLine(message);
            }
        }

        private void ReceiveCommand(Client client, string cmd, string[] args)
        {
            switch (cmd)
            {
                case "bind":
                    flashPtr = WinApi.GetForegroundWindow();

                    try
                    {
                        var flashProcess = Process.GetProcesses().Single(p => p.Id != 0 && p.MainWindowHandle == flashPtr);
                        if (flashProcess.ProcessName != config.FlashPlayerName)
                        {
                            gui?.ShowChangeFlashNameMessage(flashProcess.ProcessName, config.FlashPlayerName, () =>
                            {
                                config.FlashPlayerName = flashProcess.ProcessName;
                                client.Notify("Updated config!");
                                ConfigManager.WriteXML(config);
                            });
                        }
                    }
                    catch
                    {

                    }

                    gui?.SetHandle(flashPtr);
                    client.Notify("FameBot is now active");
                    break;
                case "start":
                    Start();
                    client.Notify("FameBot is starting");
                    break;
                case "gui":
                    gui?.Close();
                    gui = new FameBotGUI();
                    gui.Show();
                    //gui.SetHandle(flashPtr);
                    break;
                case "famebot":
                case "fb":
                    if (args.Length >= 1)
                    {
                        if (string.Compare("set", args[0], true) == 0)
                        {
                            if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                            {
                                client.Notify("No argument to set was provided");
                                return;
                            }
                            var setting = args[1].ToLower();
                            switch (setting)
                            {
                                case "realmposition":
                                case "rp":
                                    config.RealmLocation = client.PlayerData.Pos;
                                    ConfigManager.WriteXML(config);
                                    client.Notify("Successfully changed realm position!");
                                    break;
                                case "fountainposition":
                                case "fp":
                                    config.FountainLocation = client.PlayerData.Pos;
                                    ConfigManager.WriteXML(config);
                                    client.Notify("Successfully changed fountain position!");
                                    break;
                                default:
                                    client.Notify("Unrecognized setting.");
                                    break;
                            }
                        }
                        if (string.Compare("prefer", args[0], true) == 0)
                        {
                            if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                            {
                                client.Notify("No realm name was provided");
                                return;
                            }
                            preferredRealmName = args[1];
                            client.Notify("Set preferred realm to " + args[1]);
                        }
                    }
                    break;
                case "getdist":
                    XLog("Distance = " + client.PlayerData.Pos.DistanceTo(Global.QuestLocation));
                    break;
                case "001":
                    PluginUtils.ShowGenericSettingsGUI(_RealmQuests.Default, "Plugin Settings");
                    break;
            }
        }

        public static void InvokeGuiEvent(GuiEvent evt)
        {
            guiEvent?.Invoke(evt);
        }

        public static void InvokeSendMessageEvent(string message)
        {
            sendMessage?.Invoke(message);
        }

        private void Stop()
        {
            if (!enabled)
                return;
            Log("Stopping bot.");
            followTarget = false;
            gotoRealm = false;
            targets.Clear();
            enabled = false;
            isInNexus = false;
        }

        private void TimeToNexusCheck(Client client)
        {
            if (TimeSinceRealmClosed.ElapsedMilliseconds > 5000 && !EscapeIssued)
            {
                if (_RealmQuests.Default.NexusRealmClosedDelay == 0)
                {
                    int random = rnd.Next(15000, 60000);
                    PluginUtils.Delay(random, () => Escape(client, "Nexusing. (Delay: " + random + "ms)"));
                    EscapeIssued = true;
                }
                else
                {
                    int delay = _RealmQuests.Default.NexusRealmClosedDelay;
                    PluginUtils.Delay(delay, () => Escape(client, "Nexusing. (Delay: " + delay + "ms)"));
                    EscapeIssued = true;
                }
            }
        }

        private void NullQuestCheck(Client client, Location location)
        {
            if (Global.QuestLocation == location)
            {
                Global.QuestLocation = null;
                XLog("Quest: Nulled");
            }
        }


        private void Start()
        {
            if (enabled)
                return;
            if (Disabled)
                return;
            Log("Starting bot.");
            targets.Clear();
            enabled = true;
            if (currentMapName == null)
                return;
            if (currentMapName.Equals("Nexus") && config.AutoConnect)
            {
                // If the client is in the nexus, start moving towards the realms.
                gotoRealm = true;
                followTarget = false;
                if (connectedClient != null)
                    MoveToRealms(connectedClient);
            }
            else
            {
                gotoRealm = false;
                followTarget = true;
            }
        }

        /// <summary>
        /// Call this function to send an Escape packet.
        /// </summary>
        /// <param name="client">The client which will send the packet.</param>
        private void Escape(Client client, string m)
        {
            client.SendToServer(Packet.Create(PacketType.ESCAPE));
            XLog(m);
        }

        /// <summary>
        /// Print a message to the event log.
        /// </summary>
        /// <param name="message">The string to log.</param>
        private void Log(string message)
        {
            logEvent?.Invoke(this, new LogEventArgs(message));
        }

        /// <summary>
        /// Attempt to press the play button until the client connects.
        /// </summary>
        private async void PressPlay()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (Disabled)
                return;
            if (!config.AutoConnect)
                return;
            /*if (!enabled)
                return;*/

            if ((connectedClient?.Connected ?? false))
            {
                Log("Client is connected. No need to press play.");
                return;
            }
            else
                Log("Client still not connected. Pressing play button...");

            // Get the window details before pressing the button in case
            // it has changed size or position on the desktop.
            RECT windowRect = new RECT();
            WinApi.GetWindowRect(flashPtr, ref windowRect);
            var size = windowRect.GetSize();

            // The play button is located half way across the
            // window and roughly 92% of the way to the bottom.
            int playButtonX = size.Width / 2 + windowRect.Left;
            int playButtonY = (int)((double)size.Height * 0.92) + windowRect.Top;

            // Convert the screen point to a window point.
            POINT relativePoint = new POINT(playButtonX, playButtonY);
            WinApi.ScreenToClient(flashPtr, ref relativePoint);

            // Press the buttons.
            WinApi.SendMessage(flashPtr, (uint)MouseButton.LeftButtonDown, new IntPtr(0x1), new IntPtr((relativePoint.Y << 16) | (relativePoint.X & 0xFFFF)));
            WinApi.SendMessage(flashPtr, (uint)MouseButton.LeftButtonUp, new IntPtr(0x1), new IntPtr((relativePoint.Y << 16) | (relativePoint.X & 0xFFFF)));

            PressPlay();
        }

        /// <summary>
        /// Releases any keys which may be pressed.
        /// </summary>
        private void ResetAllKeys()
        {
            W_PRESSED = false;
            A_PRESSED = false;
            S_PRESSED = false;
            D_PRESSED = false;
        }

        #region PacketHookMethods
        private void OnUpdate(Client client, Packet p)
        {
            UpdatePacket packet = p as UpdatePacket;
            if (Disabled) return;
            // Get new info.

            /*
            if (_RealmQuests.Default.NexusReskin && currentMapName == "Nexus")
            {
                foreach (Tile tile in packet.Tiles)
                {
                    if (((tile.X <= 156 && tile.Y >= 111) ||
                        (tile.X >= 161 && tile.Y >= 111) ||
                        (tile.Y >= 136)) && (tile.Y != 135))
                    {
                        tile.Type = GameData.Tiles.ByName("Red Earth Water Deep").ID;
                    }
                    else if (tile.Y >= 111)
                    {
                        tile.Type = GameData.Tiles.ByName("Metal Plate Stripes").ID;
                    }
                    if (tile.Y == 135)
                    {
                        if (tile.X <= 154 || tile.X >= 163)
                        {
                            tile.Type = GameData.Tiles.ByName("Red Earth Water Deep").ID;
                        }
                    }
                }
            }
            */

            foreach (Entity obj in packet.NewObjs)
            {

                if (obj.Status.ObjectId == QuestId)
                {
                    if (Debug) XLog("Quest: " + obj.Status.ObjectId + " " + (GameData.Objects.ByID((UInt16)obj.ObjectType).Name));

                    if (Enum.IsDefined(typeof(RealmQuests), (ushort)obj.ObjectType))
                    {
                        if (!SeenQuests.ContainsKey(obj.Status.ObjectId)) SeenQuests.Add(obj.Status.ObjectId, obj.ObjectType);
                        if (!SeenQuestsLoc.ContainsKey(obj.Status.ObjectId)) SeenQuestsLoc.Add(obj.Status.ObjectId, obj.Status.Position);

                        if (RareBagPriority)
                        {
                            if (Debug) XLog("Quest ignored, whitebag nearby");
                        }
                        else if ((obj.ObjectType == Convert.ToUInt16(RealmQuests.Lich)
                            || obj.ObjectType == Convert.ToUInt16(RealmQuests.ActualLich))
                            && _RealmQuests.Default.IgnoreLich)
                        {
                            if (Debug) XLog("Quest: Lich (Ignored)");
                        }
                        else if ((obj.ObjectType == Convert.ToUInt16(RealmQuests.GhostKing)
                            || obj.ObjectType == Convert.ToUInt16(RealmQuests.ActualGhostKing))
                            && _RealmQuests.Default.IgnoreGhostKing)
                        {
                            if (Debug) XLog("Quest: Ghost King (Ignored)");
                        }
                        else if (obj.ObjectType == Convert.ToUInt16(RealmQuests.Avatar)
                            && _RealmQuests.Default.IgnoreAvatar)
                        {
                            if (Debug) XLog("Quest: Avatar (Ignored)");
                        }
                        else if (obj.ObjectType == Convert.ToUInt16(RealmQuests.Pentaract)
                            && _RealmQuests.Default.IgnorePentaract)
                        {
                            if (Debug) XLog("Quest: Penta (Ignored)");
                        }
                        else if ((obj.ObjectType == Convert.ToUInt16(RealmQuests.GarnetStatue)
                            || obj.ObjectType == Convert.ToUInt16(RealmQuests.JadeStatue))
                            && _RealmQuests.Default.IgnoreStatues)
                        {
                            if (Debug) XLog("Quest: Statues (Ignored)");
                        }
                        else if (obj.ObjectType == Convert.ToUInt16(RealmQuests.CubeGod)
                            && _RealmQuests.Default.IgnoreCube)
                        {
                            if (Debug) XLog("Quest: Cube (Ignored)");
                        }
                        else if ((obj.ObjectType == Convert.ToUInt16(RealmQuests.EntAncient)
                            || obj.ObjectType == Convert.ToUInt16(RealmQuests.ActualEntAncient))
                            && _RealmQuests.Default.IgnoreEnt)
                        {
                            if (Debug) XLog("Quest: Ent (Ignored)");
                        }
                        else if (obj.Status.ObjectId == BlockObjId)
                        {
                            if (Debug) XLog("Fucking bees");

                        }
                        else
                        {
                            if (!Global.FarmingLostSentry)
                            {
                                Global.QuestLocation = obj.Status.Position;
                            }
                            if (Debug) XLog("Quest: " + obj.Status.ObjectId + " " + (GameData.Objects.ByID((UInt16)obj.ObjectType).Name) + " (Assigned)");

                            GoodQuest.Restart();
                            BadQuest.Reset();
                            if (Debug)
                            {
                                XLog("Bad Quest reset");
                                XLog("Good Quest restart");
                            }
                            ValidQuestId = QuestId;
                            QuestId = -1;
                            Global.ValidQuestObjType = obj.ObjectType;
                        }

                        if (QuestId != -1)
                        {
                            if (Debug) XLog("Good Quest reset");
                            GoodQuest.Reset();
                            if (BadQuest.ElapsedMilliseconds == 0)
                            {
                                if (Debug) XLog("Bad Quest restart");
                                BadQuest.Restart();
                            }
                        }

                        if (GameData.Objects.ByID((UInt16)obj.ObjectType).Name == "Horned Drake" && client.PlayerData.Level > 19)
                        {
                            TimeSinceRealmClosed.Start();
                        }
                    }
                }

                if (obj.ObjectType == Convert.ToUInt16(RealmQuests.LostSentry))
                {
                    if (!_RealmQuests.Default.IgnoreSentry)
                    {
                        SentryObjID = obj.Status.ObjectId;
                        Global.SentryLocation = obj.Status.Position;

                        foreach (StatData statData in obj.Status.Data)
                        {
                            if (statData.Id == StatsType.HP)
                            {
                                Global.SentryHealth = statData.IntValue;
                            }
                        }
                        if (Global.SentryHealth < _RealmQuests.Default.TeleportToSentryHealth && Global.SentryLocation != null && SentryObjID != -1)
                        {
                            Global.FarmingLostSentry = true;
                            XLog("Lost Sentry Health: " + Global.SentryHealth);
                        }
                    }
                    else
                    {
                        if (Debug) XLog("Ignored Sentry");
                    }
                }


                // Player info.
                if (Enum.IsDefined(typeof(Classes), (short)obj.ObjectType))
                {
                    if (obj.ObjectType != (short)Classes.Rogue) ObjectLocations.Add(obj.Status.ObjectId, obj.Status.Position);
                    foreach (StatData statData in obj.Status.Data)
                    {
                        if (statData.Id == StatsType.LootDropBoostTime && obj.Status.ObjectId == client.ObjectId)
                        {
                            if (statData.IntValue > 0) lootdrop = statData.IntValue;
                            else lootdrop = 0;
                        }
                        if (statData.Id == StatsType.Name)
                        {
                            PlayerNames.Add(obj.Status.ObjectId, statData.StringValue);
                            /*if (name == "Wyrm")
                            {
                                Disabled = true;
                            }*/
                        }
                        /*
                        if (statData.Id == StatsType.Name && client.ObjectId == obj.Status.ObjectId)
                        {
                            name = statData.StringValue;
                            if (name == "Wyrm")
                            {
                                Disabled = true;
                            }
                        }
                        */
                        /*if (statData.Id == StatsType.Size && client.ObjectId != obj.Status.ObjectId)
                        {
                            if (name == "Ziar" || name == "Ziiar")
                            {
                                statData.IntValue = 200;
                            }
                            if (name == "Wyrm")
                            {
                                statData.IntValue = 700;
                            }
                        }*/
                    }
                    PlayerData playerData = new PlayerData(obj.Status.ObjectId);
                    playerData.Class = (Classes)obj.ObjectType;
                    playerData.Pos = obj.Status.Position;
                    foreach (var data in obj.Status.Data)
                    {
                        playerData.Parse(data.Id, data.IntValue, data.StringValue);
                    }

                    if (playerPositions.ContainsKey(obj.Status.ObjectId))
                        playerPositions.Remove(obj.Status.ObjectId);
                    if (playerData.Stars >= _RealmQuests.Default._MinStarsToCluster) playerPositions.Add(obj.Status.ObjectId, new Target(obj.Status.ObjectId, playerData.Name, playerData.Pos));
                }

                // Portals.
                if (obj.ObjectType == 1810)
                {
                    foreach (var data in obj.Status.Data)
                    {
                        if (data.StringValue != null)
                        {
                            // Get the portal info.
                            // This regex matches the name and the player count of the portal.
                            string pattern = @"\.(\w+) \((\d+)";
                            var match = Regex.Match(data.StringValue, pattern);

                            var portal = new Portal(obj.Status.ObjectId, int.Parse(match.Groups[2].Value), match.Groups[1].Value, obj.Status.Position);
                            if (portals.Exists(ptl => ptl.ObjectId == obj.Status.ObjectId))
                                portals.RemoveAll(ptl => ptl.ObjectId == obj.Status.ObjectId);
                            portals.Add(portal);
                        }
                    }
                }

                if (obj.ObjectType == 0x10d9) //EH Hive Bomb
                {
                    BlockObjId = ValidQuestId;
                    Escape(client, "There's fucking bees in here.");
                }

                if (currentMapName != "Nexus")
                {
                    if (!SeenObjs.ContainsKey(obj.Status.ObjectId)
                        && !RareBagPriority
                        && (obj.ObjectType == Orange
                        || obj.ObjectType == OrangeBoosted
                        || obj.ObjectType == Red
                        || obj.ObjectType == RedBoosted
                        || obj.ObjectType == Gold
                        || obj.ObjectType == GoldBoosted
                        || obj.ObjectType == White
                        || obj.ObjectType == WhiteBoosted
                        || ((obj.ObjectType == Blue
                        || obj.ObjectType == BlueBoosted)
                        && _RealmQuests.Default.MoveToBlueBags)))
                    {
                        if (obj.ObjectType == White
                            || obj.ObjectType == WhiteBoosted
                            || obj.ObjectType == Orange
                            || obj.ObjectType == OrangeBoosted
                            || obj.ObjectType == Red
                            || obj.ObjectType == RedBoosted)
                        {
                            RareBagPriority = true;
                            XLog("Rare Bag");
                            PluginUtils.Delay(23000, () => RareBagPriority = false);
                        }

                        //test
                        /*if (obj.ObjectType == Blue)
                        {
                            RareBagPriority = true;
                            PluginUtils.Delay(15000, () => RareBagPriority = false);
                        }*/

                        SeenObjs.Add(obj.Status.ObjectId, obj.Status.ObjectId);

                        FileLog("	| BAG: " + Enum.GetName(typeof(BagsX), (ushort)obj.ObjectType));

                        DoBagScan(client, obj);

                        int MoveToBagDelay;
                        if (_RealmQuests.Default.MoveToBagDelay > 50)
                        {

                            MoveToBagDelay = _RealmQuests.Default.MoveToBagDelay;
                        }
                        else
                        {
                            MoveToBagDelay = rnd.Next(2000, 3500);
                        }

                        if (CanMoveToBlueBags || RareBagPriority)
                        {
                            PluginUtils.Delay(MoveToBagDelay - 20, () => followTarget = false);
                            PluginUtils.Delay(MoveToBagDelay - 15, () => targets.Clear());
                            PluginUtils.Delay(MoveToBagDelay - 10, () => target_ = obj.Status.Position);
                            PluginUtils.Delay(MoveToBagDelay - 10, () => gotoRealm = true);
                            PluginUtils.Delay(MoveToBagDelay, () => MoveToRealms(connectedClient));
                            PluginUtils.Delay(MoveToBagDelay, () => XLog("Moving to bag."));
                            if (RareBagPriority)
                            {
                                PluginUtils.Delay(MoveToBagDelay + 23000, () => followTarget = true);
                                PluginUtils.Delay(MoveToBagDelay + 23000, () => gotoRealm = false);
                                PluginUtils.Delay(MoveToBagDelay + 23000, () => target_ = null);
                                PluginUtils.Delay(MoveToBagDelay + 23000, () => XLog("Stopped moving to bag."));
                            }
                            else
                            {
                                PluginUtils.Delay(MoveToBagDelay + 5000, () => followTarget = true);
                                PluginUtils.Delay(MoveToBagDelay + 5000, () => gotoRealm = false);
                                PluginUtils.Delay(MoveToBagDelay + 5000, () => target_ = null);
                                PluginUtils.Delay(MoveToBagDelay + 5000, () => XLog("Stopped moving to bag."));

                            }
                        }
                        else
                        {
                            XLog("Unsafe to move.");
                        }
                    }
                }

                // Enemies. Only look for enemies if EnableEnemyAvoidance is true.
                if (Enum.IsDefined(typeof(EnemyId), (int)obj.ObjectType) && config.EnableEnemyAvoidance)
                {
                    if (enemies.Exists(en => en.ObjectId == obj.Status.ObjectId))
                        enemies.RemoveAll(en => en.ObjectId == obj.Status.ObjectId);
                    enemies.Add(new Enemy(obj.Status.ObjectId, obj.Status.Position));
                }

                // Obstacles.
                if (obstacleIds.Contains(obj.ObjectType))
                {
                    if (!obstacles.Exists(obstacle => obstacle.ObjectId == obj.Status.ObjectId))
                        obstacles.Add(new Obstacle(obj.Status.ObjectId, obj.Status.Position));
                }
            }

            // Remove old info
            foreach (int dropId in packet.Drops)
            {
                if (ObjectLocations.ContainsKey(dropId))
                {
                    PlayerNames.Remove(dropId);
                    ObjectLocations.Remove(dropId);
                }
                if (dropId == ValidQuestId)
                {
                    if (SeenQuests[dropId] == Convert.ToInt16(RealmQuests.Pentaract))
                    {
                        PluginUtils.Delay(15000, () => NullQuestCheck(client, SeenQuestsLoc[dropId]));
                    }
                    else if (SeenQuests[dropId] == Convert.ToInt16(RealmQuests.ActualEntAncient)
                        || SeenQuests[dropId] == Convert.ToInt16(RealmQuests.EntAncient))
                    {
                        PluginUtils.Delay(13000, () => NullQuestCheck(client, SeenQuestsLoc[dropId]));
                    }
                    else if (SeenQuests[dropId] == Convert.ToInt16(RealmQuests.GarnetStatue)
    || SeenQuests[dropId] == Convert.ToInt16(RealmQuests.JadeStatue))
                    {
                        PluginUtils.Delay(25000, () => NullQuestCheck(client, SeenQuestsLoc[dropId]));
                    }
                    else
                    {
                        Global.QuestLocation = null;
                        GoodQuest.Reset();
                        if (Debug) XLog("ObjId: " + dropId + " (Dropped)");
                        if (Debug) XLog("Good Quest reset #2");
                    }
                }
                if (dropId == SentryObjID)
                {
                    Global.FarmingLostSentry = false;
                    Global.SentryLocation = null;
                    Global.SentryHealth = -1;
                    SentryObjID = -1;

                    SentryDied15s = true;
                    PluginUtils.Delay(15000, () => SentryDied15s = false);

                    if (Debug) XLog("Handled Lost Sentry Death");
                }
                // Remove from players list.
                if (playerPositions.ContainsKey(dropId))
                {
                    if (followTarget && targets.Exists(t => t.ObjectId == dropId))
                    {
                        // If one of the players who left was also a target, remove them from the targets list.
                        targets.Remove(targets.Find(t => t.ObjectId == dropId));
                        Log(string.Format("Dropping \"{0}\" from targets.", playerPositions[dropId].Name));
                        if (targets.Count == 0)
                        {
                            Log("No targets left in target list.");
                            if (config.EscapeIfNoTargets)
                                Escape(client, "No targets left in target list.");
                        }
                    }
                    playerPositions.Remove(dropId);
                }

                // Remove from enemies list.
                if (enemies.Exists(en => en.ObjectId == dropId))
                    enemies.RemoveAll(en => en.ObjectId == dropId);

                if (portals.Exists(ptl => ptl.ObjectId == dropId))
                    portals.RemoveAll(ptl => ptl.ObjectId == dropId);
            }
        }

        private void DoBagScan(Client client, Entity obj)
        {
            foreach (StatData statData in obj.Status.Data)
            {
                if (statData.Id >= 8 && statData.Id <= 19)
                {
                    if (statData.IntValue != -1)
                    {
                        FileLog("		| " + GameData.Items.ByID((UInt16)statData.IntValue).Name);
                    }
                }
            }
        }

        /*
        private void DisableCheck()
        {
            if (ServerName == "USWest2") Disabled = true;
        }*/


        private void OnMapInfo(Client client, Packet p)
        {
            MapInfoPacket packet = p as MapInfoPacket;

            TimeSinceMaptime.Restart();
            if (packet == null)
                return;
            portals.Clear();
            currentMapName = packet.Name;
            string map = packet.Name;
            if (map == "Realm of the Mad God") map = "Realm";

            if (packet.Name == "Oryx's Castle" && enabled)
            {
                // If the new map is oryx, go back to the nexus.
                Log("Escaping from oryx's castle.");
                Escape(client, "Nexusing... Oryx's castle");
                return;
            }

            FileLog(Environment.NewLine + "MAP: " + map + DateTime.Now.ToString(" (hh:mm:sstt)", CultureInfo.InvariantCulture));

            if (lootdrop != 0)
            {
                FileLog("	| ODDS: 1.25 (" + TimeSpan.FromSeconds(lootdrop) + ")");
            }
            else
            {
                FileLog("	| ODDS: 1.0");
            }

            if (packet.Name == "Nexus" && config.AutoConnect && enabled)
            {
                // If the new map is the nexus, start moving towards the realms again.
                isInNexus = true;
                gotoRealm = true;
                MoveToRealms(client);
            }
            else
            {
                gotoRealm = false;
                if (enabled)
                    followTarget = true;
            }
        }

        private void OnHit(Client client, Packet p)
        {
            // Check health percentage for autonexus.
            float healthPercentage = (float)client.PlayerData.Health / (float)client.PlayerData.MaxHealth * 100f;
            if (healthPercentage < config.AutonexusThreshold * 1.25f)
                Log(string.Format("Health at {0}%", (int)(healthPercentage)));
        }

        private void XLog(string s)
        {
            string time = DateTime.Now.ToString(" [hh:mm:sstt]", CultureInfo.InvariantCulture);

            PluginUtils.Log("RealmQuests", s + time);
            connectedClient.SendToClient(PluginUtils.CreateOryxNotification("RealmQuests", s + time));
        }

        private void GetCloseQuest(Client client)
        {
            foreach (var pair in SeenQuestsLoc)
            {
                float distance = pair.Value.DistanceTo(client.PlayerData.Pos);
                if (distance < 15)
                {
                    CloseToQuestType = SeenQuests[pair.Key];
                }
            }

            if (CloseToQuestType == Convert.ToUInt16(RealmQuests.EntAncient)
                || CloseToQuestType == Convert.ToUInt16(RealmQuests.ActualEntAncient))
            {
                Epsilon = 10f;
                //MinPoints = 3;
            }
            else
            {
                Epsilon = 0;
                MinPoints = 0;
            }

            if (CloseToQuestType != Convert.ToUInt16(RealmQuests.GarnetStatue) && CloseToQuestType != Convert.ToUInt16(RealmQuests.JadeStatue))
            {
                CanMoveToBlueBags = true;
            }
            else
            {
                CanMoveToBlueBags = false;
            }
        }

        private void OnNewTick(Client client, Packet p)
        {
            NewTickPacket packet = p as NewTickPacket;
            tickCount++;

            GetCloseQuest(client);

            if (target_ != null && client.PlayerData.Pos != null & _RealmQuests.Default.TeleportToBag)
            {
                if (client.PlayerData.Pos.DistanceTo(target_) < 1.1) ReachedRareBag = true;

                if (RareBagPriority && client.PlayerData.Pos.DistanceTo(target_) > 1.1 && !ReachedRareBag)
                {
                    int target = 0;
                    float minDistance = 1;

                    foreach (var pair in ObjectLocations)
                    {
                        //if (pair.Value != null)
                        float distance = target_.DistanceTo(pair.Value);
                        if (target_.DistanceTo(pair.Value) < minDistance)
                        {
                            target = pair.Key;
                            minDistance = distance;
                        }
                    }

                    if (target != 0)
                    {
                        TeleportPacket teleport = (TeleportPacket)Packet.Create(PacketType.TELEPORT);
                        teleport.ObjectId = target;
                        client.SendToServer(teleport);
                        XLog("Teleporting to " + target);
                    }
                }
            }

            if (currentMapName == "Realm of the Mad God")
            {
                this.TimeToNexusCheck(connectedClient);
                if (!Global.FarmingLostSentry
                && !SentryDied15s
                && !RareBagPriority
                && Global.TeleportedInland
                )//&& !Teleported4s
                {
                    if (_RealmQuests.Default.TeleportAwayDelay > 0) { rTeleportDelay = _RealmQuests.Default.TeleportAwayDelay; }
                    //float EntDistance = 5;
                    //bool Ent = (Global.ValidQuestObjType == Convert.ToUInt16(RealmQuests.EntAncient) || Global.ValidQuestObjType == Convert.ToUInt16(RealmQuests.ActualEntAncient));
                    //if (Global.QuestLocation != null) EntDistance = client.PlayerData.Pos.DistanceTo(Global.QuestLocation);

                    if (BadQuest.ElapsedMilliseconds > (rTeleportDelay + 23000))
                    //|| (GoodQuest.ElapsedMilliseconds > (rTeleportDelay + 23000) && Ent && EntDistance > 25)) // || (Ent && EntDistance > 25)
                    {
                        //Teleported4s = true;
                        //PluginUtils.Delay(4000, () => Teleported4s = false);

                        Global.TeleportedInland = false;
                        XLog("Attempting to TP away");
                        List<int> names = new List<int>();

                        foreach (var pair in ObjectLocations)
                        {
                            float distance = pair.Value.DistanceTo(new Location(1000, 1000));

                            if (distance > 600)
                            {
                                names.Add(pair.Key);
                                Console.WriteLine(PlayerNames[pair.Key] + " - " + distance);
                            }
                        }

                        if (names.Count != 0)
                        {
                            var randomized = names.OrderBy(a => Guid.NewGuid()).ToList();
                            int obj = randomized.First();
                            TeleportPacket teleport = (TeleportPacket)Packet.Create(PacketType.TELEPORT);
                            teleport.ObjectId = obj;
                            client.SendToServer(teleport);
                            XLog("No valid quest. Teleporting away to: " + PlayerNames[obj]);
                        }
                        else
                        {
                            XLog("No valid quest. No one to teleport to!");
                        }
                        names.Clear();
                    }
                }
            }

            // Health changed event.
            float healthPercentage = (float)client.PlayerData.Health / (float)client.PlayerData.MaxHealth * 100f;
            healthChanged?.Invoke(this, new HealthChangedEventArgs(healthPercentage));

            // Autonexus.
            if (healthPercentage < config.AutonexusThreshold && !(currentMapName?.Equals("Nexus") ?? false) && enabled)
                Escape(client, "Nexusing... Autonexus triggered.");

            // Fame event.
            fameUpdate?.Invoke(this, new FameUpdateEventArgs(client.PlayerData?.CharacterFame ?? -1, client.PlayerData?.CharacterFameGoal ?? -1));

            if (tickCount % config.TickCountThreshold == 0)
            {
                if (followTarget && playerPositions.Count > 0 && !gotoRealm)
                {
                    if (Epsilon == 0) Epsilon = config.Epsilon;
                    if (MinPoints == 0) MinPoints = config.MinPoints;
                    List<Target> newTargets = D36n4.Invoke(playerPositions.Values.ToList(), Epsilon, MinPoints, config.FindClustersNearCenter);
                    if (newTargets == null)
                    {
                        if (targets.Count != 0 && config.EscapeIfNoTargets)
                            Escape(client, "Nexusing... no valid targets");
                        targets.Clear();
                        Log("No valid clusters found.");
                        ResetAllKeys();

                    }
                    else// if (newTargets.First().Position.DistanceTo(QuestLocation) < 50)
                    {
                        if (targets.Count != newTargets.Count)
                            Log(string.Format("Now targeting {0} players.", newTargets.Count));
                        targets = newTargets;
                    }
                }
                tickCount = 0;
            }

            // Updates.
            foreach (Status status in packet.Statuses)
            {
                if (ObjectLocations.ContainsKey(status.ObjectId)) ObjectLocations[status.ObjectId] = status.Position;
                if (SeenQuestsLoc.ContainsKey(status.ObjectId)) SeenQuestsLoc[status.ObjectId] = status.Position;

                if (status.ObjectId == ValidQuestId)
                {
                    Global.QuestLocation = status.Position;

                }

                if (status.ObjectId == SentryObjID)
                {
                    Global.SentryLocation = status.Position;
                    foreach (StatData statData in status.Data)
                    {
                        if (statData.Id == StatsType.HP)
                        {
                            Global.SentryHealth = statData.IntValue;
                        }
                    }
                    if (Global.SentryHealth < _RealmQuests.Default.TeleportToSentryHealth && Global.SentryLocation != null && SentryObjID != -1)
                    {
                        Global.FarmingLostSentry = true;
                        XLog("Lost Sentry Health: " + Global.SentryHealth);
                    }
                }

                // Update player positions.
                if (playerPositions.ContainsKey(status.ObjectId))
                    playerPositions[status.ObjectId].UpdatePosition(status.Position);

                // Update enemy positions.
                if (enemies.Exists(en => en.ObjectId == status.ObjectId))
                    enemies.Find(en => en.ObjectId == status.ObjectId).Location = status.Position;

                // Update portal player counts when in nexus.
                if (portals.Exists(ptl => ptl.ObjectId == status.ObjectId) && (isInNexus))
                {
                    foreach (var data in status.Data)
                    {
                        if (data.StringValue != null)
                        {
                            var strCount = data.StringValue.Split(' ')[1].Split('/')[0].Remove(0, 1);
                            portals[portals.FindIndex(ptl => ptl.ObjectId == status.ObjectId)].PlayerCount = int.Parse(strCount);
                        }
                    }
                }

                // Change the speed if in Nexus.
                if (isInNexus && status.ObjectId == client.ObjectId)
                {
                    foreach (var data in status.Data)
                    {
                        if (data.Id == StatsType.Speed)
                        {
                            if (data.IntValue > 45)
                            {
                                List<StatData> list = new List<StatData>(status.Data) {
                                    new StatData {
                                        Id = StatsType.Speed, IntValue = 45
                                    }
                                };
                                status.Data = list.ToArray();
                            }
                        }
                    }
                }
            }


            // If the client has stopped moving for whatever reason, reset the keys.
            if (enabled)
            {
                if (lastLocation != null)
                {
                    if (client.PlayerData.Pos.X == lastLocation.X && client.PlayerData.Pos.Y == lastLocation.Y)
                    {
                        //Console.WriteLine("Keys reset by Famebot");

                        ResetAllKeys();
                    }
                }
                lastLocation = client.PlayerData.Pos;
            }

            // Reset keys if the bot is not active.
            if (!followTarget && !gotoRealm)
            {
                ResetAllKeys();
            }

            if (followTarget && targets.Count > 0)
            {
                // Get the target position: the average of all current targets.
                var targetPosition = new Location(targets.Average(t => t.Position.X), targets.Average(t => t.Position.Y));
                if (lastAverage != null)
                {
                    var dir = targetPosition.Subtract(lastAverage);
                    var faraway = targetPosition.Add(dir.Scale(20));
                    var desiredTargets = (int)(targets.Count * (config.TrainTargetPercentage / 100f));
                    List<Target> newTargets = new List<Target>();
                    for (int i = 0; i < desiredTargets; i++)
                    {
                        var closest = targets.OrderBy((t) => t.Position.DistanceSquaredTo(faraway)).First();
                        newTargets.Add(closest);
                        targets.RemoveAll((t) => t.Name == closest.Name);
                    }
                    targets.AddRange(newTargets);
                    lastAverage = targetPosition;
                    targetPosition = new Location(newTargets.Average(t => t.Position.X), newTargets.Average(t => t.Position.Y));
                }
                else
                {
                    lastAverage = targetPosition;
                }

                if (client.PlayerData.Pos.DistanceTo(targetPosition) > config.TeleportDistanceThreshold && !RareBagPriority)
                //&& (targetPosition.DistanceTo(Global.QuestLocation) < _RealmQuests.Default.QuestTeleportDistance)
                {
                    // If the distance exceeds the teleport threshold, send a text packet to teleport.
                    var name = targets.OrderBy(t => t.Position.DistanceTo(targetPosition)).First().Name;
                    if (name != client.PlayerData.Name)
                    {
                        var tpPacket = (PlayerTextPacket)Packet.Create(PacketType.PLAYERTEXT);
                        tpPacket.Text = "/teleport " + name;
                        Global.TeleportedInland = true;
                        client.SendToServer(tpPacket);

                        //XLog("Issued teleport to " + PlayerLocation)
                    }
                }

                // There should never be anything in the enemies list if EnableEnemyAvoidance is false,
                // but just in case, only perform this behaviour if EnableEnemyAvoidance is true.
                if (config.EnableEnemyAvoidance && enemies.Exists(en => en.Location.DistanceSquaredTo(client.PlayerData.Pos) <= (config.EnemyAvoidanceDistance * config.EnemyAvoidanceDistance)))
                {
                    // If there is an enemy within the specified number of tiles, actively attempt to avoid it.
                    Location closestEnemy = enemies.OrderBy(en => en.Location.DistanceSquaredTo(client.PlayerData.Pos)).First().Location;
                    double angleDifference = client.PlayerData.Pos.GetAngleDifferenceDegrees(targetPosition, closestEnemy);

                    if (Math.Abs(angleDifference) < 70.0)
                    {
                        // Get the angle between the enemy and the player.
                        double angle = Math.Atan2(client.PlayerData.Pos.Y - closestEnemy.Y, client.PlayerData.Pos.X - closestEnemy.X);
                        if (angleDifference <= 0)
                            angle += (Math.PI / 2); // add 90 degrees to the angle to go clockwise around the enemy.
                        if (angleDifference > 0)
                            angle -= (Math.PI / 2); // remove 90 degrees from the angle to go anti-clockwise around the enemy.

                        // Calculate a point on a 'circle' around the enemy with a radius 8 at the angle specified.
                        float newX = closestEnemy.X + config.EnemyAvoidanceDistance * (float)Math.Cos(angle);
                        float newY = closestEnemy.Y + config.EnemyAvoidanceDistance * (float)Math.Sin(angle);

                        var avoidPos = new Location(newX, newY);
                        CalculateMovement(client, avoidPos, config.FollowDistanceThreshold);
                        return;
                    }
                }

                if (obstacles.Exists(obstacle => obstacle.Location.DistanceSquaredTo(client.PlayerData.Pos) <= 4))
                {
                    // If there is an obstacle within 2 tiles, actively attempt to move around it.
                    Location closestObstacle = obstacles.OrderBy(obstacle => obstacle.Location.DistanceSquaredTo(client.PlayerData.Pos)).First().Location;
                    double angleDifference = client.PlayerData.Pos.GetAngleDifferenceDegrees(targetPosition, closestObstacle);

                    if (Math.Abs(angleDifference) < 70.0)
                    {
                        double angle = Math.Atan2(client.PlayerData.Pos.Y - closestObstacle.Y, client.PlayerData.Pos.X - closestObstacle.X);
                        if (angleDifference <= 0)
                            angle += (Math.PI / 2); // add 90 degrees to the angle to go clockwise around the obstacle.
                        if (angleDifference > 0)
                            angle -= (Math.PI / 2); // remove 90 degrees from the angle to go anti-clockwise around the obstacle.

                        float newX = closestObstacle.X + 2f * (float)Math.Cos(angle);
                        float newY = closestObstacle.Y + 2f * (float)Math.Sin(angle);

                        var avoidObstaclePos = new Location(newX, newY);
                        CalculateMovement(client, avoidObstaclePos, 0.5f);
                        return;
                    }
                }

                CalculateMovement(client, targetPosition, config.FollowDistanceThreshold);

                /*
                if (SentryObjID != -1 && SentryLocation != null && !SentryTeleported)
                {

                    if (SentryHealth <= _RealmQuests.Default.SentryLowHealthTP)
                    {
                        float minDistance = 50f;
                        int target = 0;

                        foreach (var pair in ObjectLocations)
                        {
                            float distance = pair.Value.DistanceSquaredTo(SentryLocation);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                target = pair.Key;
                            }
                        }

                        if (target != 0)
                        {
                            TeleportPacket teleport = (TeleportPacket)Packet.Create(PacketType.TELEPORT);
                            teleport.ObjectId = target;
                            client.SendToServer(teleport);
                            PluginUtils.Delay(50, () => Start());
                            SentryTeleported = true;
                        }

                        //Console.WriteLine(QuestLocation.DistanceSquaredTo(client.PlayerData.Pos));
                    }
                }
                */
            }
        }

        private void OnText(Client client, Packet p)
        {
            TextPacket packet = p as TextPacket;
            PlayerTextPacket p2 = (PlayerTextPacket)Packet.Create(PacketType.PLAYERTEXT);

            if (packet.NumStars == -1)
            {
                if (packet.Text.Contains("server.oryx_closed_realm") && currentMapName == "Realm of the Mad God")
                {
                    TimeSinceRealmClosed.Start();
                    XLog("Realm closed.");
                    XLog("Realm closed.");
                    BlockObjId = -1;

                    XLog("Realm closed.");
                }
                //[Mila]           #Mysterious Crystal: Sweet treasure awaits for powerful adventurers!

                /*if (packet.Text.Contains("Free at last!") && _RealmQuests.Default._FarmCrystal_NotImplemented)
                {
                    Global.FarmingCrystal = true;
                    Console.WriteLine("Attempting to farm crystal");
                }

                if (packet.Text.Contains("I'm finally free! Yesss!!!"))
                {
                    Global.FarmingCrystal = false;
                    Console.WriteLine("Stopped farming crystal");

                }*/
            }

            /*
            if (packet.NumStars != -1)
            {
                if (packet.Text.Contains("crystal"))
                {
                    XLog(packet.Text);
                }
            }*/


            if (packet.Name == client.PlayerData?.Name || packet.NumStars < 1)
                return;
            if (Regex.IsMatch(packet.Text, spampattern, RegexOptions.IgnoreCase) || packet.Text == " ")
                return;
            receiveMesssage?.Invoke(this, new MessageEventArgs(packet.Text, packet.Name, packet.Recipient == client.PlayerData?.Name ? true : false));
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        private async void MoveToRealms(Client client, bool realmChosen = false)
        {
            if (Disabled) return;

            if (client == null)
            {
                Log("No client passed to MoveToRealms.");
                return;
            }
            Location target = config.RealmLocation;

            if (client.PlayerData == null)
            {
                await Task.Delay(5);
                MoveToRealms(client);
                return;
            }

            var healthPercentage = (float)client.PlayerData.Health / (float)client.PlayerData.MaxHealth;
            if (healthPercentage < 0.95f && currentMapName == "Nexus")
                target = config.FountainLocation;

            if (target_ != null) target = target_;

            else if ((client.PlayerData.Pos.Y <= config.RealmLocation.Y + 1f && client.PlayerData.Pos.Y != 0) || realmChosen)
            {
                // When the client reaches the portals, evaluate the best option.
                if (portals.Count != 0)
                {
                    bool hasNoPreferredRealm = true;
                    // Is there a preferred realm?
                    if (!string.IsNullOrEmpty(preferredRealmName))
                    {
                        if (portals.Exists(ptl => string.Compare(ptl.Name, preferredRealmName, true) == 0))
                        {
                            hasNoPreferredRealm = false;
                            Portal preferred = portals.Single(ptl => string.Compare(ptl.Name, preferredRealmName, true) == 0);
                            target = preferred.Location;
                            bestName = preferred.Name;
                            realmChosen = true;
                        }
                        else
                        {
                            // The preferred realm doesn't exist anymore.
                            client.Notify(preferredRealmName + " not found. Choosing new realm");
                            Log("The realm \"" + preferredRealmName + "\" was not found. Choosing the best realm instead...");
                            preferredRealmName = null;
                        }
                    }

                    if (hasNoPreferredRealm)
                    {
                        int bestCount = 0;
                        if (portals.Where(ptl => ptl.PlayerCount == 85).Count() > 1)
                        {
                            foreach (Portal ptl in portals.Where(ptl => ptl.PlayerCount == 85))
                            {
                                int count = playerPositions.Values.Where(plr => plr.Position.DistanceSquaredTo(ptl.Location) <= 4).Count();
                                if (count > bestCount)
                                {
                                    bestCount = count;
                                    bestName = ptl.Name;
                                    target = ptl.Location;
                                    realmChosen = true;
                                }
                            }
                        }
                        else
                        {
                            Portal ptl = portals.OrderByDescending(prtl => prtl.PlayerCount).First();
                            target = ptl.Location;
                            bestName = ptl.Name;
                            realmChosen = true;
                        }
                    }
                }
                else if (currentMapName == "Nexus") target = config.RealmLocation;
            }

            CalculateMovement(client, target, 0.5f);

            //            if (client.PlayerData.Pos.DistanceTo(target) < 1f && portals.Count != 0 && target != bag_target)

            if (client.PlayerData.Pos.DistanceTo(target) < 1f && portals.Count != 0 && target != target_)
            {
                if (client.PlayerData.Pos.DistanceTo(target) <= client.PlayerData.TilesPerTick() && client.PlayerData.Pos.DistanceTo(target) > 0.01f)
                {
                    if (client.Connected)
                    {
                        ResetAllKeys();
                        GotoPacket gotoPacket = Packet.Create(PacketType.GOTO) as GotoPacket;
                        gotoPacket.Location = target;
                        gotoPacket.ObjectId = client.ObjectId;
                        blockNextAck = true;
                        client.SendToClient(gotoPacket);
                    }
                }
                /*if (client.State.LastRealm?.Name.Contains(bestName) ?? false)
                {
                    // If the best realm is the last realm the client is connected to, send a reconnect.
                    Log("Last realm is still the best realm. Sending reconnect.");
                    ReconnectHandler.SendReconnect(connectedClient, connectedClient.State.LastRealm);
                    gotoRealm = false;
                    return;
                }*/
                else
                {
                    Log("Attempting connection.");
                    gotoRealm = false;
                    PluginUtils.Delay(550, () => AttemptConnection(client, portals.OrderBy(ptl => ptl.Location.DistanceSquaredTo(client.PlayerData.Pos)).First().ObjectId));
                }
            }
            await Task.Delay(5);
            if (gotoRealm)
            {
                MoveToRealms(client, realmChosen);
            }
            else
            {
                Log("Stopped moving to realm.");
            }
        }

        private async void AttemptConnection(Client client, int portalId)
        {
            UsePortalPacket packet = (UsePortalPacket)Packet.Create(PacketType.USEPORTAL);
            packet.ObjectId = portalId;

            if (!portals.Exists(ptl => ptl.ObjectId == portalId))
            {
                gotoRealm = true;
                MoveToRealms(connectedClient);
                return;
            }

            // Get the player count of the current portal. The packet should
            // only be sent if there is space for the player to enter.
            var pCount = portals.Find(p => p.ObjectId == portalId).PlayerCount;
            if (connectedClient.Connected && pCount < 999)
                connectedClient.SendToServer(packet);
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (connectedClient.Connected && enabled)
                AttemptConnection(connectedClient, portalId);
            else if (enabled)
                Log("Connection successful.");
            else
                Log("Bot disabled, cancelling connection attempt.");
        }

        /// <summary>
        /// Calculate which keys need to be pressed in order to move the client closer to targetPosition.
        /// </summary>
        /// <param name="client">The client who will be moved.</param>
        /// <param name="targetPosition">The target position to move towards.</param>
        /// <param name="tolerance">The distance (in game tiles) </param>
        private void CalculateMovement(Client client, Location targetPosition, float tolerance)
        {
            // Left or right
            if (client.PlayerData.Pos.X < targetPosition.X - tolerance)
            {
                // Move right
                D_PRESSED = true;
                A_PRESSED = false;
            }
            else if (client.PlayerData.Pos.X <= targetPosition.X + tolerance)
            {
                // Stop moving
                D_PRESSED = false;
            }
            if (client.PlayerData.Pos.X > targetPosition.X + tolerance)
            {
                // Move left
                A_PRESSED = true;
                D_PRESSED = false;
            }
            else if (client.PlayerData.Pos.X >= targetPosition.X - tolerance)
            {
                // Stop moving
                A_PRESSED = false;
            }

            // Up or down
            if (client.PlayerData.Pos.Y < targetPosition.Y - tolerance)
            {
                // Move down
                S_PRESSED = true;
                W_PRESSED = false;
            }
            else if (client.PlayerData.Pos.Y <= targetPosition.Y + tolerance)
            {
                // Stop moving
                S_PRESSED = false;
            }
            if (client.PlayerData.Pos.Y > targetPosition.Y + tolerance)
            {
                // Move up
                S_PRESSED = false;
                W_PRESSED = true;
            }
            else if (client.PlayerData.Pos.Y >= targetPosition.Y - tolerance)
            {
                // Stop moving
                W_PRESSED = false;
            }
        }

        public enum RealmQuests : ushort
        {
            Lich = 0x091b,
            ActualLich = 0x091c,

            EntAncient = 0x091f, //first
            ActualEntAncient = 0x0920,

            ActualGhostKing = 0x092d,
            GhostKing = 0x0928,

            CubeGod = 0x0d59,
            GobbleGod = 0x0e29,

            Pentaract = 0x0d5f,

            SkullShrine = 0x0d56,
            PumpkinShrine = 0x0e51,

            Avatar = 0x734d,

            GhostShip = 0x0e37,

            GrandSphinx = 0x0d54,

            Hermit = 0x0d61,

            JadeStatue = 0x6fcb,
            GarnetStatue = 0x6fca,

            RedBee = 0x10e5,
            BlueBee = 0x10e6,
            YellowBee = 0x10e4,

            LordOfTheLostLands = 0x0d50,

            RockDragon = 0x5e78,

            LostSentry = 0xb030,

            Biff = 0x89f,

            Bonegrind = 0x0e5c,

            PermafrostLord = 0x1fee,

            EventChest = 0x744D,
        }

        public enum BagsX : short
        {
            Soulbound = 0x0503,
            Blue = 0x050B, //5
            BlueBoosted = 0x6be,
            Brown = 0x0500, //0
            BrownBoosted = 0x6ad,
            Cyan = 0x0509, //4
            CyanBoosted = 0x6bd,
            Egg = 0x0508, //3
            EggBoosted = 0x6bb,
            Gold = 0x050E, //7
            GoldBoosted = 0x6bc,
            Orange = 0x50F, //8
            OrangeBoosted = 0x6bf,
            Pink = 0x0506, //1
            PinkBoosted = 0x6ae,
            Purple = 0x0507, //2
            PurpleBoosted = 0x6ba,
            Red = 0x6AC, //9
            RedBoosted = 0x6c0,
            White = 0x050C, //6
            WhiteBoosted = 0x0510,
        }
    }
}
