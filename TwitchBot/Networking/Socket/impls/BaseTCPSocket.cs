using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TwitchBot.Networking.Socket.impls
{
    class BaseTCPSocket : BaseSocket
    {

        public BaseTCPSocket()
        {
            client = new TcpClient();
        }

        public override void Connect(IPEndPoint ep)
        {
            client.BeginConnect(ep.Address, ep.Port, (ar) => {
                BaseTCPSocket sock = (BaseTCPSocket)ar.AsyncState;
                sock.client.EndConnect(ar);

                if (sock.client.Connected)
                {
                    var data = new byte[10000];
                    sock.client.GetStream().BeginRead(data, 0, 10000, (asyncResult) =>
                    {
                        BaseTCPSocket baseSock = (BaseTCPSocket)asyncResult.AsyncState;
                        baseSock.OnReceivedData(data);
                   }, sock);
                }
                sock.ActionDone(RegisterCallbackType.ConnectDone, sock.client.Connected);
            }, this);

        }

        private void OnReceivedData(byte[] data)
        {
            ActionDone(RegisterCallbackType.ReceivedData, data);
            data = new byte[10000];
            client.GetStream().BeginRead(data, 0, 10000, (asyncResult) =>
            {
                BaseTCPSocket baseSock = (BaseTCPSocket)asyncResult.AsyncState;
                baseSock.OnReceivedData(data);
            }, this);
        }

        public override void Send(byte[] data)
        {
            if (client.Connected)
            {
                client.GetStream().BeginWrite(data, 0, data.Length, (ar) =>
                {
                    BaseTCPSocket baseSock = (BaseTCPSocket)ar.AsyncState;
                    baseSock.ActionDone(RegisterCallbackType.SendDone, true);
                }, this);
            }
            else
            {
                ActionDone(RegisterCallbackType.SendDone, false);
            }
        }




        TcpClient client;
    }
}
