using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public class AccountFormatter : IDataFormatter<Account, string>
    {
        public string FormatScheme;
        public Dictionary<string, Func<Account, object>> Placeholders;

        public AccountFormatter()
        {
            Placeholders = new Dictionary<string, Func<Account, object>>();
        }

        public void AddPlaceholder(string key, Func<Account, object> handler)
            => Placeholders.Add(key, handler);

        public string Format(Account obj)
        {
            string output = FormatScheme;
            foreach (var (key, handler) in Placeholders)
            {
                object value = handler(obj);

                string formatted = "";
                if (value is DateTime timestamp)
                    formatted = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                else if (value != null)
                    formatted = value.ToString();

                output = output.Replace("{" + key + "}", formatted);
            }
            return output;
        }
    }
}
