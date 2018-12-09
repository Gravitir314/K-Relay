using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.GameData.DataStructures;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;
using Mila.Properties;

namespace Mila {
 public class Mila : IPlugin {
  public string GetAuthor () {
   return " ";
  }

  public string GetName () {
   return "Mila 3.0";
  }

  public string GetDescription () {
   return "Additional credit to Killer Be Killed, Jazz, 059, Lxys, AmejiOrape, pixelzerg, apemanzilla, and krazyshank.";
  }

  public string[] GetCommands () {
   return new string[] {
    "/mila, /ps <player>, /hs config, /hs id <id>, /hs <class>, /hs custom, /hs save, /qtp, /qtp config, /ar, /sa, /sa events, /sa keys, /tp, /spn"
   };
  }

  private readonly string sewerQ1 = "What time is it?";
  private readonly string sewerQ1a = "Its pizza time!";
  private readonly string sewerQ2 = "Where is the safest place in the world?";
  private readonly string sewerQ2a = "Inside my shell.";
  private readonly string sewerQ3 = "What is fast, quiet and hidden by the night?";
  private readonly string sewerQ3a = "A ninja of course!";
  private readonly string sewerQ4 = "How do you like your pizza?";
  private readonly string sewerQ4a = "Extra cheese, hold the anchovies.";
  private readonly string sewerQ5 = "Who did this to me?";
  private readonly string sewerQ5a = "Dr. Terrible, the mad scientist.";
  private readonly string lodQuestion = "Choose the Dragon Soul you wish to commune with!";
  private readonly string CraigQuestion = "Well, before I explain how this all works, let me tell you that you can always say SKIP and we'll just get on with it. Otherwise, just wait a sec while I get everything in order.";
  private readonly string spampattern = "y.xyz|Oryx[Jj]ackpot|LIFEPOT. ORG|RPGStash|ROTMGMax|rea1m|Realmgold.com|ROTMG,ORG|RealmGold|RealmShop|Wh!te|RealmBags|-------------------|Rea[lI!]mK[i!]ngs|ORYXSH[O0]P|Rea[lI!]mStock|Rea[lI!]mPower";
  private Client connectedClient;
  private Dictionary<Location, string> _Obj = new Dictionary<Location, string> ();
  private Dictionary<Location, string> _Tiles = new Dictionary<Location, string> ();
  private Dictionary<byte, int> EmptyVaultSpaces = new Dictionary<byte, int> ();
  private Dictionary<byte, int> MadGodTokens = new Dictionary<byte, int> ();
  private Dictionary<byte, int> MossTokens = new Dictionary<byte, int> ();
  private Dictionary<byte, int> Slots = new Dictionary<byte, int> ();
  private Dictionary<byte, int> StoneTokens = new Dictionary<byte, int> ();
  private Dictionary<int, int> CurrPlayers = new Dictionary<int, int> ();
  private Dictionary<int, string> ObjToString = new Dictionary<int, string> ();
  private Dictionary<string, PlayerData> dict = new Dictionary<string, PlayerData> ();
  private Dictionary<string, int> BlockedPlayers = new Dictionary<string, int> ();
  private Dictionary<string, int> GuildBois = new Dictionary<string, int> ();
  private Dictionary<string, int> LockedPlayers = new Dictionary<string, int> ();
  private Dictionary<string, string> SessionNames = new Dictionary<string, string> ();
  private Random rnd = new Random ();
  private Stopwatch MapTime = new Stopwatch ();
  private Stopwatch TimeSinceLastCall = new Stopwatch ();
  private Stopwatch TimeSinceLastGuildInvite = new Stopwatch ();
  private Stopwatch TimeSinceLastHello = new Stopwatch ();
  private Stopwatch TimeSinceLastMapInfo = new Stopwatch ();
  private Stopwatch TimeSinceLastMsg = new Stopwatch ();
  private Stopwatch TimeSinceLastNewTick = new Stopwatch ();
  private Stopwatch TimeSinceLastTrade = new Stopwatch ();
  private Location Vault1Loc = new Location (44.5f, 70.5f);
  private bool BlockedPlayersSaved;
  private bool LockedPlayersSaved;
  private bool debugging;
  private bool _xfirstx_ = true;
  private int Argus;
  private int Basaran;
  private int ChosenCharID;
  private int CultPortalObjId;
  private int CurrentCharID;
  private int Dirge;
  private int Gaius;
  private int GuildInviteCount;
  private int JoinedPlayersSinceLastPong;
  private int LostHallsPortalObjId;
  private int LostSentry;
  private int Malus;
  private int Malus2;
  private int MapPongs;
  private int Pongs;
  private int TrackJoiningPlayers;
  private int VoidPortalObjId;
  private int currStars;
  private int fame;
  private int Vault1;
  private int[] newTick = new int[3];
  private readonly ushort Blue = Convert.ToUInt16 (BagsX.Blue);
  private readonly ushort BlueBoosted = Convert.ToUInt16 (BagsX.BlueBoosted);
  private readonly ushort Egg = Convert.ToUInt16 (BagsX.Egg);
  private readonly ushort EggBoosted = Convert.ToUInt16 (BagsX.EggBoosted);
  private readonly ushort Orange = Convert.ToUInt16 (BagsX.Orange);
  private readonly ushort OrangeBoosted = Convert.ToUInt16 (BagsX.OrangeBoosted);
  private readonly ushort Pink = Convert.ToUInt16 (BagsX.Pink);
  private readonly ushort PinkBoosted = Convert.ToUInt16 (BagsX.PinkBoosted);
  private readonly ushort Purple = Convert.ToUInt16 (BagsX.Purple);
  private readonly ushort PurpleBoosted = Convert.ToUInt16 (BagsX.PurpleBoosted);
  private readonly ushort Red = Convert.ToUInt16 (BagsX.Red);
  private readonly ushort RedBoosted = Convert.ToUInt16 (BagsX.RedBoosted);
  private readonly ushort VIAL = GameData.Items.ByName ("Vial of Pure Darkness").ID;
  private readonly ushort White = Convert.ToUInt16 (BagsX.White);
  private readonly ushort WhiteBoosted = Convert.ToUInt16 (BagsX.WhiteBoosted);
  private readonly ushort Yellow = Convert.ToUInt16 (BagsX.Gold);
  private readonly ushort YellowBoosted = Convert.ToUInt16 (BagsX.GoldBoosted);
  private string ReconName = "";
  private string ServerName;
  private string currAccountID = "";
  private string currMap;
  private string currName = "";
  private string myName = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";
  private string nextlod = "";
  private string prevMap;
  private Dictionary<string, int> Playz = new Dictionary<string, int> ();
  public int memz = 0;
  public ushort Egg1 => Egg;
  public enum RealmQuests : ushort {
   Lich = 2331,
   ActualLich,
   EntAncient = 2335,
   ActualEntAncient,
   ActualGhostKing = 2349,
   GhostKing = 2344,
   CubeGod = 3417,
   GobbleGod = 3625,
   Pentaract = 3423,
   SkullShrine = 3414,
   PumpkinShrine = 3665,
   Avatar = 29517,
   GhostShip = 3639,
   GrandSphinx = 3412,
   Hermit = 3425,
   JadeStatue = 28619,
   GarnetStatue = 28618,
   RedBee = 4325,
   BlueBee,
   YellowBee = 4324,
   LordOfTheLostLands = 3408,
   RockDragon = 24184,
   LostSentry = 45104,
   Biff = 2207,
   Bonegrind = 3676,
   PermafrostLord = 8174,
   EventChest = 29773,
   SpiritofOryx = 8170,
   MegamadOryxStatue = 8232,
   MegamadOryxBrute = 8147,
   PossessedOryxStatue = 8171
  }
  public enum BagsX : short {
   Soulbound = 1283,
   Blue = 1291,
   BlueBoosted = 1726,
   Brown = 1280,
   BrownBoosted = 1709,
   Cyan = 1289,
   CyanBoosted = 1725,
   Egg = 1288,
   EggBoosted = 1723,
   Gold = 1294,
   GoldBoosted = 1724,
   Orange = 1295,
   OrangeBoosted = 1727,
   Pink = 1286,
   PinkBoosted = 1710,
   Purple = 1287,
   PurpleBoosted = 1722,
   Red = 1708,
   RedBoosted = 1728,
   White = 1292,
   WhiteBoosted = 1296
  }
  public enum PortalsX : short {
   MadLab = 2192,
   Woodland = 1884,
   Vault = 1824
  }

  public void Initialize (Proxy proxy) {
   proxy.ClientConnected += delegate (Client client) {
    connectedClient = client;
    Vault1 = 0;
    ObjToString.Clear ();
    _Tiles.Clear ();
    _Obj.Clear ();
    MossTokens.Clear ();
    _xfirstx_ = true;
    MadGodTokens.Clear ();
    StoneTokens.Clear ();
    Playz.Clear ();
    EmptyVaultSpaces.Clear ();
    Slots.Clear ();
    GuildBois.Clear ();
    CurrPlayers.Clear ();
    Pongs = 0;
    MapPongs = 0;
    MapTime.Restart ();
   };
   TimeSinceLastGuildInvite.Start ();
   TimeSinceLastNewTick.Start ();
   TimeSinceLastCall.Start ();
   TimeSinceLastTrade.Start ();
   TimeSinceLastHello.Start ();
   TimeSinceLastMsg.Start ();
   proxy.HookCommand ("mila", new CommandHandler (OnCommand));
   proxy.HookCommand ("ps", new CommandHandler (OnCommand));
   proxy.HookCommand ("hs", new CommandHandler (OnCommand));
   proxy.HookCommand ("qtp", new CommandHandler (OnCommand));
   proxy.HookCommand ("ar", new CommandHandler (OnCommand));
   proxy.HookCommand ("sa", new CommandHandler (OnCommand));
   proxy.HookCommand ("tp", new CommandHandler (OnCommand));
   proxy.HookCommand ("getpos", new CommandHandler (OnCommand));
   proxy.HookCommand ("tile", new CommandHandler (OnCommand));
   proxy.HookCommand ("debugging", new CommandHandler (OnCommand));
   proxy.HookCommand ("obj", new CommandHandler (OnCommand));
   proxy.HookCommand ("spn", new CommandHandler (OnCommand));
   proxy.HookPacket (PacketType.MAPINFO, new PacketHandler (OnMapInfo));
   proxy.HookPacket (PacketType.CREATESUCCESS, new PacketHandler (OnCreateSuccess));
   proxy.HookPacket (PacketType.MOVE, new PacketHandler (OnMove));
   proxy.HookPacket (PacketType.UPDATE, new PacketHandler (OnUpdate));
   proxy.HookPacket (PacketType.NEWTICK, new PacketHandler (OnNewTick));
   proxy.HookPacket (PacketType.PONG, new PacketHandler (OnPong));
   proxy.HookPacket (PacketType.QUESTOBJID, new PacketHandler (OnQuestObjId));
   proxy.HookPacket (PacketType.TEXT, new PacketHandler (OnText));
   proxy.HookPacket (PacketType.PLAYERTEXT, new PacketHandler (OnPlayerText));
   proxy.HookPacket (PacketType.INVITEDTOGUILD, new PacketHandler (OnGuildInvite));
   proxy.HookPacket (PacketType.RECONNECT, new PacketHandler (OnReconnect));
   proxy.HookPacket (PacketType.HELLO, new PacketHandler (OnHello));
   proxy.HookPacket (PacketType.SERVERPLAYERSHOOT, new PacketHandler (OnServerPlayerShoot));
   proxy.HookPacket (PacketType.SHOWEFFECT, new PacketHandler (OnShowEffect));
   proxy.HookPacket (PacketType.ALLYSHOOT, new PacketHandler (OnAllyShoot));
   proxy.HookPacket (PacketType.DAMAGE, new PacketHandler (OnDamage));
   proxy.HookPacket (PacketType.CLIENTSTAT, new PacketHandler (OnClientStat));
   proxy.HookPacket (PacketType.FAILURE, new PacketHandler (OnFailure));
   proxy.HookPacket (PacketType.LOAD, new PacketHandler (OnLoad));
   proxy.HookPacket (PacketType.ACCOUNTLIST, new PacketHandler (OnAccountList));
   proxy.HookPacket (PacketType.USEITEM, new PacketHandler (OnUseItem));
   proxy.HookPacket (PacketType.INVSWAP, new PacketHandler (OnInvSwap));
   proxy.HookPacket (PacketType.TRADEREQUESTED, new PacketHandler (OnTradeRequested));
  }

  private void OnCommand (Client client, string command, string[] args) {
   if (command == "mila") {
    PluginUtils.ShowGenericSettingsGUI (MilaConfig.Default, "Mila settings");
    return;
   }
   if (command == "spn") {
    {
     if (args.Count () == 0) {
      if (memz == 0) memz = 1;
      else if (memz == 1) memz = 2;
      else if (memz == 2) memz = 0;

      if (memz == 0) client.SendToClient (PluginUtils.CreateNotification (client.ObjectId, 0xFF0000, "Not spoofing"));
      else if (memz == 1) client.SendToClient (PluginUtils.CreateNotification (client.ObjectId, 0x00FFFF, "Spoofing FROM " + MilaConfig.Default.Name));
      else if (memz == 2) client.SendToClient (PluginUtils.CreateNotification (client.ObjectId, 0x0000FF, "Spoofing TO " + MilaConfig.Default.Name));
     }
    }
   }
   if (command == "ps") {
    try {
     PlayerData playerData = dict[args[0].ToLower ()];
     PluginUtils.Log ("Mila", string.Concat (new object[] {
      "PlayerStatus ",
      playerData.Name,
      "> Realm Gold: ",
      playerData.RealmGold
     }));
     PluginUtils.Log ("Mila", "PlayerStatus " + playerData.Name + "> Account Id: " + playerData.AccountId);
     PluginUtils.Log ("Mila", string.Concat (new object[] {
      "PlayerStatus ",
      playerData.Name,
      "> Dead Fame: ",
      playerData.AccountFame
     }));
     PluginUtils.Log ("Mila", string.Concat (new object[] {
      "PlayerStatus ",
      playerData.Name,
      "> Character Fame: ",
      playerData.CharacterFame
     }));
     PluginUtils.Log ("Mila", string.Concat (new object[] {
      "PlayerStatus ",
      playerData.Name,
      "> Skin: ",
      playerData.Skin
     }));
     return;
    } catch {
     PluginUtils.Log ("Mila", "PlayerStatus Error: Player \"" + ((args.Length < 1) ? "null" : args[0]) + "\" not found!");
     return;
    }
   }
   if (command == "hs") {
    if (args.Length == 0) {
     return;
    }
    if (args[0] == "rogue" || args[0] == "r" || args[0] == "ro") {
     ChosenCharID = HotSwap.Default.Rogue;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Rogue");
      return;
     }
    } else if (args[0] == "archer" || args[0] == "ar" || args[0] == "arch") {
     ChosenCharID = HotSwap.Default.Archer;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Archer");
      return;
     }
    } else if (args[0] == "wizard" || args[0] == "wi" || args[0] == "wiz") {
     ChosenCharID = HotSwap.Default.Wizard;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Wizard");
      return;
     }
    } else if (args[0] == "samurai" || args[0] == "sam" || args[0] == "samu") {
     ChosenCharID = HotSwap.Default.Samurai;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Samurai");
      return;
     }
    } else if (args[0] == "priest" || args[0] == "pr") {
     ChosenCharID = HotSwap.Default.Priest;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Priest");
      return;
     }
    } else if (args[0] == "warrior" || args[0] == "wa" || args[0] == "warr" || args[0] == "war") {
     ChosenCharID = HotSwap.Default.Warrior;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Warrior");
      return;
     }
    } else if (args[0] == "knight" || args[0] == "k" || args[0] == "kn") {
     ChosenCharID = HotSwap.Default.Knight;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Knight");
      return;
     }
    } else if (args[0] == "paladin" || args[0] == "pala" || args[0] == "pal" || args[0] == "pa") {
     ChosenCharID = HotSwap.Default.Paladin;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Paladin");
      return;
     }
    } else if (args[0] == "assassin" || args[0] == "ass" || args[0] == "as") {
     ChosenCharID = HotSwap.Default.Assassin;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Assassin");
      return;
     }
    } else if (args[0] == "necro" || args[0] == "necromancer" || args[0] == "neg" || args[0] == "nec" || args[0] == "ne") {
     ChosenCharID = HotSwap.Default.Necromancer;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Necromancer");
      return;
     }
    } else if (args[0] == "huntress" || args[0] == "hunt" || args[0] == "h" || args[0] == "hu") {
     ChosenCharID = HotSwap.Default.Huntress;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Huntress");
      return;
     }
    } else if (args[0] == "mystic" || args[0] == "m" || args[0] == "mys" || args[0] == "my") {
     ChosenCharID = HotSwap.Default.Mystic;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Mystic");
      return;
     }
    } else if (args[0] == "trickster" || args[0] == "trick" || args[0] == "t" || args[0] == "tr") {
     ChosenCharID = HotSwap.Default.Trickster;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Trickster");
      return;
     }
    } else if (args[0] == "sorcerer" || args[0] == "sorc" || args[0] == "s" || args[0] == "so") {
     ChosenCharID = HotSwap.Default.Sorcerer;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Sorcerer");
      return;
     }
    } else if (args[0] == "ninja" || args[0] == "nin" || args[0] == "ni") {
     ChosenCharID = HotSwap.Default.Ninja;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to Ninja");
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._01_Name) {
     ChosenCharID = HotSwapCustom.Default._01_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._01_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._02_Name) {
     ChosenCharID = HotSwapCustom.Default._02_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._02_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._03_Name) {
     ChosenCharID = HotSwapCustom.Default._03_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._03_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._04_Name) {
     ChosenCharID = HotSwapCustom.Default._04_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._04_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._05_Name) {
     ChosenCharID = HotSwapCustom.Default._05_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._05_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._06_Name) {
     ChosenCharID = HotSwapCustom.Default._06_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._06_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._07_Name) {
     ChosenCharID = HotSwapCustom.Default._07_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._07_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._08_Name) {
     ChosenCharID = HotSwapCustom.Default._08_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._08_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._09_Name) {
     ChosenCharID = HotSwapCustom.Default._09_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._09_Name);
      return;
     }
    } else if (args[0] == HotSwapCustom.Default._10_Name) {
     ChosenCharID = HotSwapCustom.Default._10_ID;
     if (ChosenCharID != 0) {
      XLog ("Hotswapping to " + HotSwapCustom.Default._10_Name);
      return;
     }
    } else {
     if (args[0] == "save") {
      DoHotSwapSave (client);
      return;
     }
     if (args[0] == "config") {
      PluginUtils.ShowGenericSettingsGUI (HotSwap.Default, "Mila");
      return;
     }
     if (args[0] == "custom") {
      PluginUtils.ShowGenericSettingsGUI (HotSwapCustom.Default, "Mila");
      return;
     }
     if (args[0] == "id") {
      ChosenCharID = Convert.ToInt32 (args[1]);
      XLog ("Hotswapping to character ID " + Convert.ToInt32 (args[1]));
      return;
     }
    }
   } else
    switch (command) {
     case "getpos":
      XLog ("X = " + client.PlayerData.Pos.X);
      XLog ("Y = " + client.PlayerData.Pos.Y);
      break;
     case "debugging":
      debugging = !debugging;
      break;
     case "qtp":
      if (args.Length == 0) {
       if (QuickTP.Default.Enabled) {
        QuickTP.Default.Enabled = false;
        XLog ("QuickTP has been disabled");
       }
       QuickTP.Default.Enabled = true;
       XLog ("QuickTP has been enabled");
      }
      break;
     case "config":
      PluginUtils.ShowGenericSettingsGUI (QuickTP.Default, "Mila");
      break;
     case "ar":
      PluginUtils.ShowGenericSettingsGUI (AutoRespond.Default, "Mila");
      break;
     case "tile":
      {
       using (Dictionary<Location, string>.KeyCollection.Enumerator enumerator = _Tiles.Keys.GetEnumerator ()) {
        while (enumerator.MoveNext ()) {
        Location location = enumerator.Current;
        if ((double) client.PlayerData.Pos.DistanceTo (location) < 2.5) {
        Console.WriteLine (location + " " + _Tiles[location]);
         }
        }
       }
      }
      break;
     case "obj":
      {
       using (Dictionary<Location, string>.KeyCollection.Enumerator enumerator2 = _Obj.Keys.GetEnumerator ()) {
        while (enumerator2.MoveNext ()) {
        Location location2 = enumerator2.Current;
        if (client.PlayerData.Pos.DistanceTo (location2) < 2.5) {
        Console.WriteLine (location2 + " " + _Obj[location2]);
         }
        }
       }
      }
      break;
     case "sa":
      {
       if (args.Length == 0) {
        PluginUtils.ShowGenericSettingsGUI (SoundAlerts.Default, "Mila");
       }
       if (args[0] == "keys" || args[0] == "key") {
        PluginUtils.ShowGenericSettingsGUI (KeyAlerts.Default, "Mila");
       }
       if (args[0] == "events" || args[0] == "event") {
        PluginUtils.ShowGenericSettingsGUI (EventAlerts.Default, "Mila");
       }
      }
      break;
     case "tp":
      {
       TeleportPacket teleportPacket = (TeleportPacket) Packet.Create (PacketType.TELEPORT);
       if (args.Length == 0) {
        teleportPacket.ObjectId = client.ObjectId;
        client.SendToServer (teleportPacket);
       }
      }
      break;
     default:
      return;
    }
  }

  private void XLog (string s) {
   string str = DateTime.Now.ToString (" [hh:mm:sstt]", CultureInfo.InvariantCulture);
   PluginUtils.Log ("Mila", s + str);
   connectedClient.AnnounceYellow (s + str);
  }

  private void DoHotSwapSave (Client client) {
   if (client.Connected) {
    switch (client.PlayerData.Class) {
     case Classes.Archer:
      HotSwap.Default.Archer = CurrentCharID;
      break;
     case Classes.Assassin:
      HotSwap.Default.Assassin = CurrentCharID;
      break;
     case Classes.Huntress:
      HotSwap.Default.Huntress = CurrentCharID;
      break;
     case Classes.Knight:
      HotSwap.Default.Knight = CurrentCharID;
      break;
     case Classes.Mystic:
      HotSwap.Default.Mystic = CurrentCharID;
      break;
     case Classes.Necromancer:
      HotSwap.Default.Necromancer = CurrentCharID;
      break;
     case Classes.Ninja:
      HotSwap.Default.Ninja = CurrentCharID;
      break;
     case Classes.Paladin:
      HotSwap.Default.Paladin = CurrentCharID;
      break;
     case Classes.Priest:
      HotSwap.Default.Priest = CurrentCharID;
      break;
     case Classes.Rogue:
      HotSwap.Default.Rogue = CurrentCharID;
      break;
     case Classes.Sorcerer:
      HotSwap.Default.Sorcerer = CurrentCharID;
      break;
     case Classes.Samurai:
      HotSwap.Default.Samurai = CurrentCharID;
      break;
     case Classes.Trickster:
      HotSwap.Default.Trickster = CurrentCharID;
      break;
     case Classes.Warrior:
      HotSwap.Default.Warrior = CurrentCharID;
      break;
     case Classes.Wizard:
      HotSwap.Default.Wizard = CurrentCharID;
      break;
     default:
      return;
    }
    XLog (string.Concat (new object[] {
     "Stored ID ",
     CurrentCharID,
     " as ",
     client.PlayerData.Class
    }));
    HotSwap.Default.Save ();
   }
  }

  private void OnAccountList (Client client, Packet packet) {
   AccountListPacket accountListPacket = (AccountListPacket) packet;
   myName = client.PlayerData.Name;
   if (HotSwap.Default._AutoSave && accountListPacket.AccountListId == 0 && currMap == "Nexus") {
    DoHotSwapSave (client);
   }
   if (!BlockedPlayersSaved && accountListPacket.AccountListId == 1) {
    foreach (string key in accountListPacket.AccountIds) {
     BlockedPlayers.Add (key, accountListPacket.AccountListId);
    }
    BlockedPlayersSaved = true;
   }
   if (!LockedPlayersSaved && accountListPacket.AccountListId == 0) {
    foreach (string key2 in accountListPacket.AccountIds) {
     LockedPlayers.Add (key2, accountListPacket.AccountListId);
    }
    LockedPlayersSaved = true;
   }
  }

  private void OnCreateSuccess (Client client, Packet packet) {
   ObjToString.Clear ();
   ServerName = GameData.Servers.Map.Single ((KeyValuePair<string, ServerStructure> x) => x.Value.Address == Proxy.DefaultServer).Value.Name;
   TellStats (client);
   ChosenCharID = 0;
  }

  private void OnUseItem (Client client, Packet packet) {
   UseItemPacket useItemPacket = (UseItemPacket) packet;
  }

  private void Use (Client client, byte slotid, int type, byte usetype) {
   UseItemPacket useItemPacket = (UseItemPacket) Packet.Create (PacketType.USEITEM);
   useItemPacket.Time = connectedClient.Time;
   useItemPacket.SlotObject = new SlotObject {
    ObjectId = connectedClient.ObjectId,
    SlotId = slotid,
    ObjectType = type
   };
   useItemPacket.ItemUsePos = new Location {
    X = 0f,
    Y = 0f
   };
   useItemPacket.UseType = usetype;
   client.SendToServer (useItemPacket);
  }

  private void OnInvSwap (Client client, Packet packet) {
   InvSwapPacket invSwapPacket = (InvSwapPacket) packet;
   XLog (invSwapPacket.Position.ToString ());
   XLog (invSwapPacket.SlotObject1.ToString ());
   XLog (invSwapPacket.SlotObject2.ToString ());
   XLog (invSwapPacket.Time.ToString ());
  }

  private void OnClientStat (Client client, Packet packet) {
   ClientStatPacket clientStatPacket = (ClientStatPacket) packet;
   clientStatPacket.Name.Equals ("CultistHideoutCompleted");
   XLog (clientStatPacket.Name + " = " + clientStatPacket.Value);
  }

  private void OnPlayerText (Client client, Packet packet) {
   PlayerTextPacket playerTextPacket = (PlayerTextPacket) packet;
   if (MilaConfig.Default.CommandGuard && (playerTextPacket.Text.StartsWith ("g ") || playerTextPacket.Text.StartsWith (" "))) {
    playerTextPacket.Send = false;
   }
   if (AutoRespond.Default.FastLOD && currMap == "Lair of Draconis" && (playerTextPacket.Text.Equals ("Black", StringComparison.CurrentCultureIgnoreCase) || playerTextPacket.Text.Equals ("Red", StringComparison.CurrentCultureIgnoreCase) || playerTextPacket.Text.Equals ("Green", StringComparison.CurrentCultureIgnoreCase) || playerTextPacket.Text.Equals ("Blue", StringComparison.CurrentCultureIgnoreCase))) {
    nextlod = playerTextPacket.Text.ToLower ();
    XLog ("Next LOD set to " + nextlod);
    playerTextPacket.Send = false;
    return;
   }
   if (MilaConfig.Default.CommandGuard && playerTextPacket.Text.StartsWith ("/")) {
    if (memz == 1) {
     playerTextPacket.Send = false;
     if (!playerTextPacket.Text.StartsWith ("/")) {
      int objid = -1;
      if (Playz.ContainsKey (MilaConfig.Default.Name)) {
       objid = Playz[MilaConfig.Default.Name];
      }
      TextPacket tpacket = (TextPacket) Packet.Create (PacketType.TEXT);
      tpacket.BubbleTime = 10;
      tpacket.CleanText = "";
      tpacket.Name = MilaConfig.Default.Name;
      tpacket.NumStars = MilaConfig.Default.Stars;
      tpacket.ObjectId = objid;
      tpacket.Recipient = client.PlayerData.Name;
      tpacket.Text = playerTextPacket.Text;
      tpacket.isSupporter = false;
      client.SendToClient (tpacket);
     }
    } else if (memz == 2) {
     playerTextPacket.Send = false;

     if (!playerTextPacket.Text.StartsWith ("/")) {
      TextPacket tpacket = (TextPacket) Packet.Create (PacketType.TEXT);
      tpacket.BubbleTime = 10;
      tpacket.CleanText = "";
      tpacket.Name = client.PlayerData.Name;
      tpacket.NumStars = client.PlayerData.Stars;
      tpacket.ObjectId = client.ObjectId;
      tpacket.Recipient = MilaConfig.Default.Name;
      tpacket.Text = playerTextPacket.Text;
      tpacket.isSupporter = false;
      client.SendToClient (tpacket);
     }
    }
    if (playerTextPacket.Text.StartsWith ("/pause")) {
     playerTextPacket.Text = "/pause";
     return;
    }
    if (playerTextPacket.Text.StartsWith ("/who")) {
     playerTextPacket.Text = "/who";
     return;
    }
    if (playerTextPacket.Text.StartsWith ("/tutorial")) {
     playerTextPacket.Text = "/tutorial";
     return;
    }
    if (playerTextPacket.Text.StartsWith ("/nexustutorial")) {
     playerTextPacket.Text = "/nexustutorial";
     return;
    }
    if (playerTextPacket.Text.StartsWith ("/event")) {
     playerTextPacket.Text = "/event";
     return;
    }
    if (playerTextPacket.Text.StartsWith ("/server")) {
     playerTextPacket.Text = "/server";
     return;
    }
    if (!(playerTextPacket.Text == "/mila") && !(playerTextPacket.Text == "/ar") && !playerTextPacket.Text.StartsWith ("/hs") && !playerTextPacket.Text.StartsWith ("/qtp") && !playerTextPacket.Text.StartsWith ("/sa") && !playerTextPacket.Text.StartsWith ("/tell ") && !playerTextPacket.Text.StartsWith ("/unlock ") && !playerTextPacket.Text.StartsWith ("/g ") && !playerTextPacket.Text.StartsWith ("/t ") && !playerTextPacket.Text.StartsWith ("/yell ") && !playerTextPacket.Text.StartsWith ("/guild ") && !playerTextPacket.Text.StartsWith ("/join ") && !playerTextPacket.Text.StartsWith ("/ignore ") && !playerTextPacket.Text.StartsWith ("/unignore ") && !playerTextPacket.Text.StartsWith ("/lock ") && !playerTextPacket.Text.StartsWith ("/trade ") && !playerTextPacket.Text.StartsWith ("/invite ") && !playerTextPacket.Text.StartsWith ("/teleport ") && !playerTextPacket.Text.StartsWith ("/tp ")) {
     Console.WriteLine (playerTextPacket.Text);
     playerTextPacket.Send = false;
    }
   }
  }

  private void OnLoad (Client client, Packet packet) {
   LoadPacket loadPacket = (LoadPacket) packet;
   if (ChosenCharID != 0) {
    loadPacket.CharacterId = ChosenCharID;
   }
   CurrentCharID = loadPacket.CharacterId;
   if (MilaConfig.Default.HS_Enabled) {
    if (currMap == MilaConfig.Default.HS_Map || currMap == MilaConfig.Default.HS_Map2 || currMap == MilaConfig.Default.HS_Map3 || currMap == MilaConfig.Default.HS_Map4 || currMap == MilaConfig.Default.HS_Map5) {
     loadPacket.CharacterId = MilaConfig.Default.HS_CharID;
     return;
    }
    loadPacket.CharacterId = MilaConfig.Default.HS_Normal;
   }
  }

  private void OnPong (Client client, Packet packet) {
   PlayerTextPacket playerTextPacket = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
   Pongs++;
   MapPongs++;
   if (Pongs >= 4 && MilaConfig.Default.USW2_Sell && MilaConfig.Default.USW2_Sell_Msg != "" && currMap == "Nexus" && ServerName == "USWest2" && MapTime.ElapsedMilliseconds > 5000L) {
    playerTextPacket.Text = MilaConfig.Default.USW2_Sell_Msg;
    connectedClient.SendToServer (playerTextPacket);
    Pongs = 0;
   }
   int num = TrackJoiningPlayers - JoinedPlayersSinceLastPong;
   if (num >= MilaConfig.Default.NotifyNexusPlayerWave && TimeSinceLastMapInfo.ElapsedMilliseconds > 5000L && currMap == "Nexus") {
    XLog (num + " players joined within one second");
    PlayAlert (Resources.Alert);
   }
   JoinedPlayersSinceLastPong = TrackJoiningPlayers;
  }

  private void Swap (Client client, int obj1id, byte slot1id, int obj1type, int obj2id, byte slot2id, int obj2type) {
   InvSwapPacket invSwapPacket = (InvSwapPacket) Packet.Create (PacketType.INVSWAP);
   invSwapPacket.Time = client.Time;
   invSwapPacket.Position = client.PlayerData.Pos;
   invSwapPacket.SlotObject1 = new SlotObject {
    ObjectId = obj1id,
    SlotId = slot1id,
    ObjectType = obj1type
   };
   invSwapPacket.SlotObject2 = new SlotObject {
    ObjectId = obj2id,
    SlotId = slot2id,
    ObjectType = obj2type
   };
   client.SendToServer (invSwapPacket);
  }

  private void Drop (Client client, byte slotid, int objtype) {
   InvDropPacket invDropPacket = (InvDropPacket) Packet.Create (PacketType.INVDROP);
   invDropPacket.Slot = new SlotObject {
    ObjectId = connectedClient.ObjectId,
    SlotId = slotid,
    ObjectType = objtype
   };
   client.SendToServer (invDropPacket);
  }

  public static void Log (string message) {
   string str = DateTime.Now.ToString ("MM/dd/yyyy hh:mm:sstt");
   using (StreamWriter streamWriter = File.AppendText ("chat.txt")) {
    streamWriter.WriteLine (str + " " + message);
   }
  }

  private void OnGuildInvite (Client client, Packet packet) {
   InvitedToGuildPacket invitedToGuildPacket = (InvitedToGuildPacket) packet;
   if (MilaConfig.Default.BlockGuildInvites) {
    invitedToGuildPacket.Send = false;
   }
   if (TimeSinceLastGuildInvite.ElapsedMilliseconds < 1000L) {
    invitedToGuildPacket.Send = false;
   }
   if (TimeSinceLastGuildInvite.ElapsedMilliseconds > 1000L) {
    Mila.Log (invitedToGuildPacket.Name + " has invited you to join the guild \"" + invitedToGuildPacket.GuildName + "\"");
    GuildInviteCount = 0;
    TimeSinceLastGuildInvite.Restart ();
    if (MilaConfig.Default.BlockGuildInvites) {
     XLog (invitedToGuildPacket.Name + " has invited you to join the guild \"" + invitedToGuildPacket.GuildName + "\"");
     return;
    }
   } else {
    GuildInviteCount++;
    if (GuildInviteCount == 10 || GuildInviteCount == 20 || GuildInviteCount == 30) {
     XLog (invitedToGuildPacket.Name + " has potentially attempted to guild invite crash you. count = " + GuildInviteCount);
     Mila.Log (invitedToGuildPacket.Name + " has potentially attempted to guild invite crash you. count = " + GuildInviteCount);
    }
   }
  }

  private void OnFailure (Client client, Packet packet) {
   FailurePacket failurePacket = (FailurePacket) packet;
   PluginUtils.Log ("Mila", string.Concat (new object[] {
    "<FAILURE> ErrorId: ",
    failurePacket.ErrorId,
    " ErrorMessage: ",
    failurePacket.ErrorMessage
   }));
   failurePacket.ErrorMessage.Contains ("dead");
  }

  private void OnText (Client client, Packet packet) {
   TextPacket p = (TextPacket) packet;
   PlayerTextPacket p2 = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
   string text = p.Text.ToLower ();
   if (p.NumStars == -1) {
    foreach (var name in p.Name)
     if (p.Name == "#Mystery Box Shop") {
      p.Send = false;
     }
    else if (p.Name.StartsWith ("#")) {
     p.Send = false;
     if (MilaConfig.Default.zDebug) {
      PluginUtils.Log ("Mila", p.Name + ": " + p.Text);
     }
    } else if (p.Text.StartsWith ("Current number of the remaining enemies") && !p.Text.EndsWith ("1.") && !p.Text.EndsWith ("2.") && !p.Text.EndsWith ("3.")) {
     p.Send = false;
    }
    if (SoundAlerts.Default.Boss && (p.Text == "Innocent souls. So delicious. You have sated me. Now come, I shall give you your reward." || p.Text == "Ha Ha, you are too late. Lord Xil will soon arrive in this realm." || p.Text == "Welcome to the Final Act, my friends. My puppets require life essence in order to continue performing...")) {
     PlayAlert (Resources.Boss);
    }
    if (SoundAlerts.Default.Troom && (p.Text == "The door to Daichi's private study is opened." || p.Text == "Am I not an uncanny likeness of Oryx himself?")) {
     PlayAlert (Resources.Treasure);
    }
    if (AutoRespond.Default.Cemetary && text.Contains ("say") && text.Contains ("ready")) {
     p2.Text = "ready";
     client.SendToServer (p2);
     return;
    }
    if (AutoRespond.Default.OceanTrench && text.Contains ("king") && text.Contains ("alexander")) {
     p2.Text = "He lives and reigns and conquers the world";
     client.SendToServer (p2);
     return;
    }
    if (AutoRespond.Default.Craig && p.Text == CraigQuestion) {
     p2.Text = "skip";
     client.SendToServer (p2);
     return;
    }
    if (AutoRespond.Default.FastLOD && p.Text == lodQuestion) {
     if (nextlod != "") {
      p2.Text = nextlod;
      client.SendToServer (p2);
      nextlod = "";
      return;
     }
    } else if (p.Text.Contains ("server.dungeon_opened_by")) {
     if (KeyAlerts.Default.Abyss && p.Text.Contains ("Abyss of")) {
      PlayAlert (Resources.abyss);
     }
     if (KeyAlerts.Default.BattleNexus && p.Text.Contains ("Battle Nexus")) {
      PlayAlert (Resources.battle);
     }
     if (KeyAlerts.Default.Candyland && p.Text.Contains ("Candyland Hunting")) {
      PlayAlert (Resources.candyland);
     }
     if (KeyAlerts.Default.Cemetary && p.Text.Contains ("Haunted Cemetary") && !p.Text.Contains ("Haunted")) {
      PlayAlert (Resources.candyland);
     }
     if (KeyAlerts.Default.CrawlingDepths && p.Text.Contains ("Crawling Depths")) {
      PlayAlert (Resources.crawling_depths);
     }
     if (KeyAlerts.Default.DavysLocker && p.Text.Contains ("s Locker")) {
      PlayAlert (Resources.davys);
     }
     if (KeyAlerts.Default.DeadwaterDocks && p.Text.Contains ("Deadwater Docks")) {
      PlayAlert (Resources.docks);
     }
     if (KeyAlerts.Default.Encore && p.Text.Contains ("s Encore")) {
      PlayAlert (Resources.Encore);
     }
     if (KeyAlerts.Default.IceCave && p.Text.Contains ("Ice Cave")) {
      PlayAlert (Resources.icecave);
     }
     if (KeyAlerts.Default.IceTomb && p.Text.Contains ("Ice Tomb")) {
      PlayAlert (Resources.icetomb);
     }
     if (KeyAlerts.Default.LoD && p.Text.Contains ("Lair of Drac")) {
      PlayAlert (Resources.lod);
     }
     if (KeyAlerts.Default.LostHalls && p.Text.Contains ("Lost Halls")) {
      PlayAlert (Resources.LH);
     }
     if (KeyAlerts.Default.MadLab && p.Text.Contains ("Mad Lab")) {
      PlayAlert (Resources.madlab);
     }
     if (KeyAlerts.Default.MagicWoods && p.Text.Contains ("Magic Woods")) {
      PlayAlert (Resources.mwoods);
     }
     if (KeyAlerts.Default.Manor && p.Text.Contains ("Manor of")) {
      PlayAlert (Resources.manor);
     }
     if (KeyAlerts.Default.MountainTemple && p.Text.Contains ("Mountain Temple")) {
      PlayAlert (Resources.Temple);
     }
     if (KeyAlerts.Default.Nest && p.Text.Contains ("The Nest")) {
      PlayAlert (Resources.nest);
     }
     if (KeyAlerts.Default.OceanTrench && p.Text.Contains ("Ocean Trench")) {
      PlayAlert (Resources.OT);
     }
     if (KeyAlerts.Default.MadGodMayhem && p.Text.Contains ("Mad God Mayhem")) {
      PlayAlert (Resources.Mayhem);
     }
     if (KeyAlerts.Default.ParasiteChambers && p.Text.Contains ("Parasite Chambers")) {
      PlayAlert (Resources.parasite_chambers);
     }
     if (KeyAlerts.Default.Sewers && p.Text.Contains ("Toxic Sewers")) {
      PlayAlert (Resources.sewers);
     }
     if (KeyAlerts.Default.Shaitan && p.Text.Contains ("Shaitan's Portal")) {
      PlayAlert (Resources.shaitan);
     }
     if (KeyAlerts.Default.Shatters && p.Text.Contains ("The Shatters")) {
      PlayAlert (Resources.shatters);
     }
     if (KeyAlerts.Default.Theatre && p.Text.Contains ("s Theatre")) {
      PlayAlert (Resources.theatre);
     }
     if (KeyAlerts.Default.Tomb && p.Text.Contains ("Tomb of the Ancients")) {
      PlayAlert (Resources.tomb);
     }
     if (KeyAlerts.Default.UDL && p.Text.Contains ("Undead Lair")) {
      PlayAlert (Resources.udl);
     }
     if (KeyAlerts.Default.Reef && p.Text.Contains ("Cnidarian Reef")) {
      PlayAlert (Resources.Reef);
     }
     if (KeyAlerts.Default.Woodland && p.Text.Contains ("Woodland Lab")) {
      PlayAlert (Resources.woodland);
      return;
     }
    } else if (p.Text.Contains ("server.dungeon_unlocked_by")) {
     if (MilaConfig.Default.zDebug) {
      XLog ("Name: " + p.Name);
     }
     if (MilaConfig.Default.zDebug) {
      XLog ("Text: " + p.Text);
     }
     if (SoundAlerts.Default.UnlockedCellar && p.Text.Contains ("Wine Cellar")) {
      PlayAlert (Resources.Cellar);
      return;
     }
    } else {
     if (SoundAlerts.Default.RealmEvent && p.Text == "..." && currMap == "Realm of the Mad God") {
      PlayAlert (Resources.Event);
      return;
     }
     if (SoundAlerts.Default.ChestSpawned && p.Text == "The Reward Chest has spawned! It will be Invulnerable for 15 seconds.") {
      PlayAlert (Resources.Chest);
      return;
     }
     if (AutoRespond.Default.Sewers && (p.Text == sewerQ1 || p.Text == sewerQ2 || p.Text == sewerQ3 || p.Text == sewerQ4 || p.Text == sewerQ5)) {
      if (p.Text == sewerQ1) {
       p2.Text = sewerQ1a;
      }
      if (p.Text == sewerQ2) {
       p2.Text = sewerQ2a;
      }
      if (p.Text == sewerQ3) {
       p2.Text = sewerQ3a;
      }
      if (p.Text == sewerQ4) {
       p2.Text = sewerQ4a;
      }
      if (p.Text == sewerQ5) {
       p2.Text = sewerQ5a;
      }
      client.SendToServer (p2);
      return;
     }
    }
   } else {
    if ((Regex.IsMatch (p.Text, spampattern, RegexOptions.IgnoreCase) || p.Text == " ") && MilaConfig.Default.BlockSpam) {
     p.Send = false;
     return;
    }
    if (ServerName == "USWest2" && currMap == "Nexus" && MilaConfig.Default.USW2_Filter_Msg && p.Name != client.PlayerData.Name && p.Recipient != client.PlayerData.Name && p.Recipient != "*Guild*" && !text.Contains (MilaConfig.Default.USW2_Filter_Msg1) && !text.Contains (MilaConfig.Default.USW2_Filter_Msg2) && !text.Contains (MilaConfig.Default.USW2_Filter_Msg3)) {
     p.Send = false;
    }
    if (SoundAlerts.Default.PrivateMessage && p.Recipient == myName) {
     PlayAlert (Resources.Message);
    }
    if (p.Recipient == "*Guild*") {
     if (MilaConfig.Default.LogGuildChat) {
      Mila.Log ("<" + p.Name + "-GUILD> " + p.Text);
     }
     if (MilaConfig.Default.BlockGuildChat) {
      p.Send = false;
     }
    }
    if (MilaConfig.Default.LogPrivateMessages && client.PlayerData != null) {
     if (p.Recipient == client.PlayerData.Name) {
      Mila.Log ("<" + p.Name + "-PM> " + p.Text);
      if (!SessionNames.ContainsKey (p.Name) && MilaConfig.Default.AutoRespondToMessages && TimeSinceLastMsg.ElapsedMilliseconds > 5000L) {
       TimeSinceLastMsg.Restart ();
       SessionNames.Add (p.Name, "_nouse_");
       PluginUtils.Delay (MilaConfig.Default.AutoRespondToMessagesAfter, delegate {
        p2.Text = "/tell " + p.Name + " " + MilaConfig.Default.AutoRespondToMessagesMsg;
       });
       PluginUtils.Delay (MilaConfig.Default.AutoRespondToMessagesAfter + 50, delegate {
        connectedClient.SendToServer (p2);
       });
      }
     } else if (p.Recipient != "" && p.Recipient != "*Guild*") {
      Mila.Log ("<" + p.Recipient + "-SENT> " + p.Text);
      if (!SessionNames.ContainsKey (p.Name) && MilaConfig.Default.AutoRespondToMessages) {
       SessionNames.Add (p.Name, "_nouse_");
      }
     }
    }
    if (MilaConfig.Default.LogChat && p.Name != client.PlayerData.Name && p.Recipient != client.PlayerData.Name) {
     Mila.Log ("<" + p.Name + "-CHAT> " + p.Text);
    }
    if (MilaConfig.Default.BlockPrivateMessages && p.NumStars != -1 && p.Name != client.PlayerData.Name && p.Recipient == client.PlayerData.Name) {
     p.Send = false;
    }
    if (QuickTP.Default.Enabled && (text == QuickTP.Default.Phrase01 || text == QuickTP.Default.Phrase02 || text == QuickTP.Default.Phrase03 || text == QuickTP.Default.Phrase04 || text == QuickTP.Default.Phrase05 || text == QuickTP.Default.Phrase06 || text == QuickTP.Default.Phrase07 || text == QuickTP.Default.Phrase08 || text == QuickTP.Default.Phrase09 || text == QuickTP.Default.Phrase10 || text == QuickTP.Default.Phrase11 || text == QuickTP.Default.Phrase12 || text == QuickTP.Default.Phrase13 || text == QuickTP.Default.Phrase14 || text == QuickTP.Default.Phrase15) && TimeSinceLastCall.ElapsedMilliseconds > (long) (QuickTP.Default.Delay + 1000)) {
     TimeSinceLastCall.Restart ();
     PlayAlert (Resources.Alert);
     XLog ("Quickly teleporting to " + p.Name + ". Matching phrase: " + text);
     PluginUtils.Delay (QuickTP.Default.Delay + 125, delegate {
      p2.Text = "/tp " + p.Name;
     });
     PluginUtils.Delay (QuickTP.Default.Delay + 250, delegate {
      client.SendToServer (p2);
     });
    }
   }
  }

  private void TellStats (Client client) {
   string text = TimeSinceLastMapInfo.Elapsed.ToString ("hh\\:mm\\:ss");
   if (prevMap != "" && text != "00:00:00") {
    PluginUtils.Log ("Mila", "You were inside " + prevMap + " for " + text);
   }
   TimeSinceLastMapInfo.Restart ();
  }

  private void OnMapInfo (Client client, Packet packet) {
   MapInfoPacket mapInfoPacket = packet as MapInfoPacket;
   prevMap = currMap;
   currMap = mapInfoPacket.Name;
   if (SoundAlerts.Default.NexusAlert && mapInfoPacket.Name == "Nexus") {
    PlayAlert (Resources.Nexus);
   }
   if (MilaConfig.Default.NexusPause && mapInfoPacket.Name == "Nexus") {
    PlayerTextPacket p2 = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
    p2.Text = "/pause";
    PluginUtils.Delay (8000, delegate {
     client.SendToServer (p2);
    });
   }
   TrackJoiningPlayers = 0;
  }

  private void OnUpdate (Client client, Packet packet) {
   UpdatePacket updatePacket = (UpdatePacket) packet;
   PlayerTextPacket playerTextPacket = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
   foreach (Tile tile in updatePacket.Tiles) {
    Location key = new Location ((float) tile.X, (float) tile.Y);
    if (!_Tiles.ContainsKey (key)) {
     _Tiles.Add (key, tile.Type + " " + GameData.Tiles.ByID (tile.Type).Name);
    }
   }
   foreach (Entity entity in updatePacket.NewObjs) {
    if (Enum.IsDefined (typeof (Classes), (short) entity.ObjectType)) {
     foreach (StatData statData in entity.Status.Data) {
      if (statData.Id == StatsType.Name) {
       if (!Playz.ContainsKey (statData.StringValue)) {
        Playz.Add (statData.StringValue, entity.Status.ObjectId);
       }
      }
     }
    }
    if (entity.ObjectType == 45217) {
     Argus = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45219) {
     Gaius = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45221) {
     Basaran = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45223) {
     Dirge = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45215) {
     Malus = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45231) {
     Malus2 = entity.Status.ObjectId;
     XLog ("Malus found");
     PlayAlert (Resources.Malus);
    }
    if (entity.ObjectType == 45072 && !ObjToString.ContainsKey (entity.Status.ObjectId)) {
     ObjToString.Add (entity.Status.ObjectId, "_nouse_");
     XLog ("Agonized Titan found");
     PlayAlert (Resources.Treasure);
    }
    if (entity.ObjectType == 45075) {
     XLog ("Void opened");
     PlayAlert (Resources.Void);
     VoidPortalObjId = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45073 && !ObjToString.ContainsKey (entity.Status.ObjectId)) {
     ObjToString.Add (entity.Status.ObjectId, "_nouse_");
     XLog ("Marble Colossus found");
     PlayAlert (Resources.Colossus);
    }
    if (entity.ObjectType == 45092) {
     LostHallsPortalObjId = entity.Status.ObjectId;
    }
    if (entity.ObjectType == 45155) {
     CultPortalObjId = entity.Status.ObjectId;
     if (!ObjToString.ContainsKey (entity.Status.ObjectId)) {
      ObjToString.Add (entity.Status.ObjectId, "_nouse_");
      XLog ("Cultist Hideout opened");
      PlayAlert (Resources.Cultist);
     }
    }
    if (Enum.IsDefined (typeof (Mila.RealmQuests), entity.ObjectType)) {
     if (MilaConfig.Default.NotifyRealmQuests) {
      XLog ("QUEST: " + Enum.GetName (typeof (Mila.RealmQuests), entity.ObjectType));
     }
     if (EventAlerts.Default.Avatar && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.Avatar)) {
      PlayAlert (Resources.Avatar);
     } else if (EventAlerts.Default.CubeGod && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.CubeGod)) {
      PlayAlert (Resources.Cube);
     } else if (EventAlerts.Default.Ent && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.EntAncient)) {
      PlayAlert (Resources.Ent);
     } else if (EventAlerts.Default.RockDragon && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.RockDragon)) {
      PlayAlert (Resources.Dragon);
     } else if (EventAlerts.Default.GhostShip && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.GhostShip)) {
      PlayAlert (Resources.Ghost);
     } else if (EventAlerts.Default.Hermit && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.Hermit)) {
      PlayAlert (Resources.Hermit);
     } else if (EventAlerts.Default.JadeStatues && (entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.JadeStatue) || entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.GarnetStatue))) {
      PlayAlert (Resources.Statues);
     } else if (EventAlerts.Default.LordoftheLostLands && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.LordOfTheLostLands)) {
      PlayAlert (Resources.Lord);
     } else if (EventAlerts.Default.Pentaract && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.Pentaract)) {
      PlayAlert (Resources.Pentaract);
     } else if (EventAlerts.Default.LostSentry && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.LostSentry)) {
      PlayAlert (Resources.Sentry);
     } else if (EventAlerts.Default.SkullShrine && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.SkullShrine)) {
      PlayAlert (Resources.Skull);
     } else if (EventAlerts.Default.GrandSphinx && entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.GrandSphinx)) {
      PlayAlert (Resources.Sphinx);
     }
     if (entity.ObjectType == Convert.ToUInt16 (Mila.RealmQuests.LostSentry)) {
      LostSentry = entity.Status.ObjectId;
     }
    }
    if (Enum.IsDefined (typeof (Mila.BagsX), (short) entity.ObjectType) && !ObjToString.ContainsKey (entity.Status.ObjectId)) {
     if (entity.ObjectType == White || entity.ObjectType == WhiteBoosted) {
      if (MilaConfig.Default.NotifyWhiteBag) {
       PlayAlert (Resources.White);
      }
      XLog ("White bag");
      DoBagScan (client, entity);
     } else if ((entity.ObjectType == Orange || entity.ObjectType == OrangeBoosted) && MilaConfig.Default.NotifyOrangeBag) {
      XLog ("Orange bag");
      PlayAlert (Resources.Orange);
      DoBagScan (client, entity);
     } else if ((entity.ObjectType == Red || entity.ObjectType == RedBoosted) && MilaConfig.Default.NotifyRedBag) {
      XLog ("Red bag");
      PlayAlert (Resources.Red);
      DoBagScan (client, entity);
     } else if ((entity.ObjectType == Egg1 || entity.ObjectType == EggBoosted) && SoundAlerts.Default.EggAlert) {
      PlayAlert (Resources.Egg);
      XLog ("Egg bag");
      DoBagScan (client, entity);
     }
     ObjToString.Add (entity.Status.ObjectId, "_nouse_");
    }
    if (Enum.IsDefined (typeof (Classes), (short) entity.ObjectType)) {
     if (!CurrPlayers.ContainsKey (entity.Status.ObjectId)) {
      CurrPlayers.Add (entity.Status.ObjectId, entity.Status.ObjectId);
     }
     TrackJoiningPlayers++;
     foreach (StatData statData in entity.Status.Data) {
      bool flag = false;
      if (statData.Id == StatsType.Size && entity.Status.ObjectId != client.ObjectId) {
       statData.IntValue = MilaConfig.Default.ResizePlayers;
      }
      if ((statData.Id >= 12 && statData.Id <= 19) || (statData.Id >= 71 && statData.Id <= 78 && entity.Status.ObjectId == connectedClient.ObjectId)) {
       int num = 8;
       if (statData.Id >= 71 && statData.Id <= 78) {
        num = 59;
       }
       if (statData.IntValue != -1) {
        string name = GameData.Items.ByID ((ushort) statData.IntValue).Name;
        byte key2 = (byte) (statData.Id - num);
        int intValue = statData.IntValue;
        if (!Slots.ContainsKey (key2)) {
         Slots.Add (key2, (int) ((short) intValue));
        }
       }
       if (statData.Id == StatsType.Backpack7 && statData.IntValue != -1) {
        PlayAlert (Resources.Alert);
       }
      }
      if (statData.Id == StatsType.Stars) {
       currStars = statData.IntValue;
      }
      bool flag2 = false;
      if (statData.Id == StatsType.Name) {
       currName = statData.StringValue;
       if (currMap != "Nexus" && currMap != "Realm of the Mad God" && !ObjToString.ContainsKey (entity.Status.ObjectId)) {
        ObjToString.Add (entity.Status.ObjectId, statData.StringValue);
       }
       if (currName == "Alky" || currName == "Krathan" || currName == "Myzzrym" || currName == "RndOmSXD") {
        XLog ("Admin found: " + currName);
       }
      }
      if (statData.Id == StatsType.AccountId) {
       currAccountID = statData.StringValue;
       if (flag2) {
        Mila.Log (currName + " = " + currAccountID);
       }
       if (flag2) {
        XLog (currName + " = " + currAccountID);
       }
      }
      if (statData.Id == StatsType.CharacterFame) {
       fame = statData.IntValue;
      }
      if (statData.Id == StatsType.Glowing && entity.Status.ObjectId == client.ObjectId && MilaConfig.Default.RedGlow) {
       statData.IntValue = 999;
      }
      if (statData.Id == StatsType.GuildName) {
       if (statData.StringValue.Length == 0 && currStars >= MilaConfig.Default.ShowGuildless) {
        Console.WriteLine ("/invite " + currName);
       } else if (statData.StringValue == client.PlayerData.GuildName) {
        flag = true;
       }
      }
      if (statData.Id == StatsType.GuildName && entity.Status.ObjectId == client.ObjectId && MilaConfig.Default.GuildNameToServerName) {
       statData.StringValue = ServerName;
      }
      if (statData.Id == StatsType.SupporterPoints && statData.IntValue != 0) {
       Console.WriteLine (currName + " supporter points: " + statData.IntValue);
      }
      if (MilaConfig.Default.GuildNameToServerName && statData.Id == StatsType.GuildRank && entity.Status.ObjectId == client.ObjectId) {
       statData.IntValue = 40;
      }
      if (BlockedPlayers.ContainsKey (currAccountID) && MilaConfig.Default.UnblockEveryone && !flag) {
       XLog (string.Concat (new object[] {
        "Found ",
        currName,
        " on your block list. Unblocking... Count = ",
        BlockedPlayers.Count
       }));
       playerTextPacket.Text = "/unignore " + currName;
       client.SendToServer (playerTextPacket);
       BlockedPlayers.Remove (statData.StringValue);
      }
      if (LockedPlayers.ContainsKey (currAccountID) && MilaConfig.Default.UnlockEveryone && !flag) {
       XLog (string.Concat (new object[] {
        "Found ",
        currName,
        " on your lock list. Unlocking... Count = ",
        LockedPlayers.Count
       }));
       playerTextPacket.Text = "/unlock " + currName;
       client.SendToServer (playerTextPacket);
       LockedPlayers.Remove (statData.StringValue);
      }
      if (flag && !LockedPlayers.ContainsKey (currAccountID) && MilaConfig.Default.LockGuildies) {
       playerTextPacket.Text = "/lock " + currName;
       connectedClient.SendToServer (playerTextPacket);
       if (!GuildBois.ContainsKey (currName)) {
        GuildBois.Add (currName, entity.Status.ObjectId);
       }
      }
      PlayerData playerData = new PlayerData (entity.Status.ObjectId) {
       Class = (Classes) entity.ObjectType
      };
      foreach (StatData statData2 in entity.Status.Data) {
       playerData.Parse (statData2.Id, statData2.IntValue, statData2.StringValue);
      }
      if (!dict.ContainsKey (playerData.Name.ToLower ())) {
       dict.Add (playerData.Name.ToLower (), playerData);
      }
     }
    } else {
     Location key3 = new Location (entity.Status.Position.X, entity.Status.Position.Y);
     if (!_Obj.ContainsKey (key3)) {
      _Obj.Add (key3, entity.ObjectType + " " + GameData.Objects.ByID (entity.ObjectType).Name);
     }
    }
   }
   foreach (int key4 in updatePacket.Drops) {
    if (CurrPlayers.ContainsKey (key4)) {
     CurrPlayers.Remove (key4);
    }
    if (ObjToString.ContainsKey (key4) && ObjToString[key4] != "_nouse_" && MilaConfig.Default.PlayerLeftNotify) {
     XLog (ObjToString[key4] + " has left.");
     ObjToString.Remove (key4);
    }
   }
  }

  private void DoBagScan (Client client, Entity entity) {
   foreach (StatData statData in entity.Status.Data) {
    if (statData.Id >= 8 && statData.Id <= 19 && statData.IntValue != -1) {
     if ((GameData.Items.ByID ((ushort) statData.IntValue).Name.Contains ("Rare") || GameData.Items.ByID ((ushort) statData.IntValue).Name.Contains ("Uncommon")) && GameData.Items.ByID ((ushort) statData.IntValue).Name.Contains ("Egg")) {
      PlayAlert (Resources.Egg);
      XLog (GameData.Items.ByID ((ushort) statData.IntValue).Name);
     }
     if (GameData.Items.ByID ((ushort) statData.IntValue).Soulbound) {
      if (!GameData.Items.ByID ((ushort) statData.IntValue).Name.Contains ("Potion")) {
       XLog (GameData.Items.ByID ((ushort) statData.IntValue).Name);
       PluginUtils.Log ("Mila", GameData.Items.ByID ((ushort) statData.IntValue).Name);
      }
      if (GameData.Items.ByID ((ushort) statData.IntValue).Name.Contains ("Life Potion")) {
       XLog (GameData.Items.ByID ((ushort) statData.IntValue).Name);
      }
      if (GameData.Items.ByID ((ushort) statData.IntValue).Name.Contains ("Effusion")) {
       PlayAlert (Resources.Effusion);
      }
     }
    }
   }
  }

  private void OnMove (Client client, Packet packet) {
   MovePacket movePacket = (MovePacket) packet;
  }

  private void OnNewTick (Client client, Packet packet) {
   NewTickPacket newTickPacket = (NewTickPacket) packet;
   if (newTick[0] != 15) {
    newTick[0]++;
    newTick[1] = newTick[1] + newTickPacket.TickTime;
   } else {
    int num = newTick[1] - 3000;
    newTick[0] = 0;
    newTick[1] = 0;
    newTick[2] = num;
   }
   if (newTickPacket.TickTime >= MilaConfig.Default.NotifyLag && TimeSinceLastHello.ElapsedMilliseconds > 3000L) {
    int num2 = newTickPacket.TickTime - 200;
    client.AnnounceOrange ("Lag! +" + num2);
   }
   if (TimeSinceLastNewTick.ElapsedMilliseconds >= (long) MilaConfig.Default.NotifyLag && TimeSinceLastHello.ElapsedMilliseconds > 3000L) {
    long num3 = TimeSinceLastNewTick.ElapsedMilliseconds - 200L;
    client.AnnounceOrange ("Lag!! +" + num3);
   }
   TimeSinceLastNewTick.Restart ();
   foreach (Status status in newTickPacket.Statuses) {
    if (status.ObjectId == client.ObjectId) {
     List<StatData> list = new List<StatData> (status.Data) {
     new StatData {
     Id = StatsType.AccountFame,
     IntValue = CurrPlayers.Count<KeyValuePair<int, int>> ()
     },
     new StatData {
     Id = StatsType.Credits,
     IntValue = CurrPlayers.Count<KeyValuePair<int, int>> ()
     }
     };
     status.Data = list.ToArray ();
    }
    foreach (StatData statData in status.Data) {
     if ((statData.Id >= 12 && statData.Id <= 19) || (statData.Id >= 71 && statData.Id <= 78 && status.ObjectId == connectedClient.ObjectId)) {
      int num4 = 8;
      if (statData.Id >= 71 && statData.Id <= 78) {
       num4 = 59;
      }
      if (statData.IntValue != -1) {
       string name = GameData.Items.ByID ((ushort) statData.IntValue).Name;
       byte slotid = (byte) (statData.Id - num4);
       int intValue = statData.IntValue;
       if (MilaConfig.Default.AutoUseCloverAndLootDrop && (name.Contains ("Lucky Clover") || name.Contains ("Loot Drop Potion"))) {
        Use (client, slotid, (int) ((short) intValue), 0);
       }
       if (MilaConfig.Default.PopMysteryStatPots && name.Contains ("Mystery Stat Pot")) {
        Use (client, slotid, (int) ((short) intValue), 0);
       }
      }
     }
    }
   }
   if (MilaConfig.Default.NotifyVialPickup && currMap == "Cultist Hideout") {
    foreach (Status status2 in newTickPacket.Statuses) {
     bool flag = false;
     bool flag2 = false;
     foreach (StatData statData2 in status2.Data) {
      if ((statData2.Id >= 8 && statData2.Id <= 19) || (statData2.Id >= 71 && statData2.Id <= 78)) {
       if (statData2.IntValue == (int) VIAL) {
        flag = true;
       }
       if (statData2.IntValue == -1) {
        flag2 = true;
       }
      }
      if (statData2.Id == 61 && status2.ObjectId == Malus2) {
       statData2.IntValue = 0;
      }
     }
     if (flag && !flag2) {
      XLog (ObjToString[status2.ObjectId] + " picked up a vial!");
     }
    }
   }
   foreach (Status status3 in newTickPacket.Statuses) {
    if (status3.Data.Count<StatData> () >= 65 && status3.Data[28].StringValue != null) {
     dict[status3.Data[28].StringValue].Parse (newTickPacket);
    }
   }
  }

  private void OnServerPlayerShoot (Client client, Packet packet) {
   ServerPlayerShootPacket serverPlayerShootPacket = packet as ServerPlayerShootPacket;
   if (MilaConfig.Default.AntiLag && serverPlayerShootPacket.OwnerId != client.ObjectId) {
    serverPlayerShootPacket.Send = false;
   }
  }

  private void OnReconnect (Client client, Packet packet) {
   ReconnectPacket reconnectPacket = packet as ReconnectPacket;
   ReconName = reconnectPacket.Name;
   PluginUtils.Log ("Mila", "<RECONNECT> Name: " + reconnectPacket.Name);
  }

  private void OnHello (Client client, Packet packet) {
   HelloPacket helloPacket = packet as HelloPacket;
   TimeSinceLastHello.Restart ();
  }

  private void OnShowEffect (Client client, Packet packet) {
   ShowEffectPacket showEffectPacket = (ShowEffectPacket) packet;
   if (MilaConfig.Default.AntiLag && showEffectPacket.TargetId != client.ObjectId) {
    EffectType effectType = showEffectPacket.EffectType;
   }
   if (MilaConfig.Default.SpellRandomColor && showEffectPacket.TargetId == client.ObjectId) {
    showEffectPacket.Color.A = (byte) rnd.Next (256);
    showEffectPacket.Color.R = (byte) rnd.Next (256);
    showEffectPacket.Color.G = (byte) rnd.Next (256);
    showEffectPacket.Color.B = (byte) rnd.Next (256);
   }
  }

  private void OnAllyShoot (Client client, Packet packet) {
   AllyShootPacket allyShootPacket = (AllyShootPacket) packet;
   if (MilaConfig.Default.AntiLag) {
    allyShootPacket.Send = false;
   }
  }

  private void OnTradeRequested (Client client, Packet packet) {
   TradeRequestedPacket tradeRequestedPacket = (TradeRequestedPacket) packet;
   PlayerTextPacket playerTextPacket = (PlayerTextPacket) Packet.Create (PacketType.PLAYERTEXT);
   if (MilaConfig.Default.BlockTradeRequests) {
    tradeRequestedPacket.Send = false;
   }
   if (SoundAlerts.Default.TradeRequest) {
    PlayAlert (Resources.Trade);
    PluginUtils.Log ("Mila", "Trade requested");
   }
  }

  private void OnDamage (Client client, Packet packet) {
   DamagePacket damagePacket = packet as DamagePacket;
   if (MilaConfig.Default.AntiLag && damagePacket.ObjectId == client.ObjectId) {
    int targetId = damagePacket.TargetId;
    int objectId = client.ObjectId;
   }
  }

  private void OnQuestObjId (Client client, Packet packet) {
   QuestObjIdPacket p = packet as QuestObjIdPacket;
   if (MilaConfig.Default.ReplaceQuestWithSentry && currMap == "Realm of the Mad God") {
    PluginUtils.Delay (5000, delegate {
     p.ObjectId = LostSentry;
    });
    PluginUtils.Delay (5000, delegate {
     client.SendToClient (p);
    });
   }
  }

  private void PlayAlert (Stream sound) {
   SoundPlayer soundPlayer = new SoundPlayer (sound);
   soundPlayer.Play ();
  }
 }
}
