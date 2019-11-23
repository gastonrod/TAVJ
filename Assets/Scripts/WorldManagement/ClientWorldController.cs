using System.Collections.Generic;
using Connections.Loggers;
using DefaultNamespace;
using UnityEngine;

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
        }

        public bool ClientSetPlayer()
        {
            return _clientSetCharacter;
        }

        public void PredictMovePlayer( Vector3 movement, int packetSize)
        {
//            Vector3 newPos = _gameObjects[_playerId].transform.position + movement * _movementSpeed;
//            byte[] predictedPositions = GetPositions((byte)(_framesStorer.CurrentSnapshotId()+3));
//            _logger.Log("playerId: " + _playerId + " length: " + predictedPositions.Length);
//            Utils.Vector3ToByteArray(newPos, predictedPositions, _playerId*UnreliableStream.PACKET_SIZE+3);
//            _framesStorer.StoreFrame(predictedPositions);
        }

        // Delegate methods
        public Frame GetNextFrame()
        {
            return _framesStorer.GetNextFrame();
        }

        public void StoreFrame(byte[] message)
        {
            _framesStorer.StoreFrame(message);
        }
        
        public void UpdatePositions()
        {
            Frame frame = GetNextFrame();
            if (frame == null)
                return;
            foreach (KeyValuePair<byte, Vector3> enemy in frame.GetEnemies())
            {
                if (enemies.ContainsKey(enemy.Key))
                {
                    enemies[enemy.Key].transform.position = enemy.Value;
                }
            }
            foreach (KeyValuePair<byte, Vector3> character in frame.GetCharacters())
            {
                if (characters.ContainsKey(character.Key))
                {
                    characters[character.Key].transform.position = character.Value;
                }
            }
        }

        public Vector3 GetPlayerPosition()
        {
            // TODO: Change when I implement player as something appart from this.
            return characters[_playerId] ? characters[_playerId].transform.position : Vector3.zero;
        }

        public void DestroyObject(byte charId, bool isChar)
        {
            DestroyGameObject(charId, isChar);
            if (!deletedIds.Contains(charId))
            {
                deletedIds.Add(charId);
            }
            if (_playerId == charId && isChar)
            {
                _logger.Log("You lost :(");
                DestroyGameObject(_playerId, isChar);
                Application.Quit();
                Time.timeScale = 0; 
            }
        }

        public void CreateObject(byte objId, PrimitiveType primitiveType)
        {
            _logger.Log("Creating object: " + objId + ", " + primitiveType);
            SpawnObject(objId, primitiveType, Vector3.back, primitiveType == PrimitiveType.Capsule ? Color.red : Color.magenta);
        }
    }
}