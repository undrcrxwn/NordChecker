using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NordChecker.Shared
{
    public static class Extensions
    {
        public static string ToShortDurationString(this TimeSpan @this)
        {
            string result = "";

            if (@this.Days    > 0) result += @this.ToString(@"d\д\ ");
            if (@this.Hours   > 0) result += @this.ToString(@"h\ч\ ");
            if (@this.Minutes > 0) result += @this.ToString(@"m\м\ ");
            if (@this.Seconds > 0 || (int)@this.TotalSeconds == 0)
                result += @this.ToString(@"s\с");

            return result;
        }

        public static string ToBase64(this string str) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

        public static string Unescape(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            var builder = new StringBuilder(str.Length);
            for (int i = 0; i < str.Length;)
            {
                int j = str.IndexOf('\\', i);
                if (j < 0 || j == str.Length - 1)
                    j = str.Length;

                builder.Append(str, i, j - i);
                if (j >= str.Length) break;

                builder.Append(str[j + 1] switch
                {
                    'n'  => '\n',
                    'r'  => '\r',
                    't'  => '\t',
                    '\\' => '\\',
                    _    => '\\' + str[j + 1]
                });
                i = j + 2;
            }

            return builder.ToString();
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            return value.GetType()
                .GetMember(value.ToString()).First()
                .GetCustomAttribute<TAttribute>();
        }
    }
}
