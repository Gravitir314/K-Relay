using Lib_K_Relay.Networking.Packets.DataObjects;

namespace Lib_K_Relay.Networking.Packets.Client
{
	public class QuestRedeemPacket : Packet
	{
		public string questID;
		public int item;
		public SlotObject[] slots;

		public override PacketType Type
		{ get { return PacketType.QUESTREDEEM; } }

		public override void Read(PacketReader r)
		{
			questID = r.ReadString();
			item = r.ReadInt32();
			slots = new SlotObject[r.ReadInt16()];
			for (int i = 0; i < slots.Length; i++)
				slots[i] = (SlotObject)new SlotObject().Read(r);
		}

		public override void Write(PacketWriter w)
		{
			w.Write(questID);
			w.Write(item);
			w.Write((short)slots.Length);
			foreach (SlotObject l in slots)
				l.Write(w);
		}
	}
}
