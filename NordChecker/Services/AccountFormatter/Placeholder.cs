using System;
using System.Collections.Generic;
using NordChecker.Models;

namespace NordChecker.Services.AccountFormatter
{
    public class Placeholder
    {
        public string Key;
        public IEnumerable<string> Aliases;
        public Func<Account, string> Binding;

        public Placeholder(string key)
            : this(key, Array.Empty<string>(), x => throw new NotImplementedException()) { }

        public Placeholder(
            string key,
            IEnumerable<string> aliases,
            Func<Account, string> binding)
        {
            Key = key;
            Aliases = aliases;
            Binding = x => binding(x) ?? $"<no-{key}>";
        }
    }
}
