using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using NordChecker.Models.Domain;
using NordChecker.Shared;

namespace NordChecker.Data
{
    public class AccountFormatter
    {
        public string FormatScheme;

        public List<Placeholder> Placeholders = new()
        {
            new(new() { "email",       "mail"  }, x => x.Email             ?? "<unknown>"),
            new(new() { "password",    "pass"  }, x => x.Password          ?? "<unknown>"),
            new(new() { "proxy",       "ip"    }, x => x.Proxy?.ToString() ?? "<no-proxy>"),
            new(new() { "expiration",  "exp"   }, x => x.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss")),
            new(new() { "token"                }, x => x.Token             ?? "<unknown>"),
            new(new() { "renew_token", "renew" }, x => x.RenewToken        ?? "<unknown>"),
            new(new() { "state"                }, x => x.State.ToString()),
            new(new() { "json"                 }, x => JsonConvert.SerializeObject(x))
        };

        public AccountFormatter(string formatScheme) =>
            FormatScheme = formatScheme;
        
        public string Format(Account account)
        {
            StringBuilder builder = new(FormatScheme.Unescape());
            foreach (var (keys, handler) in Placeholders)
            {
                string value = handler(account);
                foreach (var key in keys)
                    builder.Replace("{" + key + "}", value);
            }
            return builder.ToString();
        }
    }
}
