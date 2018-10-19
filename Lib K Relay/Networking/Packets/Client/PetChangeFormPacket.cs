using Lib_K_Relay.Networking.Packets.DataObjects;

namespace Lib_K_Relay.Networking.Packets.Client
{
	public class PetChangeFormPacket : Packet
	{
		public int petInstanceId;
		public int pickedNewPetType;
		public SlotObject item;

		public override PacketType Type
		{ get { return PacketType.PETCHANGEFORMMSG; } }

		public override void Read(PacketReader r)
		{
			petInstanceId = r.ReadInt32();
			pickedNewPetType = r.ReadInt32();
			item = (SlotObject)new SlotObject().Read(r);
		}

		public override void Write(PacketWriter w)
		{
			w.Write(petInstanceId);
			w.Write(pickedNewPetType);
			item.Write(w);
		}
	}
}
