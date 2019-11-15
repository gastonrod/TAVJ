using System;
using System.Collections.Generic;
using DefaultNamespace;
using Protocols;
using UnityEngine;
using static UnityEngine.Object;
using static Protocols.GameProtocol;

namespace Client
{
    public class SnapshotHandler
    {
        private readonly double _snapshotIntervalInSeconds;
        private readonly GameObject _playerPrefab;
        private IDictionary<byte, PlayerInfo> _players;
        private CharacterController _playerCharacterController;

        private SnapshotMessage _fromSnapshot;
        private SnapshotMessage _toSnapshot;
        private SnapshotMessage _nextSnapshot;
        private double _currentDeltaTime;
        private int _highestSnapshotId = -1;
        private bool _isInterpolationRunning;

        private bool _isClientIdSet = false;
        private byte _clientId = 0;

        private bool _isPlayerAlive = true;

        private Queue<MovementProtocol.MovementMessage> _inputQueue;

        public SnapshotHandler(GameObject playerPrefab, IDictionary<byte, PlayerInfo> players, double tickrate, Queue<MovementProtocol.MovementMessage> inputQueue)
        {
            _snapshotIntervalInSeconds = 1 / tickrate;
            _playerPrefab = playerPrefab;
            _players = players;
            _fromSnapshot = _toSnapshot = _nextSnapshot = null;
            _currentDeltaTime = -1;
            _isInterpolationRunning = false;
            _inputQueue = inputQueue;
        }

        public void SetClientInfo(byte clientId, CharacterController playerCharacterController)
        {
            _isClientIdSet = true;
            _clientId = clientId;
            _playerCharacterController = playerCharacterController;
        }
        
        public void AddSnapshot(SnapshotMessage snapshotMessage)
        {
            if (snapshotMessage.id < _highestSnapshotId) return;
            if (_fromSnapshot == null)
            {
                _fromSnapshot = snapshotMessage;
            }
            else if (_toSnapshot == null)
            {
                _toSnapshot = snapshotMessage;
            }
            else
            {
                _nextSnapshot = snapshotMessage;
            }
            _highestSnapshotId = snapshotMessage.id;
        }

        public void Update()
        {
            if (!_isPlayerAlive) return;
            _currentDeltaTime = (_isInterpolationRunning ? _currentDeltaTime + Time.deltaTime : 0);
            if (_fromSnapshot != null && _toSnapshot != null)
            {
                _isInterpolationRunning = true;
                double currentSnapshotIntervalInSeconds =
                    _snapshotIntervalInSeconds * (_toSnapshot.id - _fromSnapshot.id);
                if (_currentDeltaTime > currentSnapshotIntervalInSeconds)
                {
                    _fromSnapshot = _toSnapshot;
                    _toSnapshot = _nextSnapshot;
                    _currentDeltaTime %= currentSnapshotIntervalInSeconds;
                    _nextSnapshot = null;
                }
                if (_toSnapshot != null)
                {
                    RenderInterpolatedSnapshot();
                }
                else
                {
                    _isInterpolationRunning = false;
                }
            }

            RenderPrediction();
        }

        public bool IsPlayerAlive()
        {
            return _isPlayerAlive;
        }
        
        private void RenderInterpolatedSnapshot()
        {
            double currentSnapshotIntervalInSeconds =
                _snapshotIntervalInSeconds * (_toSnapshot.id - _fromSnapshot.id);
            int fromSnapshotIndex = 0;
            int toSnapshotIndex = 0;
            while (fromSnapshotIndex < _fromSnapshot.PlayersInfo.Length)
            {
                // Skip players that are in toSnapshot but not in fromSnapshot
                while (toSnapshotIndex < _toSnapshot.PlayersInfo.Length &&
                       _toSnapshot.PlayersInfo[toSnapshotIndex].ClientId <
                       _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId) toSnapshotIndex++;
                if (toSnapshotIndex >= _toSnapshot.PlayersInfo.Length) return;
                
                if (_fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId ==
                    _toSnapshot.PlayersInfo[toSnapshotIndex].ClientId)    // Found player in both snapshots
                {
                    if (_isClientIdSet && _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId != _clientId)
                    {
                        var currentPlayerClientId = _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId;
                        var fromPosition = _fromSnapshot.PlayersInfo[fromSnapshotIndex].Position;
                        var toPosition = _toSnapshot.PlayersInfo[toSnapshotIndex].Position;
                        var position = new Vector3(
                            Interpolate(fromPosition.x, toPosition.x, _currentDeltaTime,
                                currentSnapshotIntervalInSeconds),
                            Interpolate(fromPosition.y, toPosition.y, _currentDeltaTime,
                                currentSnapshotIntervalInSeconds),
                            Interpolate(fromPosition.z, toPosition.z, _currentDeltaTime,
                                currentSnapshotIntervalInSeconds));
                        var rotation = Quaternion.Lerp(_fromSnapshot.PlayersInfo[fromSnapshotIndex].Rotation, _toSnapshot.PlayersInfo[fromSnapshotIndex].Rotation, (float) (_currentDeltaTime / currentSnapshotIntervalInSeconds));
                        bool foundCurrentPlayer =
                            _players.TryGetValue(currentPlayerClientId, out PlayerInfo playerInfo);
                        if (foundCurrentPlayer)
                        {
                            playerInfo.PlayerTransform.SetPositionAndRotation(position, rotation);
                        }
                        else
                        {
                            GameObject currentPlayerGameObject = Instantiate(_playerPrefab, position, rotation);
                            currentPlayerGameObject.GetComponent<CharacterController>().enabled = false;
                            currentPlayerGameObject.GetComponent<ClientIdHolder>().SetClientId(currentPlayerClientId);
                            _players.Add(currentPlayerClientId, new PlayerInfo() {PlayerGameObject = currentPlayerGameObject, PlayerTransform = currentPlayerGameObject.GetComponent<Transform>()});
                        }
                    }
                    fromSnapshotIndex++;
                }
                else    // Player left
                {
                    while (fromSnapshotIndex < _fromSnapshot.PlayersInfo.Length &&
                           _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId <
                           _toSnapshot.PlayersInfo[toSnapshotIndex].ClientId)
                    {
                        byte leftPlayerId = _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId;
                        bool foundPlayerThatLeft = _players.TryGetValue(leftPlayerId, out PlayerInfo leftPlayerInfo);
                        if (foundPlayerThatLeft && leftPlayerInfo.Alive)
                        {
                            leftPlayerInfo.Alive = false;
                            Destroy(leftPlayerInfo.PlayerGameObject);
                        }
                        fromSnapshotIndex++;
                    }
                }
            }
        }

        private void RenderPrediction()
        {
            SnapshotMessage snapshot = GetLatestSnapshot();
            if (snapshot != null)
            {
                SnapshotMessage.SinglePlayerInfo info = GetInfoFromSnapshotForPlayer(snapshot, _clientId);
                if (info == null)
                {
                    _isPlayerAlive = false;
                    return;
                }
                while (_inputQueue.Count > 0 && _inputQueue.Peek().id < info.NextInputId) _inputQueue.Dequeue();    // Discard inputs that have already been applied
                bool foundTransform = _players.TryGetValue(_clientId, out PlayerInfo playerInfo);
                if (!foundTransform) throw new Exception("Failed to get current player's transform");
                _playerCharacterController.enabled = false;
                playerInfo.PlayerTransform.position = info.Position;
                _playerCharacterController.enabled = true;
                foreach (var message in _inputQueue)
                {
                    Quaternion rotation = new Quaternion(0, message.horizontalRotation, 0, message.scalarRotation);
                    Vector3 forward = rotation * Vector3.forward;
                    Vector3 right = rotation * Vector3.right;
                    Vector3 directionVector = PlayerMovementCalculator.GetVectorDirectionFromMovementDirectionAndRotation(message.direction, forward, right);
                    Vector3 movementDeltaVector = PlayerMovementCalculator.CalculateDelta(directionVector, Time.fixedDeltaTime);
                    _playerCharacterController.Move(movementDeltaVector);
                }
            }
        }

        private float Interpolate(float x1, float x2, double t, double tMax)
        {
            return (float) (x1 + (x2 - x1) * t / tMax);
        }

        private SnapshotMessage GetLatestSnapshot()
        {
            if (_nextSnapshot != null) return _nextSnapshot;
            if (_toSnapshot != null) return _toSnapshot;
            return _fromSnapshot;
        }

        private SnapshotMessage.SinglePlayerInfo GetInfoFromSnapshotForPlayer(SnapshotMessage snapshot, byte clientId)
        {
            for (int i = 0; i < snapshot.PlayersInfo.Length; i++)
            {
                if (snapshot.PlayersInfo[i].ClientId == clientId) return snapshot.PlayersInfo[i];
            }
            return null;
        }
    }
}