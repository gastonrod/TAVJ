using Connections.Streams;
using DefaultNamespace;
using UnityEngine;

namespace WorldManagement
{
    public abstract class WorldController
    {
        protected GameObject[] _gameObjects = new GameObject[byte.MaxValue];
        private byte _gameObjectsCount = 0;
        private byte[] _gameObjectTypes = new byte[byte.MaxValue];
        protected byte _movementSpeed = 1;

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

    }
}