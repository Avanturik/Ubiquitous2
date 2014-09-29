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
        //Normal windows
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
            if (!StatusWindow.IsLoaded)
                StatusWindow = new StatusWindow();

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
            if (!DashboardWindow.IsLoaded)
                DashboardWindow = new DashBoardWindow();

            DashboardWindow.Visibility = Visibility.Visible;
        }
        public static void HideDashBoard()
        {
            DashboardWindow.Visibility = Visibility.Hidden;
        }

        public static void SetGlobalStatus(WindowState state)
        {
            StatusWindow.WindowState = state;
            MusicTickerWindow.WindowState = state;
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
