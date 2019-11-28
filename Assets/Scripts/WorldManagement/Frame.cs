using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class Frame
    {
        private Dictionary<byte, Vector3> _enemies = new Dictionary<byte, Vector3>();
        private Dictionary<byte, Vector3> _characters = new Dictionary<byte, Vector3>();
        public byte frameID;
        private byte _playerId;
        private byte _lastInputId;
        private bool _isInjected = false;

        private Frame(Dictionary<byte, Vector3> enemies, Dictionary<byte, Vector3> characters, byte lastInputId, byte frameID)
        {
            _enemies = enemies;
            _characters = characters;
            _lastInputId = lastInputId;
            this.frameID = frameID;
        }

        public Frame(Frame frame)
        {
            _enemies = new Dictionary<byte, Vector3>();
            _characters = new Dictionary<byte, Vector3>();
            _lastInputId = frame._lastInputId;
            frameID = frame.frameID;
            _isInjected = true;
            foreach (KeyValuePair<byte, Vector3> enemyPair in frame._enemies)
            {
                _enemies[enemyPair.Key] = enemyPair.Value;
            }
            foreach (KeyValuePair<byte, Vector3> characterPair in frame._characters)
            {
                _characters[characterPair.Key] = characterPair.Value;
            }
        }
        
        public Frame(byte[] snapshot, byte playerId, Queue<InputPackage> inputPackages)
        {
            _playerId = playerId;
            frameID = snapshot[0];
            for (int i = 1; i < snapshot.Length;)
            {
                byte objId = snapshot[i++];
                PrimitiveType primitiveType = (PrimitiveType) snapshot[i++];
                switch (primitiveType)
                {
                    case PrimitiveType.Capsule:
                        if (objId == playerId)
                        {
                            _lastInputId = snapshot[i];
                        }
                        i++;
                        _characters[objId] = Utils.ByteArrayToVector3(snapshot, i);
                        break;
                    case PrimitiveType.Cylinder:
                        i++;
                        _enemies[objId] = Utils.ByteArrayToVector3(snapshot, i);
                        break;
                }
                i += 12;
            }
            ApplyPredictedInputs(inputPackages);
        }

        void ApplyPredictedInputs(Queue<InputPackage> inputPackages)
        {
            if (inputPackages.Count == 0)
                return;
            byte maxInputId = _lastInputId;
            foreach (InputPackage inputPackage in inputPackages)
            {
                if (inputPackage.id > _lastInputId)
                {
                    _characters[_playerId] = _characters[_playerId] + InputUtils.DecodeInput(inputPackage.input);
                    maxInputId = inputPackage.id;
                }
            }
            while (inputPackages.Count > 0 && inputPackages.Peek().id <= _lastInputId)
            {
                inputPackages.Dequeue();
            }

            _lastInputId = maxInputId;
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
            Frame returnFrame = new Frame(enemies, characters, f1._lastInputId, f1.frameID);
            return returnFrame;
        }
        
        public override string ToString()
        {
            string s = "(" + frameID + ", " + _lastInputId+ "): {";
            if (_enemies.Count > 0)
            {
                s += "Enemies: {";
                foreach (KeyValuePair<byte, Vector3> enemyPair in _enemies)
                {
                    s += " <" + enemyPair.Key + ": " + enemyPair.Value + ">,";
                }
            }

            s += "}\nCharacters: {";
            foreach (KeyValuePair<byte, Vector3> characterPair in _characters)
            {
                s += " <" + characterPair.Key + ": " + characterPair.Value + ">,";
            }
            s += "}";
            return s;
        }

        public void PredictMovement(InputPackage inputPackage, byte playerId)
        {
            _playerId = playerId;
            _lastInputId = inputPackage.id;
            _characters[playerId] = _characters[playerId] + InputUtils.DecodeInput(inputPackage.input);
        }

        public void UpdateOtherEntitiesPositions(Frame frame)
        {
            Vector3 playerPosition = _characters[_playerId];
            _enemies = frame._enemies;
            _characters = frame._characters;
            _characters[_playerId] = playerPosition;
            frameID = frame.frameID;
        }

        public bool IsInjected()
        {
            return _isInjected;
        }
    }
}