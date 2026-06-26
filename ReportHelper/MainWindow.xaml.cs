using System.Windows;
using ReportHelper.ViewModels;

namespace ReportHelper
{
    public partial class MainWindow : Window
    {
        // MainViewModel is now supplied by the DI container (see App.xaml.cs) instead
        // of being constructed here directly — this is what lets MainViewModel itself
        // receive its IServiceProvider dependency without MainWindow needing to know
        // anything about how MainViewModel is built.
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
