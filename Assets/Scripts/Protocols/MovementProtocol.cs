using System;
using System.IO;

namespace Protocols
{
    public class MovementProtocol
    {
        public enum Direction : byte {Up = 1, Down = 2, Left = 3, Right = 4}

        public static byte[] SerializeMessage(MovementMessage message)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(message.id);
                    writer.Write((byte) message.direction);
                }
                return m.ToArray();
            }
        }

        public static MovementMessage DeserializeMessage(byte[] serializedMessage)
        {
            MovementMessage result = new MovementMessage();
            using (MemoryStream m = new MemoryStream(serializedMessage))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.id = reader.ReadInt32();
                    result.direction = (Direction)reader.ReadByte();
                }
            }
            return result;
        }
        
        public static byte[] Serialize(Direction direction)
        {
            var result = new byte[1];
            result[0] = (byte)direction;
            return result;
        }

        public static Direction Deserialize(byte[] data)
        {
            if (data.Length != 1)
            {
                throw new Exception("Incorrect packet length");
            }
            return (Direction) data[0];
        }

        public class MovementMessage
        {
            public int id;
            public Direction direction;
        }
    }

}