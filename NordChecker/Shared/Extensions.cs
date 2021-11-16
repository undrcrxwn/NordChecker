using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace NordChecker.Shared
{
    public static class Extensions
    {
        /// <summary>
        /// Replaces value of the reference type object with a <i>copy</i> of another instance's value.
        /// </summary>
        public static void ReplaceWith<T>(this T @this, T instance)
            where T : class
        {
            if (@this is null) throw new ArgumentNullException(nameof(@this));
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var size = Marshal.SizeOf(typeof(T));
            var pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(instance, pointer, false);
            Marshal.PtrToStructure(pointer, @this);
            Marshal.FreeHGlobal(pointer);
        }

        public static string ToShortDurationString(this TimeSpan @this)
        {
            string result = "";

            if (@this.Days > 0)    result += @this.ToString(@"d\д\ ");
            if (@this.Hours > 0)   result += @this.ToString(@"h\ч\ ");
            if (@this.Minutes > 0) result += @this.ToString(@"m\м\ ");
            if (@this.Seconds > 0 || (int)@this.TotalSeconds == 0)
                result += @this.ToString(@"s\с");

            return result;
        }

        public static string ToBase64(this string @this)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(@this));

        public static string Unescape(this string @this)
        {
            if (string.IsNullOrEmpty(@this)) return @this;

            var builder = new StringBuilder(@this.Length);
            for (int i = 0; i < @this.Length;)
            {
                int j = @this.IndexOf('\\', i);
                if (j < 0 || j == @this.Length - 1)
                    j = @this.Length;

                builder.Append(@this, i, j - i);
                if (j >= @this.Length) break;

                builder.Append(@this[j + 1] switch
                {
                    'n'  => '\n',
                    'r'  => '\r',
                    't'  => '\t',
                    '\\' => '\\',
                    _    => '\\' + @this[j + 1]
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
