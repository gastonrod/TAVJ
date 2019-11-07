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
                    writer.Write(message.id);
                    foreach (var playerInfo in message.PlayersInfo)
                    {
                        writer.Write(playerInfo.ClientId);
                        writer.Write(playerInfo.Position.x);
                        writer.Write(playerInfo.Position.y);
                        writer.Write(playerInfo.Position.z);
                        writer.Write(playerInfo.Rotation.x);
                        writer.Write(playerInfo.Rotation.y);
                        writer.Write(playerInfo.Rotation.z);
                        writer.Write(playerInfo.NextInputId);
                    }
                }
                return m.ToArray();
            }
        }

        public static SnapshotMessage DeserializeSnapshotMessage(byte[] message)
        {
            const int SERIALIZED_SIZE = sizeof(byte) + 2 * 3 * sizeof(float);
            int PLAYERS_COUNT = (message.Length - sizeof(int)) / SERIALIZED_SIZE;
            SnapshotMessage result = new SnapshotMessage {PlayersInfo = new SnapshotMessage.SinglePlayerInfo[PLAYERS_COUNT]};
            using (MemoryStream m = new MemoryStream(message))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.id = reader.ReadInt32();
                    for (int i = 0; i < PLAYERS_COUNT; i++)
                    {
                        result.PlayersInfo[i] = new SnapshotMessage.SinglePlayerInfo
                        {
                            ClientId = reader.ReadByte(),
                            Position = {x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle()},
                            Rotation = {x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle()},
                            NextInputId = reader.ReadInt32()
                        };
                    }
                }
            }
            return result;
        }

        public class SnapshotMessage
        {
            public int id;
            public SinglePlayerInfo[] PlayersInfo;
            
            public class SinglePlayerInfo
            {
                public byte ClientId;
                public Vector3 Position;
                public Quaternion Rotation;
                public int NextInputId;
            }
        }
    }
}