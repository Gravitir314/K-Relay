namespace Lib_K_Relay.Networking.Packets.Server
{
	public class HatchPetPacket : Packet
	{
		public string petName;
		public int petSkin;
		public int itemType;

		public override PacketType Type
		{ get { return PacketType.HATCHPET; } }

		public override void Read(PacketReader r)
		{
			petName = r.ReadString();
			petSkin = r.ReadInt32();
			itemType = r.ReadInt32();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(petName);
			w.Write(petSkin);
			w.Write(itemType);
		}
	}
}
