using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace UB.ViewModel
{
    public class SettingsChatHeaderViewModel : ViewModelBase, IHeightMeasurer
    {
        public bool Enabled { get; set; }
        public String ChatName { get; set; }

        [PreferredConstructor]
        public SettingsChatHeaderViewModel(bool enabled, String chatName)
        {
            Enabled = enabled;
            ChatName = chatName;
        }
        public double GetEstimatedHeight(double availableWidth)
        {
            return 30;
        }
    }
}
