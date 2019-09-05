public class PacketProcessorProtocol
{
    public static byte[] Serialize(Message message)
    {
        return null;
    }

    public static byte[] SerializeClientToServerMessage(ClientToServerMessage message)
    {
        return null;
    }

    public static Message Deserialize(byte[] data)
    {
        return new Message();
    }

    public static DeserializeClientToServerMessage(byte[] data)
    {
        return new ClientToServerMessage();
    }

    public class Message
    {
        public byte StreamId;
        public byte[] Payload;
    }

    public class ClientToServerMessage
    {
        public byte ClientId;
        public byte StreamId;
        public byte[] Payload;
    }
}