using System.Collections.Generic;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;
using ILogger = Connections.Loggers.ILogger;

namespace WorldManagement
{
    public abstract class WorldController
    {
        protected Dictionary<byte, GameObject> characters = new Dictionary<byte, GameObject>();
        protected Dictionary<byte, GameObject> enemies = new Dictionary<byte, GameObject>();
        protected byte _movementSpeed = 1;
        protected ILogger _logger;

        protected WorldController(ILogger logger)
        {
            _logger = logger;
        }

        protected byte[] GetPositions(byte snapshotId)
        {
            int gameObjectsCount = (characters.Count + enemies.Count);
            byte[] positions = new byte[gameObjectsCount * UnreliableStream.PACKET_SIZE + 1];
            int j = 0;
            positions[j++] = snapshotId;
            foreach (KeyValuePair<byte, GameObject> enemy in enemies)
            {
                positions[j++] = enemy.Key;
                positions[j++] = (byte) PrimitiveType.Cylinder;
                Utils.Vector3ToByteArray(enemy.Value.transform.position, positions, j);
                j += 12;
            }
            foreach (KeyValuePair<byte, GameObject> character in characters)
            {
                positions[j++] = character.Key;
                positions[j++] = (byte) PrimitiveType.Capsule;
                Utils.Vector3ToByteArray(character.Value.transform.position, positions, j);
                j += 12;
            }

            _logger.Log("Positions: " + Utils.FrameToString(positions));
            return positions;
        }

        protected GameObject SpawnObject(byte id, PrimitiveType primitiveType, Vector3 pos, Color color)
        {
            Dictionary<byte, GameObject> objectDict = primitiveType == PrimitiveType.Capsule? characters : enemies;
            if (objectDict.ContainsKey(id))
                return objectDict[id];
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.transform.position = pos;
            gameObject.GetComponent<Renderer>().material.color = color;
            objectDict.Add(id, gameObject);
            return gameObject;
        }

        protected void SpawnCharacter(byte id, Color objectColor)
        {
            GameObject capsule = SpawnObject(id, PrimitiveType.Capsule, new Vector3(0, 1.1f, 0), objectColor);
            capsule.tag = "Player";
        }

        protected HashSet<byte> AttackNPCsNearPoint(Vector3 transformPosition)
        {
            HashSet<byte> deletedIds = new HashSet<byte>();
            foreach (KeyValuePair<byte, GameObject> enemy in enemies)
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
            Dictionary<byte, GameObject> objectDict = isChar ? characters : enemies;
            if (objectDict.ContainsKey(id))
            {
                Object.Destroy(objectDict[id]);
                objectDict.Remove(id);
            }
        }
    }
}