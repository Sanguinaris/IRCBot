using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchBot.Protocol.IRC
{
    public enum Commands {
        UNKWN,
        Reply,
        Error,

        PRIVMSG,
        MOTD,
        MODE,

        PING,
        NOTICE,
    }

    class Command
    {
        public Command(Commands cmd, string sender, List<string> parameters, string info)
        {
            this.cmd = cmd;
            this.parameters = parameters;
            this.sender = sender;
            addInfo = info;
        }

        public string GetSender()
        {
            return sender;
        }

        public List<string> GetParameters()
        {
            return parameters;
        }

        public Commands GetCommand()
        {
            return cmd;
        }

        public string GetAdditionalInfo()
        {
            return addInfo;
        }


        string sender;
        List<string> parameters;
        string addInfo;
        Commands cmd;
    }
}
