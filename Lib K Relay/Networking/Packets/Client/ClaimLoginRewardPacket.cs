namespace Lib_K_Relay.Networking.Packets.Client
{
	public class ClaimLoginRewardPacket : Packet
	{
		public string claimKey;
		public string type;

		public override PacketType Type
		{ get { return PacketType.CLAIMLOGINREWARDMSG; } }

		public override void Read(PacketReader r)
		{
			claimKey = r.ReadString();
			type = r.ReadString();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(claimKey);
			w.Write(type);
		}
	}
}
