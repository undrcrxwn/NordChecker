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

            Placeholders = new PlaceholderListBuilder()
                .AddPlaceholder("email").KnownAs("mail", "login", "m", "l")
                .BoundTo(x => x.Email ?? "<no-email>")
                .AddPlaceholder("password").KnownAs("pass", "p")
                .BoundTo(x => x.Password ?? "<no-password>")
                .AddPlaceholder("proxy").KnownAs("ip")
                .BoundTo(x => x.Proxy?.ToString() ?? "<no-proxy>")
                .AddPlaceholder("expiration").KnownAs("expire", "exp", "e")
                .BoundTo(x => x.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss"))
                .AddPlaceholder("token").KnownAs("t")
                .BoundTo(x => x.Token ?? "<no-token>")
                .AddPlaceholder("renew-token").KnownAs("renew token", "renewtoken", "renew", "r")
                .BoundTo(x => x.RenewToken ?? "<no-renew-token>")
                .AddPlaceholder("state").KnownAs("kind", "s")
                .BoundTo(x => x.State.ToString())
                .AddPlaceholder("json").KnownAs("info", "data", "raw", "serialized", "j")
                .BoundTo(x => JsonConvert.SerializeObject(x))
                .Build();
        }

        public string Format(Account account)
        {
            BakeFormatSchemeIfNeeded();

            StringBuilder builder = new(FormatScheme.Unescape());
            foreach (var placeholder in Placeholders)
            {
                string value = placeholder.Binding(account);
                builder.Replace("{" + placeholder.Key + "}", value);
            }

            return builder.ToString();
        }

        public void BakeFormatSchemeIfNeeded()
        {
            if (FormatScheme.GetHashCode() != _BakedFormatSchemeHash)
                BakeFormatScheme();
        }

        public void BakeFormatScheme()
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
