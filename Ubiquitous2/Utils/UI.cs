using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace UB.Utils
{
    public static class UI
    {
        public static void Dispatch(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }

        public static T DispatchFunc<T>( Func<T> func ) where T: class
        {
            if (func == null)
                return null;

            return DispatcherHelper.UIDispatcher.Invoke(() => func());
        }

        public static Dispatcher UIDispatcher
        {
            get {
                return DispatcherHelper.UIDispatcher;
            }
        }
    }
}
