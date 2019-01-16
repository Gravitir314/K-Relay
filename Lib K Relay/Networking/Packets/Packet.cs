using Lib_K_Relay.GameData.DataStructures;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lib_K_Relay.Networking.Packets
{
    public class Packet
    {
        public bool Send = true;
        public byte Id;

        private byte[] _data;

        public virtual PacketType Type
        { get { return PacketType.UNKNOWN; } }

        public virtual void Read(PacketReader r)
        {
            _data = r.ReadBytes((int)r.BaseStream.Length - 5); // All of the packet data
        }

        public virtual void Write(PacketWriter w)
        {
            w.Write(_data); // All of the packet data
        }

        public static Packet Create(PacketType type)
        {
            PacketStructure st = GameData.GameData.Packets.ByName(type.ToString());
            Packet packet = (Packet)Activator.CreateInstance(st.Type);
            packet.Id = st.ID;
            return packet;
        }

        public static T Create<T>(PacketType type)
        {
            Packet packet = (Packet)Activator.CreateInstance(typeof(T));
            packet.Id = GameData.GameData.Packets.ByName(type.ToString()).ID;
            return (T)Convert.ChangeType(packet, typeof(T));
        }

        public T To<T>()
        {
            return (T)Convert.ChangeType(this, typeof(T));
        }

        public static Packet Create(byte[] data)
        {
            using (PacketReader r = new PacketReader(new MemoryStream(data)))
            {
                r.ReadInt32(); // Skip over int length
                byte id = r.ReadByte();
                PacketStructure st = GameData.GameData.Packets.ByID(id);
                PacketType packetType = st.PacketType;
                Type type = st.Type;
                // Reflect the type to a new instance and read its data from the PacketReader
                Packet packet = (Packet)Activator.CreateInstance(type);
                packet.Id = id;
                packet.Read(r);

                return packet;
            }
        }

        public override string ToString()
        {
            // Use reflection to get the packet's fields and values so we don't have
            // to formulate a ToString method for every packet type.
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);

            StringBuilder s = new StringBuilder();
            s.Append(Type + "(" + Id + ") Packet Instance");
            foreach (FieldInfo f in fields)
            {
                s.Append("\n\t" + f.Name + " => " + f.GetValue(this));
            }
            return s.ToString();
        }

        public string ToStructure()
        {
            // Use reflection to build a list of the packet's fields.
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);

            StringBuilder s = new StringBuilder();
            s.Append(Type + " [" + GameData.GameData.Packets.ByName(Type.ToString()).ID + "] \nPacket Structure:\n{");
            foreach (FieldInfo f in fields)
            {
                s.Append("\n  " + f.Name + " => " + f.FieldType.Name);
            }
            s.Append("\n}");
            return s.ToString();
        }
    }

	public enum PacketType
	{
		UNKNOWN,
		FAILURE,
        CREATESUCCESS,
		CREATE,
		PLAYERSHOOT,
		MOVE,
		PLAYERTEXT,
		TEXT,
		SERVERPLAYERSHOOT,
		DAMAGE,
		UPDATE,
		UPDATEACK,
		NOTIFICATION,
		NEWTICK,
		INVSWAP,
		USEITEM,
		SHOWEFFECT,
		HELLO,
		GOTO,
		INVDROP,
		INVRESULT,
		RECONNECT,
		PING,
		PONG,
		MAPINFO,
		LOAD,
		PIC,
		SETCONDITION,
		TELEPORT,
		USEPORTAL,
		DEATH,
		BUY,
		BUYRESULT,
		AOE,
		GROUNDDAMAGE,
		PLAYERHIT,
		ENEMYHIT,
		AOEACK,
		SHOOTACK,
		OTHERHIT,
		SQUAREHIT,
		GOTOACK,
		EDITACCOUNTLIST,
		ACCOUNTLIST,
		QUESTOBJID,
		CHOOSENAME,
		NAMERESULT,
		CREATEGUILD,
		GUILDRESULT,
		GUILDREMOVE,
		GUILDINVITE,
		ALLYSHOOT,
		ENEMYSHOOT,
		REQUESTTRADE,
		TRADEREQUESTED,
		TRADESTART,
		CHANGETRADE,
		TRADECHANGED,
		ACCEPTTRADE,
		CANCELTRADE,
		TRADEDONE,
		TRADEACCEPTED,
		CLIENTSTAT,
		CHECKCREDITS,
		ESCAPE,
		FILE,
		INVITEDTOGUILD,
		JOINGUILD,
		CHANGEGUILDRANK,
		PLAYSOUND,
		GLOBALNOTIFICATION,
		RESKIN,
		PETUPGRADEREQUEST,
		ACTIVEPETUPDATEREQUEST,
		ACTIVEPETUPDATE,
		NEWABILITY,
		PETYARDUPDATE,
		EVOLVEPET,
		DELETEPET,
		HATCHPET,
		ENTERARENA,
		IMMINENTARENAWAVE,
		ARENADEATH,
		ACCEPTARENADEATH,
		VERIFYEMAIL,
		RESKINUNLOCK,
		PASSWORDPROMPT,
		QUESTFETCHASK,
		QUESTREDEEM,
		QUESTFETCHRESPONSE,
		QUESTREDEEMRESPONSE,
		PETCHANGEFORMMSG,
		KEYINFOREQUEST,
		KEYINFORESPONSE,
		CLAIMLOGINREWARDMSG,
		LOGINREWARDMSG,
		QUESTROOMMSG,
		PETCHANGESKINMSG,
	    REALMHEROLEFTMSG
    }
}
