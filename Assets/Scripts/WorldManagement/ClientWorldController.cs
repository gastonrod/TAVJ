using Connections.Streams;
using DefaultNamespace;
using UnityEngine;

namespace WorldManagement
{
    public class ClientWorldController : WorldController
    {
        protected bool _clientSetCharacter = false;
        protected FramesStorer _framesStorer;

        public ClientWorldController(FramesStorer framesStorer)
        {
            _framesStorer = framesStorer;
        }

        public void SpawnPlayer(byte id, Color color)
        {
            SpawnCharacter(id, color);
            _clientSetCharacter = true;
            _framesStorer.SetCharId(id);
        }

        public bool ClientSetPlayer()
        {
            return _clientSetCharacter;
        }

        public void PredictMovePlayer(byte id, Vector3 movement, int packetSize)
        {
            Vector3 newPos = _gameObjects[id].transform.position + movement * _movementSpeed;
            byte[] predictedPositions = GetPositions((byte)(_framesStorer.CurrentSnapshotId()+3));
            Utils.Vector3ToByteArray(newPos, predictedPositions, id*UnreliableStream.PACKET_SIZE+3);
            _framesStorer.StoreFrame(predictedPositions);
        }

        // Delegate methods
        public byte[] GetNextFrame()
        {
            return _framesStorer.GetNextFrame();
        }

        public void StoreFrame(byte[] message)
        {
            _framesStorer.StoreFrame(message);
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
                    SpawnObject(id, primitiveType, pos, Color.yellow);
                    j += UnreliableStream.PACKET_SIZE;
                }
                else
                {
                    j+=2;
                    _gameObjects[i].transform.position = Utils.ByteArrayToVector3(snapshot, j);
                    j += 12;
                }
            }
        }
    }
}