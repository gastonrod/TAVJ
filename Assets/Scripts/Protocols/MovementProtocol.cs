using System;

namespace Protocols
{
    public class MovementProtocol
    {
        public enum Direction : byte {Up = 1, Down = 2, Left = 3, Right = 4}
        
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
    }

}