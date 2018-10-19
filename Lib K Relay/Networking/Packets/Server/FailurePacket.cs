namespace Lib_K_Relay.Networking.Packets.Server
{
    public class FailurePacket : Packet
    {
		public const int INCORRECT_VERSION = 4;
		public const int BAD_KEY = 5;
		public const int INVALID_TELEPORT_TARGET = 6;
		public const int EMAIL_VERIFICATION_NEEDED = 7;
		public const int TELEPORT_REALM_BLOCK = 9;

		public static string ActualBuildVersion;

		public int ErrorId;
        public string ErrorMessage;

        public override PacketType Type
        { get { return PacketType.FAILURE; } }

        public override void Read(PacketReader r)
        {
            ErrorId = r.ReadInt32();
            ErrorMessage = r.ReadString();
        }

        public override void Write(PacketWriter w)
        {
            w.Write(ErrorId);
            w.Write(ErrorMessage);
        }
    }
}
