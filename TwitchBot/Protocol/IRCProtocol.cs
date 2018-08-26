using System;
using System.Linq;
using System.Net;
using System.Text;
using TwitchBot.Networking.Socket;
using TwitchBot.Utils.CommandLine;

namespace TwitchBot.Protocol
{
    class IRCProtocol
    {
        private static CommandLineArg serverToConnect;
        private static CommandLineArg portToConnect;
        private static CommandLineArg channelToJoin;
        private static CommandLineArg clientName;
        private static CommandLineArg OAuthToken;

        enum ConnectionState
        {
            Connecting,
            Connected
        }

        public IRCProtocol(ISocket sock)
        {   
            this.sock = sock;
        }

        public static void Init()
        {
            serverToConnect = new CommandLineArg("-s", "--server", "Server to connect to", 1);
            portToConnect = new CommandLineArg("-p", "--port", "Port to connect to", 1);
            channelToJoin = new CommandLineArg("-c", "--channel", "Channel to join", 1);
            clientName = new CommandLineArg("-n", "--name", "Client Name", 1);

        }

        public void Run()
        {
            IPEndPoint endpoint;
            if (IPAddress.TryParse(serverToConnect.IsParsed() ? serverToConnect.GetAdditionalArg(0) : "91.217.189.21", out var addy))
                endpoint = new IPEndPoint(addy, portToConnect.IsParsed() ? int.Parse(portToConnect.GetAdditionalArg(0)) : 6667);
            else
                endpoint = new IPEndPoint(Dns.GetHostAddresses(serverToConnect.GetAdditionalArg(0))[0], int.Parse(portToConnect.GetAdditionalArg(0)));

            sock.RegisterCallback(RegisterCallbackType.ConnectDone, (o) => { OnConnected(); });
            sock.RegisterCallback(RegisterCallbackType.SendDone, (o) => { OnSendDone(); });
            sock.RegisterCallback(RegisterCallbackType.ReceivedData, (o) => { OnReceivedData(System.Text.Encoding.Default.GetString((byte[])o).Trim('\0')); });

            sock.Connect(endpoint);

            while (true)
            {
                var str = Console.ReadLine() + "\r\n";
                sock.Send(System.Text.Encoding.Default.GetBytes(str));
            }
        }

        protected virtual void OnConnected()
        {
            var prevCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("ME6-CONNECT DONE");
            Console.ForegroundColor = prevCol;
            sock.Send(System.Text.Encoding.Default.GetBytes($"NICK {(clientName.IsParsed() ? clientName.GetAdditionalArg(0) : "SNAGULELELE")}\r\n"));
            sock.Send(System.Text.Encoding.Default.GetBytes($"USER {(clientName.IsParsed() ? clientName.GetAdditionalArg(0) : "SNAGULELELE")} * *: fred von borg\r\n"));
        }

        protected virtual void OnReceivedData(string data)
        {

            var prevCol = Console.ForegroundColor;
            foreach (var command in IRC.Parser.Parse(data))
            {
                switch(command.GetCommand())
                {
                    case IRC.Commands.Error:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"ERROR: {command.GetSender()} encountered {command.GetParameters()[0]}!");
                        break;
                    case IRC.Commands.Reply:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"REPLY: {command.GetSender()} replied {command.GetParameters()[0]}!");
                        break;
                    case IRC.Commands.UNKWN:
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine($"UNKWN: {command.GetSender()} send us {command.GetAdditionalInfo()}!");
                        break;

                    case IRC.Commands.PING:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"PINGD: with additional args {command.GetAdditionalInfo()}");
                        sock.Send(Encoding.UTF8.GetBytes($"PONG :{command.GetAdditionalInfo()}\r\n"));
                        break;
                    case IRC.Commands.NOTICE:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.WriteLine($"{command.GetAdditionalInfo()}");
                        break;

                    case IRC.Commands.PRIVMSG:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"PRMSG: {command.GetSender()} ({string.Join(',',command.GetParameters())}): {command.GetAdditionalInfo()}");
                        if(!command.GetParameters().Contains(command.GetSender()))
                            sock.Send(Encoding.UTF8.GetBytes($"PRIVMSG {string.Join(',', command.GetParameters())} :{command.GetAdditionalInfo()}\r\n"));
                        break;
                    case IRC.Commands.MOTD:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"MOTD ({command.GetSender()}): {command.GetAdditionalInfo()}");
                        break;
                    case IRC.Commands.MODE:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"MODE ({command.GetParameters()[0]}): {string.Join(',',command.GetParameters().Skip(1))}");
                        if (State == ConnectionState.Connecting)
                            State = ConnectionState.Connected;
                        break;
                }
            }
            Console.ForegroundColor = prevCol;
        }

        protected virtual void OnSendDone()
        {
            var prevCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("ME6-SEND DONE");
            Console.ForegroundColor = prevCol;
        }

        protected virtual void OnEstablished()
        {
            var prevCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("ME6-ESTABLISHED DONE");
            Console.ForegroundColor = prevCol;

            if(channelToJoin.IsParsed())
            {
                sock.Send(Encoding.UTF8.GetBytes($"JOIN {channelToJoin.GetAdditionalArg(0)}\r\n"));
            }
        }
        
        ISocket sock;
        ConnectionState State {
            get
            {
                return state;
            }
            set
            {
                state = value;
                if (value == ConnectionState.Connected)
                    OnEstablished();
            } }
        ConnectionState state = ConnectionState.Connecting;
    }
}
