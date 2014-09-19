using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UB.Properties;

namespace UB.Converter
{
    public class StringToIsChecked : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string themeName = parameter as string;
            string currentThemeName = value as string;
            return  themeName.Equals(currentThemeName,StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (string)parameter;
        }
    }
}
