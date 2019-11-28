using System;
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
        private GameObject _player;

        public ClientWorldController(FramesStorer framesStorer, ClientLogger logger) : base(logger)
        {
            _framesStorer = framesStorer;
        }

        public void SpawnPlayer(byte id, Color color)
        {
            _player = SpawnCharacter(id, color, true);
            _playerId = id;
            _clientSetCharacter = true;
        }

        public bool ClientSetPlayer()
        {
            return _clientSetCharacter;
        }

        public void PredictMovePlayer(InputPackage inputPackage, int packetSize)
        {
            _framesStorer.PredictMovement(inputPackage, _playerId);
        }

        // Delegate methods
        public Frame GetNextFrame()
        {
            return _framesStorer.GetNextFrame();
        }

        public void StoreFrame(byte[] message)
        {
            _framesStorer.StoreFrame(message, _playerId);
        }
        
        public void UpdatePositions()
        {
            Frame frame = GetNextFrame();
            if (frame == null)
                return;
            foreach (KeyValuePair<byte, Vector3> enemy in frame.GetEnemies())
            {
                if (_enemies.ContainsKey(enemy.Key))
                {
                    _enemies[enemy.Key].transform.position = enemy.Value;
                }
            }
            foreach (KeyValuePair<byte, Vector3> character in frame.GetCharacters())
            {
                if (_characters.ContainsKey(character.Key))
                {
                    if (character.Key != _playerId)
                    {
                        _characters[character.Key].transform.position = character.Value;
                    }
                    else
                    {
                        CharacterController cc = _player.GetComponent<CharacterController>();
                        cc.Move(character.Value - cc.transform.position);
                    }
                }
            }
        }

        public Vector3 GetPlayerPosition()
        {
            // TODO: Change when I implement player as something appart from this.
            return _characters[_playerId] ? _characters[_playerId].transform.position : Vector3.zero;
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
            SpawnObject(objId, primitiveType, Vector3.back, primitiveType == PrimitiveType.Capsule ? Color.red : Color.magenta);
        }

        public void PlayerAttacked()
        {
            HashSet<byte> enemiesToDelete = AttackNPCsNearPoint(_player.GetComponent<CharacterController>().transform.position);
            foreach (byte id in enemiesToDelete)
            {
                ObjectsToDestroy.Enqueue(new Tuple<byte, PrimitiveType>(id, PrimitiveType.Cylinder));
            }
        }

        public Queue<Tuple<byte, PrimitiveType>> GetEnemiesToDestroy()
        {
            return ObjectsToDestroy;
        }
    }
}