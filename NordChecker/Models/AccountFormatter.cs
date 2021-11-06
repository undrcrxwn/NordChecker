using System.Collections.Generic;
using System.Text;

namespace NordChecker.Models
{
    public class AccountFormatter : IPayloadFormatter<Account, string>
    {
        public string FormatScheme;
        public List<Placeholder> Placeholders = new()
        {
            new(new() { "email",       "mail"  }, x => x.Email),
            new(new() { "password",    "pass"  }, x => x.Password),
            new(new() { "proxy",       "ip"    }, x => x.Proxy.ToString()),
            new(new() { "expiration",  "exp"   }, x => x.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss")),
            new(new() { "token"                }, x => x.Token),
            new(new() { "renew_token", "renew" }, x => x.RenewToken)
        };

        public AccountFormatter(string formatScheme) => FormatScheme = formatScheme;
        
        public string Format(Account account)
        {
            StringBuilder builder = new(FormatScheme);
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
