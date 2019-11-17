using System.Collections.Generic;
using Connections.Loggers;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace WorldManagement
{
    public class ClientWorldController : WorldController
    {
        protected bool _clientSetCharacter = false;
        protected FramesStorer _framesStorer;
        protected HashSet<byte> deletedIds = new HashSet<byte>();
        protected byte _playerId;

        public ClientWorldController(FramesStorer framesStorer, ClientLogger logger) : base(logger)
        {
            _framesStorer = framesStorer;
        }

        public void SpawnPlayer(byte id, Color color)
        {
            SpawnCharacter(id, color);
            _playerId = id;
            _clientSetCharacter = true;
            _framesStorer.SetCharId(id);
        }

        public bool ClientSetPlayer()
        {
            return _clientSetCharacter;
        }

        public void PredictMovePlayer( Vector3 movement, int packetSize)
        {
            Vector3 newPos = _gameObjects[_playerId].transform.position + movement * _movementSpeed;
            byte[] predictedPositions = GetPositions((byte)(_framesStorer.CurrentSnapshotId()+3));
            Utils.Vector3ToByteArray(newPos, predictedPositions, _playerId*UnreliableStream.PACKET_SIZE+3);
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
                    if(!deletedIds.Contains(id))
                    {
                        PrimitiveType primitiveType = (PrimitiveType)snapshot[j+1];
                        Vector3 pos = Utils.ByteArrayToVector3(snapshot, j+2);
                        SpawnObject(id, primitiveType, pos, Color.yellow);
                    }
                    j += UnreliableStream.PACKET_SIZE;
                }
                else
                {
                    j+=2;
                    _gameObjects[i].transform.position = Utils.ByteArrayToVector3(snapshot, j);
                    PrimitiveType objectType = (PrimitiveType)_gameObjectTypes[i];
                    if (_gameObjects[_playerId] && objectType.Equals(PrimitiveType.Cylinder) &&
                        _gameObjects[_playerId].transform.position.Equals(_gameObjects[i].transform.position))
                    {
                        _logger.Log("You lost :(");
                        DestroyGameObject(_playerId);
                        Application.Quit();
                         Time.timeScale = 0; 
                    }
                    j += 12;
                }
            }
        }

        public void PlayerAttacked()
        {
//            deletedIds.UnionWith(DeleteAllNPCs());
            deletedIds.UnionWith(AttackNPCsNearPoint(_gameObjects[_playerId].transform.position));
        }

        public Vector3 GetPlayerPosition()
        {
            return _gameObjects[_playerId].transform.position;
        }
    }
}