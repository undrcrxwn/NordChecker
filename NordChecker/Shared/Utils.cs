using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

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
            var window = new Window
            {
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Topmost = topmost,
                Visibility = Visibility.Hidden,
                Content = dialog
            };

            bool? result = null;
            window.Loaded += (sender, e) => result = dialog.ShowDialog();
            window.ShowDialog();
            window.Close();
            return result;
        }
    }
}
