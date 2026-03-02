using System.Threading.Tasks;
using InvoiceApp.Helpers;
using InvoiceApp.Services;

namespace InvoiceApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly JsonDataService _dataService;
        private int _selectedTabIndex;

        public CustomerViewModel CustomerVM { get; }
        public InvoiceViewModel InvoiceVM { get; }
        public DashboardViewModel DashboardVM { get; }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                SetProperty(ref _selectedTabIndex, value);
                if (value == 0) RefreshDashboard();
            }
        }

        public RelayCommand RefreshCommand { get; }

        public MainViewModel()
        {
            _dataService = new JsonDataService();
            var invoiceNumberService = new InvoiceNumberService(_dataService);
            var pdfExportService = new PdfExportService(_dataService);

            CustomerVM = new CustomerViewModel(_dataService);
            InvoiceVM = new InvoiceViewModel(_dataService, invoiceNumberService, pdfExportService);
            DashboardVM = new DashboardViewModel();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        }

        public async Task LoadAsync()
        {
            await CustomerVM.LoadAsync();
            await InvoiceVM.LoadAsync(CustomerVM.AllCustomers);
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            DashboardVM.Refresh(InvoiceVM.AllInvoices, CustomerVM.AllCustomers);
        }

        public void OnCustomersSaved()
        {
            InvoiceVM.RefreshCustomers(CustomerVM.AllCustomers);
            RefreshDashboard();
        }

        public void OnInvoicesSaved()
        {
            RefreshDashboard();
        }
    }
}
