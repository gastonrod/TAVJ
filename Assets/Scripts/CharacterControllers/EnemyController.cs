using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldManagement;

namespace DefaultNamespace
{
    public class EnemyController
    {

        private Dictionary<byte, GameObject> _players;
        private GameObject _me;
        private ServerWorldController _swc;
        private byte _id;

        private int _maxMove = 3;
        public EnemyController(ServerWorldController swc, GameObject me, byte id)
        {
            _swc = swc;
            _players = _swc.GetCharacters();
            _me = me;
            _id = id;
        }

        public void Update()
        {
            GameObject closestPlayer = null;
            byte closestPlayerId = byte.MaxValue;
            float minDist = int.MaxValue;
            foreach (KeyValuePair<byte, GameObject> playerPair in _players)
            {
                byte i = playerPair.Key;
                if (!_players[i])
                    continue;
                float dist = Math.Abs((_me.transform.position - playerPair.Value.transform.position).magnitude);
                if (dist < minDist)
                {
                    closestPlayer = playerPair.Value;
                    closestPlayerId = i;
                    minDist = dist;
                }
            }

            if (!closestPlayer)
                return;
            Vector3 move =  closestPlayer.transform.position - _me.transform.position;
            int x = Math.Abs(move.x) < Math.Abs(move.z) ? 0 : Math.Sign(move.x);
            int z = Math.Abs(move.z) < Math.Abs(move.x) ? 0 : Math.Sign(move.z);
            move.x = x;
            move.z = z;
            _swc.MoveEnemy(_id, move, closestPlayerId);
            _players = _swc.GetCharacters();
        }

    }
}