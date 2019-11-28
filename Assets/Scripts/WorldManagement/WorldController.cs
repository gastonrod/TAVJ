using System.Collections.Generic;
using Connections.Streams;
<<<<<<< Updated upstream
=======
using Connections.Loggers;
>>>>>>> Stashed changes
using DefaultNamespace;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

namespace WorldManagement
{
    public abstract class WorldController
    {
        protected Dictionary<byte, GameObject> _characters = new Dictionary<byte, GameObject>();
        protected Dictionary<byte, GameObject> _enemies = new Dictionary<byte, GameObject>();
        protected byte _movementSpeed = 1;
        protected ILogger _logger;
<<<<<<< Updated upstream

        protected WorldController(ILogger logger)
        {
            _logger = logger;
        }
=======
>>>>>>> Stashed changes

        protected WorldController(ILogger logger)
        {
            _logger = logger;
        }
        
        protected byte[] GetPositions(byte snapshotId)
        {
            int gameObjectsCount = (_characters.Count + _enemies.Count);
            byte[] positions = new byte[gameObjectsCount * UnreliableStream.PACKET_SIZE + 1];
            int j = 0;
            positions[j++] = snapshotId;
            foreach (KeyValuePair<byte, GameObject> enemy in _enemies)
            {
                positions[j++] = enemy.Key;
                positions[j++] = (byte) PrimitiveType.Cylinder;
                Utils.Vector3ToByteArray(enemy.Value.transform.position, positions, j);
                j += 12;
            }
            foreach (KeyValuePair<byte, GameObject> character in _characters)
            {
                positions[j++] = character.Key;
                positions[j++] = (byte) PrimitiveType.Capsule;
                Utils.Vector3ToByteArray(character.Value.transform.position, positions, j);
                j += 12;
            }

            _logger.Log("Positions: " + Utils.FrameToString(positions));
            return positions;
        }

        protected GameObject SpawnObject(byte id, PrimitiveType primitiveType,
            Vector3 pos, Color color, bool addCharacterController = false)
        {
            Dictionary<byte, GameObject> objectDict = primitiveType == PrimitiveType.Capsule? _characters : _enemies;
            if (objectDict.ContainsKey(id))
                return objectDict[id];
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.transform.position = pos;
            gameObject.GetComponent<Renderer>().material.color = color;
            if (addCharacterController)
                gameObject.AddComponent<CharacterController>();
            objectDict.Add(id, gameObject);
            return gameObject;
        }

        protected GameObject SpawnCharacter(byte id, Color objectColor, bool addCharacterController = false)
        {
            GameObject capsule = SpawnObject(id, PrimitiveType.Capsule,
                new Vector3(0, 1.1f, 0), objectColor, addCharacterController);
            capsule.tag = "Player";
            return capsule;
        }

<<<<<<< Updated upstream
        protected HashSet<byte> AttackNPCsNearPoint(Vector3 transformPosition)
        {
            HashSet<byte> deletedIds = new HashSet<byte>();
            foreach (KeyValuePair<byte, GameObject> enemy in _enemies)
            {
                if (Vector3.Distance(transformPosition, enemy.Value.transform.position) < 10.0)
                {
                    deletedIds.Add(enemy.Key);
                }
            }

            foreach (byte id in deletedIds)
            {
                DestroyGameObject(id, false);
=======
        protected HashSet<byte> DeleteAllNPCs()
        {
            HashSet<byte> deletedIds = new HashSet<byte>();
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (!_gameObjects[i])
                {
                    continue;
                }
                if (_gameObjectTypes[i] == (byte)PrimitiveType.Cylinder)
                {
                    _logger.Log("Deleting Cylinder: " + i);
                    deletedIds.Add((byte) i);
                    Object.Destroy(_gameObjects[i]);
                    _gameObjects[i] = null;
                    _gameObjectTypes[i] = 0;
                    _gameObjectsCount--;
                }
>>>>>>> Stashed changes
            }

            return deletedIds;
        }
<<<<<<< Updated upstream

        protected void DestroyGameObject(byte id, bool isChar)
        {
            Dictionary<byte, GameObject> objectDict = isChar ? _characters : _enemies;
            if (objectDict.ContainsKey(id))
            {
                Object.Destroy(objectDict[id]);
                objectDict.Remove(id);
            }
        }
=======
>>>>>>> Stashed changes
    }
}