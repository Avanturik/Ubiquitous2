using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;

namespace UB.Utils
{
    public static class UI
    {
        public static void Dispatch( Action action )
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }
    }
}
