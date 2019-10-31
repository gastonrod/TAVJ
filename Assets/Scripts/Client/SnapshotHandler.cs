using System.Collections.Generic;
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
        private IDictionary<byte, Transform> _players;

        private SnapshotMessage _fromSnapshot;
        private SnapshotMessage _toSnapshot;
        private SnapshotMessage _nextSnapshot;
        private double _currentDeltaTime;
        private int _highestSnapshotId = -1;
        private bool _isInterpolationRunning;

        public SnapshotHandler(GameObject playerPrefab, IDictionary<byte, Transform> players, double tickrate)
        {
            _snapshotIntervalInSeconds = 1 / tickrate;
            _playerPrefab = playerPrefab;
            _players = players;
            _fromSnapshot = _toSnapshot = _nextSnapshot = null;
            _currentDeltaTime = -1;
            _isInterpolationRunning = false;
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

                if (_fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId ==
                    _toSnapshot.PlayersInfo[toSnapshotIndex].ClientId)    // Found player in both snapshots
                {
                    var currentPlayerClientId = _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId;
                    var fromPosition = _fromSnapshot.PlayersInfo[fromSnapshotIndex].Position;
                    var toPosition = _toSnapshot.PlayersInfo[toSnapshotIndex].Position;
                    var position = new Vector3( Interpolate(fromPosition.x, toPosition.x, _currentDeltaTime, currentSnapshotIntervalInSeconds),
                                                Interpolate(fromPosition.y, toPosition.y, _currentDeltaTime, currentSnapshotIntervalInSeconds),
                                                Interpolate(fromPosition.z, toPosition.z, _currentDeltaTime, currentSnapshotIntervalInSeconds));
                    var rotation = _fromSnapshot.PlayersInfo[fromSnapshotIndex].Rotation;
                    bool foundCurrentPlayer = _players.TryGetValue(currentPlayerClientId, out Transform currentPlayerTransform);
                    if (foundCurrentPlayer)
                    {
                        currentPlayerTransform.SetPositionAndRotation(position, rotation);
                    }
                    else
                    {
                        GameObject currentPlayerGameObject = Instantiate(_playerPrefab, position, rotation);
                        currentPlayerGameObject.GetComponent<CharacterController>().enabled = false;
                        _players.Add(currentPlayerClientId, currentPlayerGameObject.GetComponent<Transform>());
                    }
                    fromSnapshotIndex++;
                }
                else    // Player left
                {
                    while (fromSnapshotIndex < _fromSnapshot.PlayersInfo.Length &&
                           _fromSnapshot.PlayersInfo[fromSnapshotIndex].ClientId <
                           _toSnapshot.PlayersInfo[toSnapshotIndex].ClientId)
                    {
                        // TODO: remove player
                        fromSnapshotIndex++;
                    }
                }
            }
        }

        private float Interpolate(float x1, float x2, double t, double tMax)
        {
            return (float) (x1 + (x2 - x1) * t / tMax);
        }
    }
}