using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace NordChecker.Models
{
    internal class Parser
    {
        public static Account? Parse(string credentials, string regexMask = @"\W*(^\w+(?:[-+.']\w+|-)*@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*):(\w+)\W*$")
        {
            Match match = Regex.Match(credentials, regexMask);
            if (match.Success)
                return new Account(match.Groups[1].Value, match.Groups[2].Value);
            throw new InvalidOperationException();
        }
    }
}
