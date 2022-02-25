using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NordChecker.Models;
using NordChecker.Shared;
using Serilog;

namespace NordChecker.Services.AccountFormatter
{
    public class AccountFormatter
    {
        public string FormatScheme;
        private int _BakedFormatSchemeHash;

        public List<Placeholder> Placeholders;
        public AccountFormatter(string formatScheme)
        {
            FormatScheme = formatScheme;
            Placeholders = MakeDefaultPlaceholders();
        }

        private static List<Placeholder> MakeDefaultPlaceholders()
        {
            return new List<Placeholder>
            {
                new("email")
                {
                    Aliases = new[] { "mail", "login", "e", "m", "l" },
                    Binding = x => x.Email
                },
                new("password")
                {
                    Aliases = new[] { "pass", "p" },
                    Binding = x => x.Password
                },
                new("proxy")
                {
                    Aliases = new[] { "ip" },
                    Binding = x => x.Proxy?.ToString()
                },
                new("expiration")
                {
                    Aliases = new[] { "expire", "date", "time", "exp" },
                    Binding = x => x.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss")
                },
                new("token")
                {
                    Aliases = new[] { "t" },
                    Binding = x => x.Token
                },
                new("renew-token")
                {
                    Aliases = new[] { "r", "renew", "renew_token" },
                    Binding = x => x.RenewToken
                },
                new("state")
                {
                    Aliases = new[] { "s", "type", "kind", "stage" },
                    Binding = x => x.State.ToString()
                },
                new("json")
                {
                    Aliases = new[] { "j", "raw", "info", "data", "serial", "serialized" },
                    Binding = x => JsonConvert.SerializeObject(x)
                },
                new("new-line")
                {
                    Aliases = new[] { "nl", "newline" },
                    Binding = x => "\n"
                }
            };
        }

        public string Format(Account account)
        {
            BakeCurrentFormatSchemeIfNeeded();

            StringBuilder builder = new(Regex.Unescape(FormatScheme));
            foreach (var placeholder in Placeholders)
            {
                string value = placeholder.Binding(account);
                builder.Replace("{" + placeholder.Key + "}", value);
            }

            return builder.ToString();
        }

        private void BakeCurrentFormatSchemeIfNeeded()
        {
            if (FormatScheme.GetHashCode() != _BakedFormatSchemeHash)
                BakeCurrentFormatScheme();
        }

        private void BakeCurrentFormatScheme()
        {
            StringBuilder builder = new(FormatScheme);
            foreach (var placeholder in Placeholders)
            {
                foreach (var alias in placeholder.Aliases)
                    builder.Replace("{" + alias + "}", "{" + placeholder.Key + "}");
            }

            FormatScheme = builder.ToString();
            _BakedFormatSchemeHash = FormatScheme.GetHashCode();
        }
    }
}
