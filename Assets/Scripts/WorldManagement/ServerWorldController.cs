<<<<<<< Updated upstream
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Connections.Loggers;
using Connections.Streams;
=======
﻿using System.Collections.Generic;
using Connections.Loggers;
>>>>>>> Stashed changes
using DefaultNamespace;
using UnityEngine;
using Random = System.Random;

namespace WorldManagement
{
    public class ServerWorldController : WorldController
    {
        private byte _lastEnemyId = 0;
        private byte _lastCharacterId = 0;
        private int _xRange;
        private int _zRange;
        private int _spawnRate;
        private Random _random = new Random();
        private Material _enemiesMaterial;
<<<<<<< Updated upstream
        private Dictionary<byte, EnemyController> enemyControllers = new Dictionary<byte, EnemyController>();
        private Queue<Tuple<byte, PrimitiveType>> objectsToDestroy = new Queue<Tuple<byte, PrimitiveType>>();
        private Queue<Tuple<byte, PrimitiveType>> objectsToCreate = new Queue<Tuple<byte, PrimitiveType>>();
        private int _spawnRateTick = 0;

        public ServerWorldController(int spawnRate, ServerLogger logger) : base(logger)
=======
        private Dictionary<byte, EnemyController> enemies = new Dictionary<byte, EnemyController>();

        public ServerWorldController(ServerLogger logger) : base(logger)
>>>>>>> Stashed changes
        {
            Vector3 planeScale = GameObject.FindWithTag("Platform").transform.localScale*5;
            _xRange = (int)(planeScale.x * 2);
            _zRange = (int)(planeScale.z * 2);
            _spawnRate = spawnRate;
        }

        public byte SpawnCharacter()
        {
            SpawnCharacter(_lastCharacterId, Color.white, true);
            objectsToCreate.Enqueue(new Tuple<byte, PrimitiveType>(_lastCharacterId, PrimitiveType.Capsule));
            return _lastCharacterId++;
        }

        public byte GetMovementSpeed()
        {
            return _movementSpeed;
        }

        public void MoveObject(byte id, Vector3 movement, bool isCharacter)
        {
            Dictionary<byte, GameObject> objectDict = isCharacter ? _characters : _enemies;
            objectDict[id].GetComponent<CharacterController>().Move(movement);
        }

        public void SpawnEnemy()
        {
            Vector3 pos = new Vector3(_random.Next(_xRange) - _xRange/2 , 1.1f, _random.Next(_zRange) - _zRange/2);
            GameObject enemy = SpawnObject(_lastEnemyId, PrimitiveType.Cylinder, pos, Color.cyan, true);
            enemyControllers[_lastEnemyId] = new EnemyController(this, enemy, _lastEnemyId);
            objectsToCreate.Enqueue(new Tuple<byte, PrimitiveType>(_lastEnemyId, PrimitiveType.Cylinder));
            _lastEnemyId++;
        }

        public byte[] GetPositions(byte id)
        {
            return base.GetPositions(id);
        }

        public void Update()
        {
            foreach (KeyValuePair<byte, EnemyController> pair in enemyControllers)
            {
                _logger.Log("ID: update " + pair.Key);
                pair.Value.Update();
            }

//            if (++_spawnRateTick == _spawnRate)
//            {
//                SpawnEnemy();
//                _spawnRateTick = 0;
//            }
        }

        public void PlayerAttacked(byte playerId)
        {
            HashSet<byte> enemiesToDelete = AttackNPCsNearPoint(_characters[playerId].transform.position);
            _logger.Log("IDs to Delete: " + enemiesToDelete);
            foreach (byte id in enemiesToDelete)
            {
                enemyControllers.Remove(id);
                objectsToDestroy.Enqueue(new Tuple<byte, PrimitiveType>(id, PrimitiveType.Cylinder));
            }
        }

        public void MoveEnemy(byte id, Vector3 move, byte playerId)
        {
            if (!_characters.ContainsKey(playerId))
            {
                return;
            }
            Vector3 playerPos = _characters[playerId].transform.position;
            MoveObject(id, move, false);
            if (_enemies[id].transform.position.Equals(playerPos))
            {
                AttackPlayer(playerId);
            }
        }

        private void AttackPlayer(byte playerId)
        {
            DestroyGameObject(playerId, true);
            objectsToDestroy.Enqueue(new Tuple<byte, PrimitiveType>(playerId, PrimitiveType.Capsule));
        }

        public Queue<Tuple<byte, PrimitiveType>> ObjectsToDestroy()
        {
            Queue<Tuple<byte, PrimitiveType>> toReturn = objectsToDestroy;
            objectsToDestroy = new Queue<Tuple<byte, PrimitiveType>>();
            return toReturn;
        }
        public Queue<Tuple<byte, PrimitiveType>> ObjectsToCreate()
        {
            Queue<Tuple<byte, PrimitiveType>> toReturn = objectsToCreate;
            objectsToCreate = new Queue<Tuple<byte, PrimitiveType>>();
            return toReturn;
        }

        public Dictionary<byte, GameObject> GetCharacters()
        {
            return _characters;
        }

        public void PlayerAttacked(byte player_id)
        {
            DeleteAllNPCs();
            enemies = new Dictionary<byte, EnemyController>();
        }
    }
}