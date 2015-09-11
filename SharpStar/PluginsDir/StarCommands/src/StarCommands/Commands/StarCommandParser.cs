using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarCommands.Commands
{
    public class StarCommandParser
    {
        private static readonly Regex ParseRegex =
            new Regex(@"(\s?((""(?<quote>([^""]*))"")|((?<text>[^""\s]+))))", RegexOptions.Compiled);

        public static StarCommand ParseCommand(string fullCommand)
        {
            string[] split = fullCommand.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (!split.Any())
                return null;

            string command = split.First();
            
            if (command.StartsWith("/"))
                command = command.Substring(1);

            string[] args = split.Skip(1).ToArray();

            MatchCollection matches = ParseRegex.Matches(string.Join(" ", args));

            var joinedArgs = new List<string>();
            foreach (Match match in matches)
            {
                string arg;

                if (match.Groups["quote"].Success)
                    arg = match.Groups["quote"].Value;
                else if (match.Groups["text"].Success)
                    arg = match.Groups["text"].Value;
                else
                    continue;

                joinedArgs.Add(arg);
            }

            return new StarCommand
            {
                CommandName = command,
                Arguments = joinedArgs.ToArray()
            };
        }
    }
}
