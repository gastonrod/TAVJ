using System;
using DefaultNamespace;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

namespace Connections
{
    public class Utils
    {

        public static ConnectionClasses GetConnectionClasses(int sourcePort, int destinationPort, ILogger logger)
        {
            return new ConnectionClasses(sourcePort, destinationPort, logger);
        }

        public static Vector3 DecodeInput(byte input)
        {
            Vector3 pos = new Vector3(0,0,0);
            if ((input & ((byte)InputCodifications.LEFT)) > 0)
            {
                pos.x += 1;
            }
            if ((input & ((byte)InputCodifications.RIGHT)) > 0)
            {
                pos.x -= 1;
            }
            if ((input & ((byte)InputCodifications.UP)) > 0)
            {
                pos.z -= 1;
            }
            if ((input & ((byte)InputCodifications.DOWN)) > 0)
            {
                pos.z += 1;
            }
            return pos;
        }
        
        public static byte[] Vector3ToByteArray(Vector3 v3)
        {
            byte[] buffer = new byte[3 * sizeof(float)];
            int startingIdx = 0;
            Vector3ToByteArray(v3, buffer, startingIdx);
            return buffer;
        }
        
        public static void Vector3ToByteArray(Vector3 v3, byte[] buffer, int startingIdx)
        {
            Buffer.BlockCopy( BitConverter.GetBytes( v3.x ), 0, buffer, (startingIdx++)*sizeof(float), sizeof(float));
            Buffer.BlockCopy( BitConverter.GetBytes( v3.y ), 0, buffer, (startingIdx++)*sizeof(float), sizeof(float));
            Buffer.BlockCopy( BitConverter.GetBytes( v3.z ), 0, buffer, (startingIdx)*sizeof(float), sizeof(float));
        }
        
        public static Vector3 ByteArrayToVector3(byte[] bytes, int startingIdx)
        {
            Vector3 vect = Vector3.zero;
            vect.x = BitConverter.ToSingle(bytes, (startingIdx++) * sizeof(float));
            vect.y = BitConverter.ToSingle(bytes, (startingIdx++) * sizeof(float));
            vect.z = BitConverter.ToSingle(bytes, (startingIdx) * sizeof(float));
            return vect;
        }

        public static String ArrayToString(byte[] array)
        {
            return "[" + string.Join(",", array) + "]";
        }
    }
}