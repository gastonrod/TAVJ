using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class Frame
    {
        private Dictionary<byte, Vector3> _enemies = new Dictionary<byte, Vector3>();
        private Dictionary<byte, Vector3> _characters = new Dictionary<byte, Vector3>();
        public byte frameID;

        private Frame(Dictionary<byte, Vector3> enemies, Dictionary<byte, Vector3> characters)
        {
            _enemies = enemies;
            _characters = characters;
        }
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
                        _characters[objID] = Utils.ByteArrayToVector3(snapshot, i);
                        break;
                    case PrimitiveType.Cylinder:
                        _enemies[objID] = Utils.ByteArrayToVector3(snapshot, i);
                        break;
                }
                i += 12;
            }
        }

        public Dictionary<byte, Vector3> GetEnemies()
        {
            return _enemies;
        }
        public Dictionary<byte, Vector3> GetCharacters()
        {
            return _characters;
        }

        public static bool FramesAreEqual(Frame f1, Frame f2)
        {
            if(f1 == f2 || (f1 == null && f2 != null) || (f2 == null && f1 != null))
                return true;
            if (f1._enemies.Count != f2._enemies.Count || f1._characters.Count != f2._characters.Count)
                return false;
            foreach (KeyValuePair<byte, Vector3> enemyPair in f1._enemies)
            {
                if (!f2._enemies.ContainsKey(enemyPair.Key) || !f2._enemies[enemyPair.Key].Equals(enemyPair.Value))
                    return false;
            }
            foreach (KeyValuePair<byte, Vector3> characterPair in f1._characters)
            {
                if (!f2._characters.ContainsKey(characterPair.Key) || !f2._characters[characterPair.Key].Equals(characterPair.Value))
                    return false;
            }
            return true;
        }

        public static Frame Interpolate(Frame f0, Frame f1, float percentageOfFrame)
        {
            Dictionary<byte, Vector3> enemies = new Dictionary<byte, Vector3>();
            Dictionary<byte, Vector3> characters = new Dictionary<byte, Vector3>();
            foreach (KeyValuePair<byte, Vector3> enemyPair in f1._enemies)
            {
                if (f0._enemies.ContainsKey(enemyPair.Key))
                {
                    Vector3 f0ObjPos = f0._enemies[enemyPair.Key];
                    enemies[enemyPair.Key] = f0ObjPos + (enemyPair.Value - f0ObjPos) * percentageOfFrame;
                }
                else
                {
                    enemies[enemyPair.Key] = enemyPair.Value;
                }
            }
            foreach (KeyValuePair<byte, Vector3> characterPair in f1._characters)
            {
                if (f0._characters.ContainsKey(characterPair.Key))
                {
                    Vector3 f0ObjPos = f0._characters[characterPair.Key];
                    characters[characterPair.Key] = f0ObjPos + (characterPair.Value - f0ObjPos) * percentageOfFrame;
                }
                else
                {
                    characters[characterPair.Key] = characterPair.Value;
                }
            }
            Frame returnFrame = new Frame(enemies, characters);
            return returnFrame;
        }
        
        public override string ToString()
        {
            string s = "Enemies: {";
            foreach (KeyValuePair<byte, Vector3> enemyPair in _enemies)
            {
                s += " <" + enemyPair.Key + ": " + enemyPair.Value + ">,";
            }

            s += "}\nCharacters: {";
            foreach (KeyValuePair<byte, Vector3> characterPair in _characters)
            {
                s += " <" + characterPair.Key + ": " + characterPair.Value + ">,";
            }
            s += "}";
            return s;
        }
    }
}