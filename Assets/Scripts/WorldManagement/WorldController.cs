using System;
using System.Collections.Generic;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;
using Object = UnityEngine.Object;

namespace WorldManagement
{
    public abstract class WorldController
    {
        protected Dictionary<byte, GameObject> _characters = new Dictionary<byte, GameObject>();
        protected Dictionary<byte, GameObject> _enemies = new Dictionary<byte, GameObject>();
        protected Queue<Tuple<byte, PrimitiveType>> ObjectsToDestroy = new Queue<Tuple<byte, PrimitiveType>>();
        protected Queue<Tuple<byte, PrimitiveType>> ObjectsToCreate = new Queue<Tuple<byte, PrimitiveType>>();
        protected byte[] characterLastInputIds = new byte[byte.MaxValue];
        protected byte _movementSpeed = 1;
        protected ILogger _logger;

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
                positions[j++] = 0;
                Utils.Vector3ToByteArray(enemy.Value.transform.position, positions, j);
                j += 12;
            }
            foreach (KeyValuePair<byte, GameObject> character in _characters)
            {
                positions[j++] = character.Key;
                positions[j++] = (byte) PrimitiveType.Capsule;
                positions[j++] = characterLastInputIds[character.Key];
                Utils.Vector3ToByteArray(character.Value.transform.position, positions, j);
                j += 12;
            }

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
            }

            return deletedIds;
        }

        protected void DestroyGameObject(byte id, bool isChar)
        {
            Dictionary<byte, GameObject> objectDict = isChar ? _characters : _enemies;
            if (objectDict.ContainsKey(id))
            {
                Object.Destroy(objectDict[id]);
                objectDict.Remove(id);
            }
        }
        
        public Queue<Tuple<byte, PrimitiveType>> GetObjectsToDestroy()
        {
            Queue<Tuple<byte, PrimitiveType>> toReturn = ObjectsToDestroy;
            ObjectsToDestroy = new Queue<Tuple<byte, PrimitiveType>>();
            return toReturn;
        }
    }
}