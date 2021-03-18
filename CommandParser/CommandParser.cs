using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandTools
{
    public class CommandParser
    {

        public Dictionary<string, string[]> ParseCommand(Type objType, string args)
        {
            var props=objType.GetProperties();

            var commands=props.Where(x => x.GetCustomAttributes(typeof(CommandParserAttribute),true).Any() ).Select(x=>x.Name).ToArray();

            if (!commands.Any()) return null;
            
            return ParseCommand(commands,args);
            
        }

        public Dictionary<string, string[]> ParseCommand(string [] commands,string args)
        {
            return ParseCommand(commands, args.Split(' ').Where(x=>!string.IsNullOrEmpty(x)).ToArray());
        }


        private void SafeAddDictionary(Dictionary<string,string[]>dictionary,string key,string[] strings)
        {
            

            if (dictionary.ContainsKey(key))
            {
                var argList = new List<string>();
                argList.AddRange(dictionary[key]);
                argList.AddRange(strings);
                dictionary[key] = argList.ToArray();

            }

            else
            {
                dictionary.Add(key, strings);
            }
        }

        public Dictionary<string, string[]> ParseCommand(string[] commands, string[] args)
        {
            var commandDict = new Dictionary<string, string[]>();
            var argList=new List<string>();
            var curCommand = "";

            foreach (var cmd in args)
            {
                if (commands.Contains(cmd))
                {
                    if (!string.IsNullOrWhiteSpace(curCommand))
                    {
                        SafeAddDictionary(commandDict, curCommand, argList.ToArray());

                        argList.Clear();
                    }

                    curCommand = cmd;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(curCommand))
                {
                    argList.Add(cmd);
                }

            }

            if (argList.Count > 0) SafeAddDictionary(commandDict, curCommand, argList.ToArray());
            

            return commandDict;

        }
    }
}
