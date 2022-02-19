using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Leaf.xNet;
using NordChecker.Models;
using NordChecker.Shared;

namespace NordChecker.Services
{
    public class InvalidMatchGroupCountException : Exception
    {
        public InvalidMatchGroupCountException(int expectedCount, int actualCount)
            : this($"{expectedCount} regex groups were expected but {actualCount} were given.") { }

        public InvalidMatchGroupCountException(int expectedCount, string[] matchGroups)
            : this($"{expectedCount} regex groups were expected but {matchGroups.Length} were given: " +
                  $"{string.Join(", ", matchGroups.Select(x => $"\"{x}\""))}.") { }

        public InvalidMatchGroupCountException(string message)
            : base(message) { }

        public InvalidMatchGroupCountException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class AccountParser
    {
        public string RegexPattern;

        public AccountParser(string regexPattern)
        {
            RegexPattern = regexPattern;
        }

        public Account Parse(string input)
        {
            string[] credentials = RegexHelper.Match(input, RegexPattern);
            if (credentials.Length != 3)
                throw new InvalidMatchGroupCountException(3, credentials);
            return new Account(credentials[1], credentials[2]);
        }
    }

    public class ProxyParser
    {
        public string RegexPattern;

        public ProxyParser(string regexPattern)
        {
            RegexPattern = regexPattern;
        }

        public Proxy Parse(string input, ProxyType proxyType)
        {
            string[] credentials = RegexHelper.Match(input, RegexPattern);
            if (credentials.Length != 2)
                throw new InvalidMatchGroupCountException(2, credentials);
            return Proxy.Parse(proxyType, $"{credentials[0]}:{credentials[1]}");
        }
    }

}
