namespace Lib_K_Relay.Networking.Packets.Server
{
	public class PetYardUpdatePacket : Packet
	{
		public int type;

		public override PacketType Type
		{ get { return PacketType.PETYARDUPDATE; } }

		public override void Read(PacketReader r)
		{
			type = r.ReadInt32();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(type);
		}
	}
}
