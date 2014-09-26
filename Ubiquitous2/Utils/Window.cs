using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.View;

namespace UB.Utils
{
    public static class Win
    {
        //Transparent windows
        private static Window[] windows = new Window[] { new StatusWindow(), new MusicTickerWindow()};

        //Normal windows
        private static Window dashboardWindow = new DashBoardWindow();

        public static void SetPlacement(this Window window, WindowSettings settings)
        {
            window.Top = settings.Top;
            window.Left = settings.Left;
            window.Width = settings.Width;
            window.Height = settings.Height;
        }

        public static WindowSettings GetPlacement(this Window window)
        {
            return new WindowSettings()
            {
                Top = window.Top,
                Left = window.Left,
                Width = window.Width,
                Height = window.Height,
            };
            
        }

        public static WindowSettings GetPlacement(this Window window, string newName)
        {
            return new WindowSettings()
            {
                Top = window.Top,
                Left = window.Left,
                Width = window.Width,
                Height = window.Height,
                WindowName = newName,
            };
        }

        public static void ShowMusicTicker()
        {
            windows[1].Visibility = Visibility.Visible;
        }

        public static void ShowStatus()
        {
            windows[0].Visibility = Visibility.Visible;
        }

        public static void HideMusicTicker()
        {
            windows[1].Visibility = Visibility.Hidden;
        }

        public static void HideStatus()
        {
            windows[0].Visibility = Visibility.Hidden;
        }
        public static void ShowDashBoard()
        {
            dashboardWindow.Visibility = Visibility.Visible;
        }
        public static void HideDashBoard()
        {
            dashboardWindow.Visibility = Visibility.Hidden;
        }

        public static void SetGlobalStatus(WindowState state)
        {
            foreach (var win in windows)
                win.WindowState = state;
        }

        public static void ReloadAllWindows()
        {
            var oldWindows = windows.ToArray();

            windows = new Window[] { new StatusWindow(), new MusicTickerWindow() };
            
            for (int i = 0; i < oldWindows.Length; i++)
            {
                var visibility = oldWindows[i].Visibility;
                oldWindows[i].Close();
                oldWindows[i] = null;
                windows[i].Visibility = visibility;
            }
            oldWindows = null;
        }
    }
}
