using System;
using System.Text.RegularExpressions;
using NordChecker.Models.Domain;

namespace NordChecker.Services
{
    public class Parser
    {
        public string RegexMask;

        public Parser(string regexMask) =>
            RegexMask = regexMask;

        public Account Parse(string credentials)
        {
            Match match = Regex.Match(credentials, RegexMask);
            if (match.Success)
                return new Account(match.Groups[1].Value, match.Groups[2].Value);
            throw new InvalidOperationException();
        }
    }
}
