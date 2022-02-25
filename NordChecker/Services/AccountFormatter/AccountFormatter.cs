using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NordChecker.Models;
using NordChecker.Shared;

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
                new("email") {
                    Aliases = new[] { "mail", "login", "m", "l" },
                    Binding = x => x.Email
                },
                new("password") {
                    Aliases = new[] { "pass", "p" },
                    Binding = x => x.Password
                },
                new("expiration") {
                    Aliases = new[] { "expire", "exp", "e" },
                    Binding = x => x.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss")
                },
                new("token") {
                    Aliases = new[] { "t" },
                    Binding = x => x.Token
                },
                new("renew-token") {
                    Aliases = new[] { "renew_token", "renew", "r" },
                    Binding = x => x.RenewToken
                },
                new("state") {
                    Aliases = new[] { "kind", "stage", "s" },
                    Binding = x => x.State.ToString()
                },
                new("json") {
                    Aliases = new[] { "info", "data", "raw", "serialized", "j" },
                    Binding = x => JsonConvert.SerializeObject(x)
                }
            };
        }

        public string Format(Account account)
        {
            BakeCurrentFormatSchemeIfNeeded();

            StringBuilder builder = new(FormatScheme.Unescape());
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
                    builder.Replace("{" + alias + "}", placeholder.Key);
            }
            FormatScheme = builder.ToString();
            _BakedFormatSchemeHash = FormatScheme.GetHashCode();
        }
    }
}
