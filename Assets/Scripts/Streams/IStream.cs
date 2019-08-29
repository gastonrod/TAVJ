using System.Collections.Generic;

namespace Streams
{
    public interface IStream
    {
        void SendMessage(byte[] data);

        IList<byte[]> GetPendingMessagesForSend();

        void Give(byte[] data);

        IList<byte[]> ReceiveMessages();
    }
}