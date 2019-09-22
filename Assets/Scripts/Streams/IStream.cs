using System.Collections.Generic;

namespace Streams
{
    public interface IStream<T>
    {
        // Called by whoever intends to send a message using this stream (like Server or Client)
        void SendMessage(byte[] data);

        // Called by whoever intends to receive a message using this stream (like Server or Client)
        IList<(byte[], T)> ReceiveMessages();
        
        // Called by whoever takes care of sending the messages this stream has to send (like PacketProcessor)
        IList<byte[]> GetPendingMessagesForSend();

        // Called by whoever takes care of receiving the messages for this stream (like PacketProcessor)
        void Give(byte[] data, T metadata);

        // TODO: add meaningful comment
        byte GetId();
    }
}