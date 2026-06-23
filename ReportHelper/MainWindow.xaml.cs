using System.Windows;
using ReportHelper.ViewModels;

namespace ReportHelper
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // MainViewModel starts on HomeViewModel and owns all navigation from here.
            DataContext = new MainViewModel();
        }
    }
}
