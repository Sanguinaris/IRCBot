using System;
using System.Collections.Generic;
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

        public IRCProtocol(ISocket sock)
        {   
            this.sock = sock;
        }

        public static void Init()
        {
            serverToConnect = new CommandLineArg("-s", "--server", "Server to connect to", 1);
            portToConnect = new CommandLineArg("-p", "--port", "Port to connect to", 1);
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

            sock.Send(System.Text.Encoding.Default.GetBytes("NICK SNAGULELELE \r\n"));
            sock.Send(System.Text.Encoding.Default.GetBytes("USER SNAGULELELE * *: fred von borg\r\n"));
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
                        Console.WriteLine($"UNKWN: {command.GetSender()} replied {command.GetParameters()[0]}!");
                        break;
                    case IRC.Commands.PING:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"PINGD: with additional args {command.GetAdditionalInfo()}");
                        sock.Send(Encoding.UTF8.GetBytes($"PONG :{command.GetAdditionalInfo()}\r\n"));
                        break;
                    case IRC.Commands.PRIVMSG:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"PRMSG: {command.GetSender()} ({string.Join(',',command.GetParameters())}): {command.GetAdditionalInfo()}");
                        break;
                    case IRC.Commands.NOTICE:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.WriteLine($"{command.GetAdditionalInfo()}");
                        break;

                }
            }
            Console.ForegroundColor = prevCol;

            return;
            var splitData = data.Split("\r\n");
            foreach (var command in splitData)
            {
                var commandArgs = command.Split(' ');

               // var prevCol = Console.ForegroundColor;
                switch (commandArgs[0])
                {
                    case "PING":
                        sock.Send(System.Text.Encoding.Default.GetBytes($"PONG {commandArgs[1]}\r\n"));
                        break;
                    default:
                        if(commandArgs.Length >= 4 && commandArgs[1] == "PRIVMSG")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{commandArgs[2]} {commandArgs[0]}{commandArgs[3]}");
                            Console.ForegroundColor = prevCol;
                        }
                        
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(command);
                        break;
                }
                Console.ForegroundColor = prevCol;
            }
        }

        protected virtual void OnSendDone()
        {
            var prevCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("ME6-SEND DONE");
            Console.ForegroundColor = prevCol;
        }
        
        ISocket sock;
    }
}
