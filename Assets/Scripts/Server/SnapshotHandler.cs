﻿using System.Collections.Generic;
using Protocols;
using UnityEngine;

namespace Server
{
    public class SnapshotHandler
    {
        private double _timeCounter = -1;
        private const double _snapshotTimeThreshold = 0.1;

        public void Update(Dictionary<byte, ClientInfo> clientsInfo, int joinedPlayersCount)
        {
            if (_timeCounter < 0)    // Never sent a snapshot
            {
                SendSnapshot(clientsInfo, joinedPlayersCount);
                _timeCounter = 0;
            }
            else    // Already sent first snapshot
            {
                _timeCounter += Time.deltaTime;
                if (_timeCounter >= _snapshotTimeThreshold)
                {
                    _timeCounter %= _snapshotTimeThreshold;
                    SendSnapshot(clientsInfo, joinedPlayersCount);
                }
            }
        }

        private void SendSnapshot(Dictionary<byte, ClientInfo> clientsInfo, int joinedPlayersCount)
        {
            GameProtocol.SnapshotMessage snapshotMessage = new GameProtocol.SnapshotMessage() {PlayersInfo = new GameProtocol.SnapshotMessage.SinglePlayerInfo[joinedPlayersCount]};
            int currentPlayerCount = 0;
            foreach (var clientInfo in clientsInfo)
            {
                byte clientId = clientInfo.Key;
                ClientInfo info = clientInfo.Value;
                if (!info.Joined) continue;
                var playerInfo = new GameProtocol.SnapshotMessage.SinglePlayerInfo {ClientId = clientId};
                var transform = info.PlayerTransform;
                playerInfo.Position = transform.position;
                playerInfo.Rotation = transform.rotation;
                snapshotMessage.PlayersInfo[currentPlayerCount++] = playerInfo;
            }
            
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