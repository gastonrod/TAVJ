using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Connections
{
    public class Connection
    {
        private readonly Queue<byte[]> messages = new Queue<byte[]>();
        private readonly UdpClient udpClient;


        public Connection(int port, int pktLossPct)
        {
            udpClient = new UdpClient(port);
            var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Int16.MaxValue);
            new Thread(() =>
            {
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