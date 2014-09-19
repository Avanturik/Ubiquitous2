using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class AppConfig :NotifyPropertyChangeBase
    {
        public AppConfig()
        {

        }

        /// <summary>
        /// The <see cref="ThemeName" /> property's name.
        /// </summary>
        public const string ThemeNamePropertyName = "ThemeName";

        private string _themeName = null;

        /// <summary>
        /// Sets and gets the ThemeName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ThemeName
        {
            get
            {
                return _themeName;
            }

            set
            {
                if (_themeName == value)
                {
                    return;
                }

                RaisePropertyChanging(ThemeNamePropertyName);
                _themeName = value;
                RaisePropertyChanged(ThemeNamePropertyName);
            }
        }
    }
}
