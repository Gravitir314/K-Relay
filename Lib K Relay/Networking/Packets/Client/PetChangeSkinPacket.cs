namespace Lib_K_Relay.Networking.Packets.Client
{
    public class PetChangeSkinPacket : Packet
    {
        public int petID;
        public int skinType;
        public int currency;

        public override PacketType Type
        { get { return PacketType.PETCHANGESKINMSG; } }

        public override void Read(PacketReader r)
        {
            petID = r.ReadInt32();
            skinType = r.ReadInt32();
			currency = r.ReadInt32();
        }

        public override void Write(PacketWriter w)
        {
            w.Write(petID);
            w.Write(skinType);
			w.Write(currency);
        }
    }
}
