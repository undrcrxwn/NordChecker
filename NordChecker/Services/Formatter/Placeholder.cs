using System;
using System.Collections.Generic;
using NordChecker.Models;

namespace NordChecker.Services.Formatter
{
    public record Placeholder(List<string> Keys, Func<Account, string> Handler);
}
