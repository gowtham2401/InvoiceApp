using System.Windows;
using InvoiceApp.ViewModels;

namespace InvoiceApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadAsync();
        }
    }
}
