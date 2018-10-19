using Lib_K_Relay.Networking.Packets.DataObjects;

namespace Lib_K_Relay.Networking.Packets.Client
{

	public class PetUpgradeRequestPacket : Packet
	{
		public int petTransType;
		public int PIDOne;
		public int PIDTwo;
		public int objectId;
		public int paymentTransType;
		public SlotObject[] slotsObject;

		public override PacketType Type
		{ get { return PacketType.PETUPGRADEREQUEST; } }

		public override void Read(PacketReader r)
		{
			petTransType = r.ReadInt32();
			PIDOne = r.ReadInt32();
			PIDTwo = r.ReadInt32();
			objectId = r.ReadInt32();
			paymentTransType = r.ReadInt32();
			slotsObject = new SlotObject[r.ReadInt16()];
			for (int i = 0; i < slotsObject.Length; i++)
				slotsObject[i] = (SlotObject)new SlotObject().Read(r);
		}

		public override void Write(PacketWriter w)
		{
			w.Write(petTransType);
			w.Write(PIDOne);
			w.Write(PIDTwo);
			w.Write(objectId);
			w.Write(paymentTransType);
			w.Write((short)slotsObject.Length);
			foreach (SlotObject l in slotsObject)
				l.Write(w);
		}

	}
}