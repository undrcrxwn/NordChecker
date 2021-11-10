using NordChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace NordChecker.Shared
{
    public static class Utils
    {
        [DllImport("kernel32.dll")]
        public static extern void AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern void FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static void ShowConsole() =>
            ShowWindow(GetConsoleWindow(), SW_SHOW);

        public static void HideConsole() =>
            ShowWindow(GetConsoleWindow(), SW_HIDE);

        public static bool? Show(this FileDialog dialog, bool topmost = false)
        {
            var window = new Window();
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStyle = WindowStyle.None;
            window.Topmost = topmost;
            window.Visibility = Visibility.Hidden;
            window.Content = dialog;

            bool? result = null;
            window.Loaded += (sender, e) => result = dialog.ShowDialog();
            window.ShowDialog();
            window.Close();
            return result;
        }
    }

    public class Ref<T>
    {
        public T Value;
        public Ref(T value) => Value = value;
    }

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
            if (@this.Days    > 0) result += @this.ToString(@"d\д\ ");
            if (@this.Hours   > 0) result += @this.ToString(@"h\ч\ ");
            if (@this.Minutes > 0) result += @this.ToString(@"m\м\ ");
            if (@this.Seconds > 0 || (int)@this.TotalSeconds == 0)
                result += @this.ToString(@"s\с");
            return result;
        }

        public static string ToBase64(this string @this)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(@this));

        public static string Unescape(this string @this)
        {
            if (string.IsNullOrEmpty(@this))
                return @this;

            var builder = new StringBuilder(@this.Length);
            for (int ix = 0; ix < @this.Length;)
            {
                int jx = @this.IndexOf('\\', ix);
                if (jx < 0 || jx == @this.Length - 1)
                    jx = @this.Length;

                builder.Append(@this, ix, jx - ix);
                if (jx >= @this.Length) break;
                switch (@this[jx + 1])
                {
                    case 'n':  builder.Append('\n'); break;
                    case 'r':  builder.Append('\r'); break;
                    case 't':  builder.Append('\t'); break;
                    case '\\': builder.Append('\\'); break;
                    default:
                        builder.Append('\\').Append(@this[jx + 1]); break;
                }
                ix = jx + 2;
            }

            return builder.ToString();
        }
    }
}
