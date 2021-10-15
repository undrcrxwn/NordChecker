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
    public class Ref<T>
    {
        public T Value;
        public Ref(T value) => Value = value;
    }

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
            Window window = new Window();
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

        public static string ToShortDurationString(this TimeSpan @this)
        {
            string result = "";
            if (@this.Days > 0)     result += @this.ToString(@"d\д\ ");
            if (@this.Hours > 0)    result += @this.ToString(@"h\ч\ ");
            if (@this.Minutes > 0)  result += @this.ToString(@"m\м\ ");
            if (@this.Seconds > 0 || (int)@this.TotalSeconds == 0)
                result += @this.ToString(@"s\с");
            return result;
        }
    }
}
