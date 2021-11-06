using System;
using System.Collections.Generic;

namespace NordChecker.Models
{
    public record Placeholder(List<string> Keys, Func<Account, string> Handler);
}
