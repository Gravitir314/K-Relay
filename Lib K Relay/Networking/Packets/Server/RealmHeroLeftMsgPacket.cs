namespace Lib_K_Relay.Networking.Packets.Server
{
    public class RealmHeroLeftMsgPacket : Packet
    {
        public int RealmHeroesLeft;

        public override PacketType Type
        { get { return PacketType.REALMHEROLEFTMSG; } }

        public override void Read(PacketReader r)
        {
            RealmHeroesLeft = r.ReadInt32();
        }

        public override void Write(PacketWriter w)
        {
            w.Write(RealmHeroesLeft);
        }
    }
}
