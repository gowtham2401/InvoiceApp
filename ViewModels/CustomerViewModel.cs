using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using InvoiceApp.Helpers;
using InvoiceApp.Models;
using InvoiceApp.ViewModels;
using InvoiceApp.Services;

namespace InvoiceApp.ViewModels
{
    public class CustomerViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly JsonDataService _dataService;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Customer> _filteredCustomers;
        private Customer _selectedCustomer;
        private string _searchText;
        private bool _isEditing;

        // Form fields
        private int _editId;
        private string _editName;
        private string _editPhone;
        private string _editEmail;
        private string _editAddress;
        private string _editGst;

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set => SetProperty(ref _filteredCustomers, value);
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set { SetProperty(ref _selectedCustomer, value); if (value != null) LoadForEdit(value); }
        }

        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ApplyFilter(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public int EditId { get => _editId; set => SetProperty(ref _editId, value); }
        public string EditName { get => _editName; set => SetProperty(ref _editName, value); }
        public string EditPhone { get => _editPhone; set => SetProperty(ref _editPhone, value); }
        public string EditEmail { get => _editEmail; set => SetProperty(ref _editEmail, value); }
        public string EditAddress { get => _editAddress; set => SetProperty(ref _editAddress, value); }
        public string EditGst { get => _editGst; set => SetProperty(ref _editGst, value); }

        public RelayCommand NewCustomerCommand { get; }
        public RelayCommand SaveCustomerCommand { get; }
        public RelayCommand DeleteCustomerCommand { get; }
        public RelayCommand CancelEditCommand { get; }

        public CustomerViewModel(JsonDataService dataService)
        {
            _dataService = dataService;
            _customers = new ObservableCollection<Customer>();
            _filteredCustomers = new ObservableCollection<Customer>();

            NewCustomerCommand = new RelayCommand(NewCustomer);
            SaveCustomerCommand = new RelayCommand(async _ => await SaveCustomerAsync(), _ => IsEditing);
            DeleteCustomerCommand = new RelayCommand(async _ => await DeleteCustomerAsync(), _ => SelectedCustomer != null && !IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
        }

        public async Task LoadAsync()
        {
            var list = await _dataService.LoadCustomersAsync();
            _customers = new ObservableCollection<Customer>(list);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _customers
                : new ObservableCollection<Customer>(_customers.Where(c =>
                    c.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (c.Email != null && c.Email.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Phone != null && c.Phone.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)));

            FilteredCustomers = filtered;
        }

        private void NewCustomer()
        {
            SelectedCustomer = null;
            EditId = 0;
            EditName = EditPhone = EditEmail = EditAddress = EditGst = "";
            IsEditing = true;
        }

        private void LoadForEdit(Customer c)
        {
            EditId = c.Id;
            EditName = c.Name;
            EditPhone = c.Phone;
            EditEmail = c.Email;
            EditAddress = c.Address;
            EditGst = c.GstNumber;
            IsEditing = true;
        }

        private async Task SaveCustomerAsync()
        {
            if (!string.IsNullOrWhiteSpace(this["EditName"])) return;

            if (EditId == 0)
            {
                var newId = _customers.Count == 0 ? 1 : _customers.Max(c => c.Id) + 1;
                var customer = new Customer
                {
                    Id = newId, Name = EditName, Phone = EditPhone,
                    Email = EditEmail, Address = EditAddress, GstNumber = EditGst,
                    CreatedOn = DateTime.Now
                };
                _customers.Add(customer);
            }
            else
            {
                var existing = _customers.FirstOrDefault(c => c.Id == EditId);
                if (existing != null)
                {
                    existing.Name = EditName; existing.Phone = EditPhone;
                    existing.Email = EditEmail; existing.Address = EditAddress;
                    existing.GstNumber = EditGst;
                }
            }

            await _dataService.SaveCustomersAsync(new List<Customer>(_customers));
            ApplyFilter();
            CancelEdit();
        }

        private async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null) return;
            var result = MessageBox.Show($"Delete customer '{SelectedCustomer.Name}'?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            _customers.Remove(SelectedCustomer);
            SelectedCustomer = null;
            await _dataService.SaveCustomersAsync(new List<Customer>(_customers));
            ApplyFilter();
        }

        private void CancelEdit()
        {
            IsEditing = false;
            SelectedCustomer = null;
            EditId = 0;
            EditName = EditPhone = EditEmail = EditAddress = EditGst = "";
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(EditName) && string.IsNullOrWhiteSpace(EditName))
                    return "Name is required.";
                if (columnName == nameof(EditEmail) && !string.IsNullOrWhiteSpace(EditEmail))
                {
                    try { var addr = new System.Net.Mail.MailAddress(EditEmail); }
                    catch { return "Invalid email format."; }
                }
                return null;
            }
        }

        public string Error => null;

        public List<Customer> AllCustomers => _customers.ToList();
    }
}
