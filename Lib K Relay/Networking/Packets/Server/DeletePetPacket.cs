namespace Lib_K_Relay.Networking.Packets.Server
{
	public class DeletePetPacket : Packet
	{
		public int petID;

		public override PacketType Type
		{ get { return PacketType.DELETEPET; } }

		public override void Read(PacketReader r)
		{
			petID = r.ReadInt32();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(petID);
		}
	}
}
