namespace Lib_K_Relay.Networking.Packets.Server
{
	public class LoginRewardPacket : Packet
	{
		public int itemId;
		public int quantity;
		public int gold;

		public override PacketType Type
		{ get { return PacketType.LOGINREWARDMSG; } }

		public override void Read(PacketReader r)
		{
			itemId = r.ReadInt32();
			quantity = r.ReadInt32();
			gold = r.ReadInt32();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(itemId);
			w.Write(quantity);
			w.Write(gold);
		}
	}
}
