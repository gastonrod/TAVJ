using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Random = System.Random;

namespace Connections
{
    public class Connection
    {
        private readonly Queue<byte[]> messages = new Queue<byte[]>();
        private readonly UdpClient udpClient;


        public Connection(int port, int delayInMs = 0, int pktLossPct = 0)
        {
            udpClient = new UdpClient(port);
            var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Int16.MaxValue);
            new Thread(() =>
            {
                Random rand = new Random();
                while (true)
                {
                    byte[] receivedMessage = udpClient.Receive(ref RemoteIpEndPoint);
                    byte[] address = RemoteIpEndPoint.Address.GetAddressBytes();
                    byte[] message = new byte[receivedMessage.Length + 6];
                    /*
                     *  |     4B    |  2B  |    xB   |
                     *  | IPAddress | Port | Message |
                     */
                    Array.Copy(address, 0, message, 0, 4);
                    message[4] = (byte) RemoteIpEndPoint.Port;
                    message[5] = (byte) (RemoteIpEndPoint.Port>> 8);
                    Array.Copy(receivedMessage, 0, message, 6, receivedMessage.Length);
                    if (delayInMs != 0)
                    {
                        if (rand.Next(100) < pktLossPct)
                        {
                            continue;
                        }
                        Thread.Sleep(delayInMs);
                    }
                    lock (messages)
                    {
                        messages.Enqueue(message);
                    }
                }
            }).Start();
        }

        public void SendData(byte[] data, IPEndPoint ipEndPoint)
        {
            udpClient.Send(data, data.Length, ipEndPoint);
        }

        public byte[] ReceiveData()
        {
            lock (messages)
            {
                if (messages.Count > 0)
                    return messages.Dequeue();
                return null;
            }
        }
        
    }
}