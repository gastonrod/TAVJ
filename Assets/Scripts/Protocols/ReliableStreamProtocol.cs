using System.Collections.Generic;
using System.IO;

namespace Protocols
{
    public class ReliableStreamProtocol
    {
        public static byte[] SerializeMessage(IMessage message)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write((byte) message.GetMessageType());
                    writer.Write(message.MessageId);
                    if (message.GetMessageType() == MessageType.DATA)
                    {
                        writer.Write(((DataMessage) message).Payload);
                    }
                }
                return m.ToArray();
            }
        }

        public static IMessage DeserializeMessage(byte[] serializedMessage)
        {
            using (MemoryStream m = new MemoryStream(serializedMessage))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    MessageType messageType = (MessageType) reader.ReadByte();
                    int messageId = reader.ReadInt32();
                    if (messageType == MessageType.DATA)
                    {
                        return new DataMessage(messageId, reader.ReadBytes(serializedMessage.Length - 5));    // TODO: change for "m.Length - m.Position" or similar
                    }
                    return new AckMessage(messageId);
                }
            }
        }

        public static LightMessage DeserializeLightMessage(byte[] data)
        {
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    MessageType messageType = (MessageType) reader.ReadByte();
                    int messageId = reader.ReadInt32();
                    return new LightMessage(messageType, messageId);
                }
            }
        }
        
        public enum MessageType : byte { DATA = 0, ACK = 1 }

        public abstract class IMessage
        {
            public readonly int MessageId;

            protected IMessage(int messageId)
            {
                MessageId = messageId;
            }
            
            public abstract MessageType GetMessageType();
        }

        public class DataMessage : IMessage
        {
            public byte[] Payload;
            
            public DataMessage(int messageId, byte[] payload) : base(messageId)
            {
                Payload = payload;
            }

            public override MessageType GetMessageType()
            {
                return MessageType.DATA;
            }
        }

        public class AckMessage : IMessage
        {
            public AckMessage(int messageId) : base(messageId) {}

            public override MessageType GetMessageType()
            {
                return MessageType.ACK;
            }
        }

        public class LightMessage
        {
            public readonly MessageType Type;
            public readonly int MessageId;

            public LightMessage(MessageType type, int messageId)
            {
                Type = type;
                MessageId = messageId;
            }
        }
        
        public class SeenManager
        {
            private int _highestConsecutiveAck = 0;
            private SortedSet<int> _nonConsecutiveAcks = new SortedSet<int>();
            
            /* Tells this SeenManager that a message with id messageId has arrived
             * Returns whether the message has been seen before or not
             */
            public bool GiveMessage(int messageId)
            {
                if (messageId <= _highestConsecutiveAck || _nonConsecutiveAcks.Add(messageId))
                {
                    return true;
                }
                var enumerator = _nonConsecutiveAcks.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == _highestConsecutiveAck + 1)
                    {
                        _highestConsecutiveAck++;
                    }
                    else
                    {
                        break;
                    }
                }
                enumerator.Dispose();
                _nonConsecutiveAcks.RemoveWhere(item => item <= _highestConsecutiveAck);
                return false;
            }
        }
    }
}