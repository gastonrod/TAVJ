using System;
using System.IO;
using UnityEngine;

namespace Protocols
{
    public class GameProtocol
    {
        public static byte[] SerializeSnapshotMessage(SnapshotMessage message)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    foreach (var playerInfo in message.PlayersInfo)
                    {
                        writer.Write(playerInfo.ClientId);
                        writer.Write(playerInfo.Position.x);
                        writer.Write(playerInfo.Position.y);
                        writer.Write(playerInfo.Position.z);
                        writer.Write(playerInfo.Rotation.x);
                        writer.Write(playerInfo.Rotation.y);
                        writer.Write(playerInfo.Rotation.z);
                    }
                }
                return m.ToArray();
            }
        }

        public static SnapshotMessage DeserializeSnapshotMessage(byte[] message)
        {
            const int SERIALIZED_SIZE = sizeof(byte) + 2 * 3 * sizeof(float);
            int PLAYERS_COUNT = message.Length / SERIALIZED_SIZE;
            SnapshotMessage result = new SnapshotMessage {PlayersInfo = new SnapshotMessage.SinglePlayerInfo[PLAYERS_COUNT]};
            using (MemoryStream m = new MemoryStream(message))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    for (int i = 0; i < PLAYERS_COUNT; i++)
                    {
                        result.PlayersInfo[i].ClientId = reader.ReadByte();
                        result.PlayersInfo[i].Position.x = reader.ReadSingle();
                        result.PlayersInfo[i].Position.y = reader.ReadSingle();
                        result.PlayersInfo[i].Position.z = reader.ReadSingle();
                        result.PlayersInfo[i].Rotation.x = reader.ReadSingle();
                        result.PlayersInfo[i].Rotation.y = reader.ReadSingle();
                        result.PlayersInfo[i].Rotation.z = reader.ReadSingle();
                    }
                }
            }
            return result;
        }

        public class SnapshotMessage
        {
            public SinglePlayerInfo[] PlayersInfo;
            
            public class SinglePlayerInfo
            {
                public byte ClientId;
                public Vector3 Position;
                public Quaternion Rotation;
            }
        }
    }
}