using System;
using System.Collections.Generic;
using Connections;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

namespace DefaultNamespace
{
    public class Utils
    {

        public static ConnectionClasses GetConnectionClasses(int sourcePort, int delayInMs, int packetLossPct,ILogger logger)
        {
            return new ConnectionClasses(sourcePort, delayInMs, packetLossPct, logger);
        }
        public static ConnectionClasses GetConnectionClasses(int sourcePort, int destinationPort, ILogger logger)
        {
            return new ConnectionClasses(sourcePort, destinationPort, logger);
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
            Buffer.BlockCopy( BitConverter.GetBytes( v3.x ), 0, buffer, startingIdx, sizeof(float));
            startingIdx += sizeof(float);
            Buffer.BlockCopy( BitConverter.GetBytes( v3.y ), 0, buffer, startingIdx, sizeof(float));
            startingIdx += sizeof(float);
            Buffer.BlockCopy( BitConverter.GetBytes( v3.z ), 0, buffer, startingIdx, sizeof(float));
        }
        
        public static Vector3 ByteArrayToVector3(byte[] bytes, int idx)
        {
            Vector3 vect = Vector3.zero;
            vect.x = BitConverter.ToSingle(bytes, idx);
            idx += sizeof(float);
            vect.y = BitConverter.ToSingle(bytes, idx);
            idx += sizeof(float);
            vect.z = BitConverter.ToSingle(bytes, idx);
            return vect;
        }

        public static String ArrayToString(byte[] array)
        {
            return "[" + string.Join(",", array) + "]";
        }

        public static string QueueToString(Queue<IPDataPacket> queue)
        {
            return "[" + string.Join(",", queue) + "]";
        }

        public static string FrameToString(byte[] frame)
        {
            string s = "{ " + frame[0] + ": ";
            for (int i = 1; i < frame.Length;)
            {
                s += "<" + frame[i] + ", ";
                i += 2;
                s += ByteArrayToVector3(frame, i) + ">, ";
                i += 12;
            }
            s += "}";
            return s;
        }

        public static bool FramesAreEqual(byte[] snapshot, byte[] interpolatedFrame)
        {
            if (snapshot == null || interpolatedFrame == null || snapshot.Length != interpolatedFrame.Length)
                return false;
            if (snapshot == interpolatedFrame)
                return true;
            for (int i = 1; i < snapshot.Length; i++)
            {
                if (snapshot[i] != interpolatedFrame[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}