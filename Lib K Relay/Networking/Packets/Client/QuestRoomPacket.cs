namespace Lib_K_Relay.Networking.Packets.Server
{
	public class QuestRoomPacket : Packet
	{
		public override PacketType Type
		{ get { return PacketType.QUESTROOMMSG; } }

		public override void Read(PacketReader r)
		{
		}

		public override void Write(PacketWriter w)
		{
		}
	}
}
