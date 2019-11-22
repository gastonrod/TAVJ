using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace DefaultNamespace
{
    public class Frame
    {
        private Dictionary<byte, Vector3> enemies = new Dictionary<byte, Vector3>();
        private Dictionary<byte, Vector3> characters = new Dictionary<byte, Vector3>();
        public byte frameID;

        public Frame(byte[] snapshot)
        {
            frameID = snapshot[0];
            for (int i = 1; i < snapshot.Length;)
            {
                byte objID = snapshot[i++];
                PrimitiveType primitiveType = (PrimitiveType) snapshot[i++];
                switch (primitiveType)
                {
                    case PrimitiveType.Capsule:
                        characters[objID] = Utils.ByteArrayToVector3(snapshot, i);
                        break;
                    case PrimitiveType.Cylinder:
                        enemies[objID] = Utils.ByteArrayToVector3(snapshot, i);
                        break;
                }
                i += 12;
            }
        }

        public Dictionary<byte, Vector3> GetEnemies()
        {
            return enemies;
        }
        public Dictionary<byte, Vector3> GetCharacters()
        {
            return characters;
        }
    }
}