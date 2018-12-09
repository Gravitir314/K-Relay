using System;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;

namespace Mila
{
	public static class ClientExtensions
	{
		public static void AnnounceYellow(this Client client, string message)
		{
			client.SendToClient(ClientExtensions.Announce(message, ""));
		}

		public static void AnnounceOrange(this Client client, string message)
		{
			client.SendToClient(ClientExtensions.Announce(message, "*Help*"));
		}

		public static TextPacket Announce(string message, string name)
		{
			TextPacket textPacket = (TextPacket)Packet.Create(PacketType.TEXT);
			textPacket.BubbleTime = 0;
			textPacket.CleanText = message;
			textPacket.Name = name;
			textPacket.NumStars = -1;
			textPacket.ObjectId = -1;
			textPacket.Recipient = "";
			textPacket.Text = "<Mila> " + message;
			return textPacket;
		}
	}
}
