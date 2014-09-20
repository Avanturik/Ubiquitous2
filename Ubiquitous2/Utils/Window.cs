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
        public static void SetPlacement(this Window window, WindowSettings settings)
        {
            WindowPlacement.SetPlacement(new WindowInteropHelper(window).Handle, settings);
        }

        public static WindowSettings GetPlacement(this Window window)
        {
            return WindowPlacement.GetPlacement(new WindowInteropHelper(window).Handle);
        }

        public static WindowSettings GetPlacement(this Window window, string newName)
        {
            var placement =  WindowPlacement.GetPlacement(new WindowInteropHelper(window).Handle);
            placement.WindowName = newName;
            return placement;
        }
    }
}
