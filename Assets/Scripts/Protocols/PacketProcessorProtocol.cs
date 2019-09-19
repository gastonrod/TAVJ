using System;
using System.IO;

namespace Protocols
{
   public class PacketProcessorProtocol
    {
        public static byte[] SerializeClientToServerMessage(ClientToServerMessage message)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(message.ClientId);
                    writer.Write(message.StreamId);
                    writer.Write(message.Payload);
                }
                return m.ToArray();
            }
        }

        public static ClientToServerMessage DeserializeClientToServerMessage(byte[] data)
        {
            ClientToServerMessage result = new ClientToServerMessage();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.ClientId = reader.ReadByte();
                    result.StreamId = reader.ReadByte();
                    result.Payload = reader.ReadBytes(data.Length - 2);    // TODO: change for "m.Length - m.Position" or similar
                }
            }
            return result;
        }

        public static byte[] SerializeServerToClientMessage(ServerToClientMessage message)
        {
            throw new NotImplementedException();
        }
        
        public static ServerToClientMessage DeserializeServerToClientMessage(byte[] data)
        {
            throw new NotImplementedException();
        }
        
        public class ClientToServerMessage
        {
            public byte ClientId;
            public byte StreamId;
            public byte[] Payload;
        }

        public class ServerToClientMessage
        {
            public byte StreamId;
            public byte[] Payload;
        }
    }
}