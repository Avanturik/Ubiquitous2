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
        public static Window SettingsWindow = new SettingsWindow();
        public static Window StatusWindow = new StatusWindow();
        public static Window MusicTickerWindow = new MusicTickerWindow();
        public static Window DashboardWindow = new DashBoardWindow();
        public static object showLock = new object();

        public static void SetPlacement(this Window window, WindowSettings settings)
        {
            window.Top = settings.Top;
            window.Left = settings.Left;
            window.Width = settings.Width;
            window.Height = settings.Height == 0 || settings.Height == double.NaN ? window.Height : settings.Height;
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
            MusicTickerWindow.Visibility = Visibility.Visible;
        }

        public static void ShowStatus()
        {
            StatusWindow.Visibility = Visibility.Visible;
        }

        public static void HideMusicTicker()
        {
            MusicTickerWindow.Visibility = Visibility.Hidden;
        }

        public static void HideStatus()
        {
            StatusWindow.Visibility = Visibility.Hidden;
        }
        public static void ShowDashBoard()
        {
            DashboardWindow.Visibility = Visibility.Visible;
        }
        public static void HideDashBoard()
        {
            DashboardWindow.Visibility = Visibility.Hidden;
        }

        public static void ShowSettings()
        {
            SettingsWindow.Visibility = Visibility.Visible;
        }
        public static void HideSettings()
        {
            SettingsWindow.Visibility = Visibility.Hidden;
        }

        public static void SetGlobalStatus(WindowState state)
        {
            StatusWindow.WindowState = state != WindowState.Maximized ? state : StatusWindow.WindowState;
            MusicTickerWindow.WindowState = state != WindowState.Maximized ? state : MusicTickerWindow.WindowState;
            SettingsWindow.WindowState = state != WindowState.Maximized ? state : SettingsWindow.WindowState;
            DashboardWindow.WindowState = state != WindowState.Maximized ? state : DashboardWindow.WindowState;
        }

        public static void ReloadAllWindows()
        {
            Visibility statusVisibility = StatusWindow.Visibility;
            Visibility musicTickerVisibility = MusicTickerWindow.Visibility;

            StatusWindow.Close();
            MusicTickerWindow.Close();

            StatusWindow = new StatusWindow();
            MusicTickerWindow = new MusicTickerWindow();

            StatusWindow.Visibility = statusVisibility;
            MusicTickerWindow.Visibility = musicTickerVisibility;
        }
    }
}
