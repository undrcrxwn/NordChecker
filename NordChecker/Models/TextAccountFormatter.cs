using System.Collections.Generic;
using System.Text;

namespace NordChecker.Models
{
    public class TextAccountFormatter : IPayloadFormatter<Account, string>
    {
        private readonly ExportSettings _ExportSettings;

        public List<Placeholder> Placeholders = new()
        {
            new(new() { "email",       "mail"  }, x => x.Email             ?? "<unknown>"),
            new(new() { "password",    "pass"  }, x => x.Password          ?? "<unknown>"),
            new(new() { "proxy",       "ip"    }, x => x.Proxy?.ToString() ?? "<no-proxy>"),
            new(new() { "expiration",  "exp"   }, x => x.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss")),
            new(new() { "token"                }, x => x.Token             ?? "<unknown>"),
            new(new() { "renew_token", "renew" }, x => x.RenewToken        ?? "<unknown>")
        };

        public TextAccountFormatter(ExportSettings exportSettings) => _ExportSettings = exportSettings;
        
        public string Format(Account account)
        {
            StringBuilder builder = new(_ExportSettings.FormatScheme);
            foreach (var (keys, handler) in Placeholders)
            {
                string value = handler(account);
                foreach (var key in keys)
                    builder.Replace("{" + key + "}", value);
            }
            return builder.ToString();
        }

        string IPayloadFormatter<Account, string>.Format(Account account) => Format(account);
    }
}
