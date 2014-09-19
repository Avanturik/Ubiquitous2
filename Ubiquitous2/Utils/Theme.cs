using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UB.Properties;

namespace UB.Utils
{
    public class Theme
    {
        public static void SwitchTheme( string themeName )
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Ubiquitous2;component/Skins/" + themeName + "/Skin.xaml", UriKind.RelativeOrAbsolute)
            });
            Ubiquitous.Default.Config.AppConfig.ThemeName = themeName;
        }
    }
}
