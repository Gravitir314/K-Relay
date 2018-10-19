namespace Lib_K_Relay.Networking.Packets.Client
{
	public class ActivePetUpdateRequestPacket : Packet
	{
		public byte commandtype;
		public uint instanceid;

		public override PacketType Type
		{ get { return PacketType.ACTIVEPETUPDATEREQUEST; } }

		public override void Read(PacketReader r)
		{
			commandtype = r.ReadByte();
			instanceid = r.ReadUInt32();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(commandtype);
			w.Write(instanceid);
		}
	}
}
