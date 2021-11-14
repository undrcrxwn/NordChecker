using System;
using System.Collections.Generic;
using NordChecker.Models.Domain;

namespace NordChecker.Data
{
    public record Placeholder(List<string> Keys, Func<Account, string> Handler);
}
