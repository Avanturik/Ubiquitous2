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
        public static Window UserListWindow = new UserListWindow();
        public static object showLock = new object();

        public static void SetPlacement(this Window window, WindowSettings settings)
        {
            if( settings.Top != double.NaN && settings.Top > 0 )
                window.Top = settings.Top;
            
            if( settings.Left != double.NaN && settings.Left > 0 )
                window.Left = settings.Left;

            if( settings.Width != double.NaN && settings.Width > 0)
                window.Width = settings.Width;
            
            if( settings.Height != double.NaN && settings.Height > 0 )
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
            MusicTickerWindow.Visibility = Visibility.Visible;
        }

        public static void ShowUserList()
        {
            UserListWindow.Visibility = Visibility.Visible;
        }

        public static void ShowStatus()
        {
            StatusWindow.Visibility = Visibility.Visible;
        }

        public static void HideMusicTicker()
        {
            MusicTickerWindow.Visibility = Visibility.Hidden;
        }
        public static void HideUserList()
        {
            UserListWindow.Visibility = Visibility.Hidden;
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
            var windows = new Window[] { StatusWindow, MusicTickerWindow, SettingsWindow, DashboardWindow, UserListWindow };
            if (state != WindowState.Maximized)
                foreach (var win in windows)
                    win.WindowState = state;
        }

        public static void ReloadAllWindows()
        {
            Visibility statusVisibility = StatusWindow.Visibility;
            Visibility musicTickerVisibility = MusicTickerWindow.Visibility;
            Visibility userListWindowVisibility = UserListWindow.Visibility;

            StatusWindow.Close();
            MusicTickerWindow.Close();
            UserListWindow.Close();

            StatusWindow = new StatusWindow();
            MusicTickerWindow = new MusicTickerWindow();
            UserListWindow = new UserListWindow();

            StatusWindow.Visibility = statusVisibility;
            UserListWindow.Visibility = statusVisibility;
            MusicTickerWindow.Visibility = musicTickerVisibility;
        }
    }
}
