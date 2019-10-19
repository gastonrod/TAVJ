using System.Net;
using Connections;
using Connections.Streams;
using Interpolation;
using UnityEngine;

namespace DefaultNamespace
{
    public class WorldController
    {
        private GameObject[] _gameObjects = new GameObject[byte.MaxValue];
        private byte _gameObjectsCount = 0;
        private byte[] _gameObjectTypes = new byte[byte.MaxValue];
        private byte _lastGameObjectId = 0;
        private byte _movementSpeed = 1;
        private Vector3 _offset = Vector3.zero;
        private bool _clientSetCharacter = false;
        private FramesStorer _framesStorer;

        public WorldController(){}
        public WorldController(Vector3 offset, FramesStorer framesStorer)
        {
            _offset = offset;
            _framesStorer = framesStorer;
        }

        
        // SpawnCharacter called by server. Will spawn an object in the last spot in the array.
        public byte SpawnCharacter()
        {
            SpawnCharacter(_lastGameObjectId, Color.white);
            return _lastGameObjectId++;
        }
        
        
        // SpawnCharacter called by client. Will spawn an object where the server told the client his object is.
        public void SpawnCharacter(byte id, Color objectColor)
        {
             GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
             Vector3 pos = new Vector3(5, 0, 0);
             capsule.transform.position = pos;
             capsule.GetComponent<Renderer>().material.color = objectColor;
             _gameObjects[id] = capsule;
             _gameObjectTypes[id] = (byte)PrimitiveType.Capsule;
             _gameObjectsCount++;
        }

        
        public byte[] GetPositions(byte snapshotId, int packetSize)
        {
            byte[] positions = new byte[_gameObjectsCount * packetSize + 1];
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

        
        public void SetPositions(byte[] snapshot)
        {
            for (int j = 1; j < snapshot.Length;)
            {
                int i = snapshot[j];
                if (!_gameObjects[i])
                {
                    byte id = (byte)i;
                    PrimitiveType primitiveType = (PrimitiveType)snapshot[j+1];
                    Vector3 pos = Utils.ByteArrayToVector3(snapshot, j+2);
                    SpawnObject(id, primitiveType, pos);
                    j += UnreliableStream.PACKET_SIZE;
                }
                else
                {
                    j+=2;
//                    _gameObjects[i].transform.position = Utils.ByteArrayToVector3(snapshot, j) + _offset;
                    _gameObjects[i].transform.position = Utils.ByteArrayToVector3(snapshot, j);
                    j += 12;
                }
            }
        }
        
        private void SpawnObject(byte id, PrimitiveType primitiveType, Vector3 pos)
        {
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.transform.position = pos;
            _gameObjects[id] = gameObject;
        }
        
        
        public void MovePlayer(byte id, Vector3 movement)
        {
            _gameObjects[id].transform.position += movement * _movementSpeed;
        }

        public byte GetMovementSpeed()
        {
            return _movementSpeed;
        }


        public void SpawnClientPlayer(byte id, Color color)
        {
            SpawnCharacter(id, color);
            _clientSetCharacter = true;
        }

        public bool ClientSetPlayer()
        {
            return _clientSetCharacter;
        }

        public void PredictMovePlayer(byte id, Vector3 movement, int packetSize)
        {
            Vector3 newPos = _gameObjects[id].transform.position + movement * _movementSpeed;
            byte[] predictedPositions = GetPositions((byte)(_framesStorer.CurrentSnapshotId()+6), packetSize);
            Utils.Vector3ToByteArray(newPos, predictedPositions, 3);
            _framesStorer.StoreFrame(predictedPositions);
        }
    }
}