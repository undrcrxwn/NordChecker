using NordChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
