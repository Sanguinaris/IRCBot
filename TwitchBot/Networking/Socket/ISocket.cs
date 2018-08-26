using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TwitchBot.Networking.Socket
{
    public enum RegisterCallbackType
    {
        ConnectDone,
        SendDone,
        ReceivedData,
        Error
    }

    public interface ISocket
    {
        void Connect(IPEndPoint ep);
        void Send(byte[] data);
        void RegisterCallback(RegisterCallbackType type, Action<object> callback);
    }
}
