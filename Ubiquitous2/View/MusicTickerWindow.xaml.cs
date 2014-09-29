using System.Windows;
using UB.Properties;

namespace UB.View
{
    /// <summary>
    /// Description for MusicTickerWindow.
    /// </summary>
    public partial class MusicTickerWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MusicTickerWindow class.
        /// </summary>
        public MusicTickerWindow()
        {
            InitializeComponent();
            this.AllowsTransparency = Ubiquitous.Default.Config.AppConfig.EnableTransparency;
            this.Closing += (o, e) => {
                this.Visibility = System.Windows.Visibility.Hidden;
                e.Cancel = true;
            };
        }
    }
}