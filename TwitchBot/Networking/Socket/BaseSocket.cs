using System;
using System.Collections.Generic;
using System.Net;

namespace TwitchBot.Networking.Socket
{
    /// <summary>
    /// Takes care of the callbacks
    /// </summary>
    class BaseSocket : ISocket
    {
        /// <summary>
        /// Ctor initializes callbacks dictionary
        /// </summary>
        public BaseSocket()
        {
            foreach(RegisterCallbackType type in Enum.GetValues(typeof(RegisterCallbackType)))
            {
                callbacks[type] = new List<Action<object>>();
            }
        }

        /// <summary>
        /// Connect to Server 
        /// (not supposed to end up in here therefore throwing)
        /// </summary>
        /// <param name="ep">Endpoint</param>
        public virtual void Connect(IPEndPoint ep)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send data to the server
        /// (not supposed to end up in here therefore throwing)
        /// </summary>
        /// <param name="data">Data</param>
        public virtual void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Registers a callback we can use to interact with other classes or so
        /// </summary>
        /// <param name="type">The type of the callback to register</param>
        /// <param name="callback">The callback to register</param>
        public void RegisterCallback(RegisterCallbackType type, Action<object> callback)
        {
            if (callback != null)
                callbacks[type].Add(callback);
        }


        /// <summary>
        /// Should be called once an action finishes to allow interaction with othr classes or so
        /// </summary>
        /// <param name="type">type of action</param>
        /// <param name="data">maybe data you want to pass</param>
        protected void ActionDone(RegisterCallbackType type, object data)
        {
            foreach(var call in callbacks[type])
            {
                call(data);
            }
        }

        Dictionary<RegisterCallbackType, List<Action<object>>> callbacks = new Dictionary<RegisterCallbackType, List<Action<object>>>();
    }
}
