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
        public static string CredentialsMask = @"\W*(^\w+(?:[-+.']\w+|-)*@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*):(\w+)\W*$";

        public static Account? Parse(string credentials)
        {
            Match match = Regex.Match(credentials, CredentialsMask);
            if (match.Success)
                return new Account(match.Groups[1].Value, match.Groups[2].Value);
            return null;
        }
    }
}
