public class PacketProcessorProtocol
{
    public static byte[] Serialize(Message message)
    {
        return null;
    }

    public static Message Deserialize(byte[] data)
    {
        return new Message();
    }

    public struct Message
    {
        public byte StreamId;
        public byte[] Payload;
    }
}