namespace Lib_K_Relay.Networking.Packets.Server
{
	public class GuildResultPacket : Packet
	{
		public bool success;
		public string lineBuilderJSON;

		public override PacketType Type
		{ get { return PacketType.GUILDRESULT; } }

		public override void Read(PacketReader r)
		{
			success = r.ReadBoolean();
			lineBuilderJSON = r.ReadString();
		}

		public override void Write(PacketWriter w)
		{
			w.Write(success);
			w.Write(lineBuilderJSON);
		}
	}
}
