using System.Collections.Generic;

namespace EthicalLab.Domain
{
    public static class TerminalVocabulary
    {
        public static readonly string[] Commands =
        {
            "help", "scan", "map", "inspect", "simulate", "notes", "report", "clear"
        };

        public static bool IsKnown(string command)
        {
            if (string.IsNullOrEmpty(command)) return false;
            string token = command.Trim();
            int space = token.IndexOf(' ');
            if (space > 0) token = token.Substring(0, space);
            token = token.ToLowerInvariant();
            for (int i = 0; i < Commands.Length; i++)
                if (Commands[i] == token) return true;
            return false;
        }
    }
}
