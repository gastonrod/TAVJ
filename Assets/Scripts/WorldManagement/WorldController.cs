using System.Collections.Generic;
using Connections.Streams;
using Connections.Loggers;
using DefaultNamespace;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

namespace WorldManagement
{
    public abstract class WorldController
    {
        protected GameObject[] _gameObjects = new GameObject[byte.MaxValue];
        private byte _gameObjectsCount = 0;
        private byte[] _gameObjectTypes = new byte[byte.MaxValue];
        protected byte _movementSpeed = 1;
        protected ILogger _logger;

        protected WorldController(ILogger logger)
        {
            _logger = logger;
        }
        
        protected byte[] GetPositions(byte snapshotId)
        {
            byte[] positions = new byte[_gameObjectsCount * UnreliableStream.PACKET_SIZE + 1];
            positions[0] = snapshotId;
            for (int i = 0, j = 1; i < _gameObjects.Length; i++)
            {
                if (!_gameObjects[i])
                {
                    continue;
                }
                positions[j++] = (byte)i;
                positions[j++] = _gameObjectTypes[i];
                Utils.Vector3ToByteArray(_gameObjects[i].transform.position, positions, j);
                j += 12;
            }

            return positions;
        }

        protected GameObject SpawnObject(byte id, PrimitiveType primitiveType, Vector3 pos, Color color)
        {
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.transform.position = pos;
            gameObject.GetComponent<Renderer>().material.color = color;
            _gameObjects[id] = gameObject;
            _gameObjectTypes[id] = (byte) primitiveType;
            _gameObjectsCount++;
            return gameObject;
        }

        protected void SpawnCharacter(byte id, Color objectColor)
        {
            GameObject capsule = SpawnObject(id, PrimitiveType.Capsule, new Vector3(0, 1.1f, 0), objectColor);
            capsule.tag = "Player";
        }

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
                    DestroyGameObject(i);
                    deletedIds.Add((byte) i);
                }
            }

            return deletedIds;
        }
        
        protected HashSet<byte> AttackNPCsNearPoint(Vector3 transformPosition)
        {
            HashSet<byte> deletedIds = new HashSet<byte>();
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (!_gameObjects[i])
                {
                    continue;
                }
                if (_gameObjectTypes[i] == (byte)PrimitiveType.Cylinder &&
                    Vector3.Distance(transformPosition, _gameObjects[i].transform.position) < 2.0)
                {
                    _logger.Log("Deleting Cylinder: " + i);
                    DestroyGameObject(i);
                    deletedIds.Add((byte) i);
                }
            }

            return deletedIds;
        }

        protected void DestroyGameObject(int id)
        {
            Object.Destroy(_gameObjects[id]);
            _gameObjects[id] = null;
            _gameObjectTypes[id] = 0;
            _gameObjectsCount--;
        }
    }
}