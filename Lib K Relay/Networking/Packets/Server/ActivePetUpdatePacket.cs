namespace Lib_K_Relay.Networking.Packets.Server
{
	public class ActivePetUpdatePacket : Packet
	{
		public int instanceID;

		public override PacketType Type
		{ get { return PacketType.ACTIVEPETUPDATE; } }

		public override void Read(PacketReader r)
		{
			instanceID = r.ReadInt32();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(instanceID);
		}
	}
}
