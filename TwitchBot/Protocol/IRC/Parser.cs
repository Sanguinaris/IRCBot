using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchBot.Protocol.IRC
{
    class Parser
    {
        // General IRC messages look like diz: WHO WHAT WHERE :INFO
        
        public static List<Command> Parse(string data)
        {
            List<Command> commands = new List<Command>();
            foreach(var rawCommand in data.Split("\r\n"))
            {
                var rawParts = rawCommand.Split(' ');
                if (rawParts.Length < 1) continue;

                switch(rawParts[0])
                {
                    case "PING":
                        commands.Add(new Command(Commands.PING, null, null, rawCommand.Split(':')[1]));
                        continue;
                    case "NOTICE":
                        commands.Add(new Command(Commands.NOTICE, null, null, rawCommand));
                        continue;
                }

                if (rawParts.Length < 2) continue;

                //Prep the WHO
                rawParts[0] = rawParts[0].Remove(0, 1).Split('!')[0];
                var rawInfoArr = rawCommand.Split(':');

                switch (rawParts[1])
                {
                    case "PRIVMSG":
                        commands.Add(new Command(Commands.PRIVMSG, rawParts[0], new List<string>(rawParts[2].Split(',')), rawInfoArr[rawInfoArr.Length - 1]));
                        continue;

                    default:
                        if (rawParts[1].StartsWith('4') || rawParts[1].StartsWith('5'))
                            commands.Add(new Command(Commands.Error, rawParts[0], new List<string> { rawParts[1] }, rawInfoArr[rawInfoArr.Length - 1]));
                        else if(rawParts[1].StartsWith('2') || rawParts[1].StartsWith('3'))
                            commands.Add(new Command(Commands.Reply, rawParts[0], new List<string> { rawParts[1] }, rawInfoArr[rawInfoArr.Length - 1]));
                        else
                            commands.Add(new Command(Commands.UNKWN, rawParts[0], new List<string> { rawParts[1] }, rawCommand));
                        continue;
                }
            }

            return commands;
        }
    }
}
