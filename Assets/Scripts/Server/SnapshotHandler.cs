using System;
using System.Collections.Generic;
using Protocols;
using UnityEngine;

namespace Server
{
    public class SnapshotHandler
    {
        private double _timeCounter = -1;
        private readonly double _snapshotTimeThreshold;
        private int _nextSnapshotId = 0;

        public SnapshotHandler(double tickrate)
        {
            _snapshotTimeThreshold = 1 / tickrate;
        }
        
        public void Update(Dictionary<byte, ClientInfo> clientsInfo, int joinedPlayersCount, int killedPlayersCount)
        {
            if (_timeCounter < 0)    // Never sent a snapshot
            {
                SendSnapshot(clientsInfo, joinedPlayersCount, killedPlayersCount);
                _timeCounter = 0;
            }
            else    // Already sent first snapshot
            {
                _timeCounter += Time.deltaTime;
                if (_timeCounter >= _snapshotTimeThreshold)
                {
                    _timeCounter %= _snapshotTimeThreshold;
                    SendSnapshot(clientsInfo, joinedPlayersCount, killedPlayersCount);
                }
            }
        }

        private void SendSnapshot(Dictionary<byte, ClientInfo> clientsInfo, int joinedPlayersCount, int killedPlayersCount)
        {
            GameProtocol.SnapshotMessage snapshotMessage = new GameProtocol.SnapshotMessage() {id = _nextSnapshotId++, PlayersInfo = new GameProtocol.SnapshotMessage.SinglePlayerInfo[joinedPlayersCount - killedPlayersCount]};
            int currentPlayerCount = 0;
            foreach (var clientInfo in clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined || !info.Alive) continue;
                var playerInfo = new GameProtocol.SnapshotMessage.SinglePlayerInfo {ClientId = clientId, NextInputId = info.NextInputId};
                var transform = info.PlayerTransform;
                playerInfo.Position = transform.position;
                playerInfo.Rotation = transform.rotation;
                snapshotMessage.PlayersInfo[currentPlayerCount++] = playerInfo;
            }
            Array.Sort(snapshotMessage.PlayersInfo, (playerInfo1, playerInfo2) => playerInfo1.ClientId - playerInfo2.ClientId);
            
            // Broadcast serialized world
            foreach (var clientInfo in clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined) continue;
                info.SnapshotStream.SendMessage(GameProtocol.SerializeSnapshotMessage(snapshotMessage));
            }
        }
    }
}