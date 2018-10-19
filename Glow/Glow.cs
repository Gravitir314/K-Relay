using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;

namespace Glow
{
    public class Glow : IPlugin
    {
        public string GetAuthor()
        { return "KrazyShank / Kronks"; }

        public string GetName()
        { return "Glow"; }

        public string GetDescription()
        { return "You're so excited about K Relay that you're literally glowing!"; }

        public string[] GetCommands()
        { return new string[] { }; }

        public void Initialize(Proxy proxy)
        {
            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
        }

        private void OnUpdate(Client client, Packet packet)
        {
            UpdatePacket update = (UpdatePacket)packet;

            for (int i = 0; i < update.NewObjs.Length; i++)
            {
                if (update.NewObjs[i].Status.ObjectId == client.ObjectId)
                {
                    for (int j = 0; j < update.NewObjs[i].Status.Data.Length; j++)
                    {
                        if (update.NewObjs[i].Status.Data[j].Id == 59)
                            update.NewObjs[i].Status.Data[j].IntValue = 100;
                    }
                }
            }
        }
    }
}
