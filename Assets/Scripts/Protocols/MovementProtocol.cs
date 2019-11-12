using System;
using System.IO;

namespace Protocols
{
    public class MovementProtocol
    {
        public enum Direction : byte {Nop = 0, Up = 1, Down = 2, Left = 3, Right = 4}

        public static byte[] SerializeMessage(MovementMessage message)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(message.id);
                    writer.Write((byte) message.direction);
                    writer.Write(message.horizontalRotation);
                    writer.Write(message.scalarRotation);
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
                    result.horizontalRotation = reader.ReadSingle();
                    result.scalarRotation = reader.ReadSingle();
                }
            }
            return result;
        }

        public class MovementMessage
        {
            public int id;
            public Direction direction;
            public float horizontalRotation;
            public float scalarRotation;
        }
    }

}