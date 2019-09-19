namespace Protocols
{
    public class JoinProtocol
    {
        public static byte[] SerializeJoinRequestMessage(JoinRequestMessage message)
        {
            var serializedMessage = new byte[1];
            serializedMessage[0] = message.Placeholder;
            return serializedMessage;
        }

        public static JoinRequestMessage DeserializeJoinRequestMessage(byte[] message)
        {
            return new JoinRequestMessage {Placeholder = message[0]};
        }
        
        public static byte[] SerializeJoinResponseMessage(JoinResponseMessage message)
        {
            var serializedMessage = new byte[1];
            serializedMessage[0] = message.ClientId;
            return serializedMessage;
        }

        public static JoinResponseMessage DeserializeJoinResponseMessage(byte[] message)
        {
            return new JoinResponseMessage {ClientId = message[0]};
        }
        
        public static byte[] SerializeJoinAcceptMessage(JoinAcceptMessage message)
        {
            var serializedMessage = new byte[1];
            serializedMessage[0] = message.ClientId;
            return serializedMessage;
        }

        public static JoinAcceptMessage DeserializeJoinAcceptMessage(byte[] message)
        {
            return new JoinAcceptMessage {ClientId = message[0]};
        }
    }

    public class JoinRequestMessage
    {
        public byte Placeholder = 0;
    }

    public class JoinResponseMessage
    {
        public byte ClientId;
    }

    public class JoinAcceptMessage
    {
        public byte ClientId;
    }
}