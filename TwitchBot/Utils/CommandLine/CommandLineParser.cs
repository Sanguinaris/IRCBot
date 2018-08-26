using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchBot.Utils.CommandLine
{
    class CommandLineParser
    {
        /// <summary>
        /// Adds a argument to the arglist
        /// </summary>
        /// <returns>idx of the item in the list</returns>
        public static void AddCmdArg(CommandLineArg arg) { argList.Add(arg); }

        public static CommandLineArg GetCmdArg(int idx) { return argList[idx]; }
        public static List<CommandLineArg> GetCmdList() { return argList; }

        /// <summary>
        /// Will Parse the given arguments and bitch if sth is off
        /// </summary>
        /// <param name="args">Command line args</param>
        /// <returns>if the parsing was successfull (!malformed)</returns>
        public static bool ParseArgs(string[] args)
        {
            List<string> argsList = args.ToList();

            foreach (var arg in argList)
            {
                if (!arg.ParseArg(ref argsList))
                    return false;
            }

            return true;
        }

        #region Command Line Args
        static CommandLineArg HelpArg;
        static bool DoHelp(CommandLineArg x)
        {
            foreach (var arg in GetCmdList())
            {
                Console.WriteLine(arg.GetShortCommand() + " " + arg.GetLongCommand() + "\t" + arg.GetDescription());
            }
            return false;
        }

        public static void Init()
        {
            HelpArg = new CommandLineArg("-h", "--help", "Calls your mom");

            HelpArg.SetFinishedParsingCllbk(DoHelp);
        }
        #endregion

        private static List<CommandLineArg> argList = new List<CommandLineArg>();
    }
}
