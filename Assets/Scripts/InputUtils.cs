using UnityEngine;

namespace DefaultNamespace
{
    public class InputUtils
    {
        public static byte GetKeyboardInput()
        {
            byte input = 0;
            if (Input.GetKey(KeyCode.A))
            {
                input |= (byte)InputCodifications.FORWARD;
            } 
            if (Input.GetKey(KeyCode.D))
            {
                input |= (byte)InputCodifications.BACK;
            } 
            if (Input.GetKey(KeyCode.W))
            {
                input |= (byte)InputCodifications.RIGHT;
            } 
            if (Input.GetKey(KeyCode.S))
            {
                input |= (byte)InputCodifications.LEFT;
            }

            if (Input.GetKey(KeyCode.K))
            {
                input |= (byte) InputCodifications.SPAWN_ENEMY;
            }
            return input;
        }

        public static bool InputSpawnEnemy(byte input)
        {
            return (input & (byte)InputCodifications.SPAWN_ENEMY) != 0;
        }

        public static Vector3 DecodeInput(byte input)
        {
            Vector3 pos = new Vector3(0,0,0);
            input = (byte)(input & 0x1f);
            if ((input & ((byte)InputCodifications.FORWARD)) > 0)
            {
                pos += Vector3.forward;
            }
            if ((input & ((byte)InputCodifications.BACK)) > 0)
            {
                pos += Vector3.back;
            }
            if ((input & ((byte)InputCodifications.RIGHT)) > 0)
            {
                pos += Vector3.right;
            }
            if ((input & ((byte)InputCodifications.LEFT)) > 0)
            {
                pos += Vector3.left;
            }
            return pos;
        }
        
    }
    public enum InputCodifications : byte
    {
        RIGHT = 1, LEFT = 1 << 2, FORWARD = 1 << 3, BACK = 1 << 4, SPAWN_ENEMY = 1 << 5,
    }
}