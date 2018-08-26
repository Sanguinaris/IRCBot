using System;
using TwitchBot.Networking.Socket.impls;
using TwitchBot.Protocol;
using TwitchBot.Utils.CommandLine;

namespace TwitchBot
{
    class Program
    {
        static void Main(string[] args)
        {
            IRCProtocol.Init();
            CommandLineParser.Init();
            if (!CommandLineParser.ParseArgs(args))
                return;
            BaseTCPSocket sock = new BaseTCPSocket();
            IRCProtocol irc = new IRCProtocol(sock);
            irc.Run();
            Console.WriteLine("All gucci");
        }
    }
}
