using System.Windows;
using UB.ViewModel;
using UB.Properties;
using System;
namespace UB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            
            InitializeComponent();
            this.AllowsTransparency = Ubiquitous.Default.Config.AppConfig.EnableTransparency;
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}