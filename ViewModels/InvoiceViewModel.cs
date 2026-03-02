using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using InvoiceApp.Helpers;
using InvoiceApp.Models;
using InvoiceApp.Services;

namespace InvoiceApp.ViewModels
{
    public class InvoiceViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly JsonDataService _dataService;
        private readonly InvoiceNumberService _invoiceNumberService;
        private readonly PdfExportService _pdfExportService;

        private ObservableCollection<Invoice> _allInvoices;
        private ObservableCollection<Invoice> _filteredInvoices;
        private Invoice _selectedInvoice;
        private bool _isEditing;

        // Editor fields
        private string _editorInvoiceNumber;
        private DateTime _editorDate = DateTime.Now;
        private Customer _editorCustomer;
        private decimal _editorTaxPercent;
        private string _editorStatus = "Draft";
        private string _editorNotes;
        private ObservableCollection<InvoiceItem> _editorItems;

        // New item fields
        private string _newItemName;
        private int _newItemQty = 1;
        private decimal _newItemPrice;

        // Filter fields
        private string _searchText;
        private string _statusFilter = "All";
        private DateTime? _fromDate;
        private DateTime? _toDate;

        private List<Customer> _customers;

        public ObservableCollection<Invoice> FilteredInvoices
        {
            get => _filteredInvoices;
            set => SetProperty(ref _filteredInvoices, value);
        }

        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set => SetProperty(ref _selectedInvoice, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string EditorInvoiceNumber { get => _editorInvoiceNumber; set => SetProperty(ref _editorInvoiceNumber, value); }
        public DateTime EditorDate { get => _editorDate; set => SetProperty(ref _editorDate, value); }
        public Customer EditorCustomer
        {
            get => _editorCustomer;
            set => SetProperty(ref _editorCustomer, value);
        }
        public decimal EditorTaxPercent
        {
            get => _editorTaxPercent;
            set { SetProperty(ref _editorTaxPercent, value); RecalculateTotals(); }
        }
        public string EditorStatus { get => _editorStatus; set => SetProperty(ref _editorStatus, value); }
        public string EditorNotes { get => _editorNotes; set => SetProperty(ref _editorNotes, value); }
        public ObservableCollection<InvoiceItem> EditorItems
        {
            get => _editorItems;
            set => SetProperty(ref _editorItems, value);
        }

        private decimal _editorSubtotal;
        private decimal _editorTaxAmount;
        private decimal _editorGrandTotal;

        public decimal EditorSubtotal { get => _editorSubtotal; set => SetProperty(ref _editorSubtotal, value); }
        public decimal EditorTaxAmount { get => _editorTaxAmount; set => SetProperty(ref _editorTaxAmount, value); }
        public decimal EditorGrandTotal { get => _editorGrandTotal; set => SetProperty(ref _editorGrandTotal, value); }

        public string NewItemName { get => _newItemName; set => SetProperty(ref _newItemName, value); }
        public int NewItemQty { get => _newItemQty; set => SetProperty(ref _newItemQty, value); }
        public decimal NewItemPrice { get => _newItemPrice; set => SetProperty(ref _newItemPrice, value); }

        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
        public string StatusFilter { get => _statusFilter; set { SetProperty(ref _statusFilter, value); ApplyFilter(); } }
        public DateTime? FromDate { get => _fromDate; set { SetProperty(ref _fromDate, value); ApplyFilter(); } }
        public DateTime? ToDate { get => _toDate; set { SetProperty(ref _toDate, value); ApplyFilter(); } }

        public List<Customer> Customers { get => _customers; set => SetProperty(ref _customers, value); }
        public List<string> StatusOptions { get; } = new List<string> { "All", "Draft", "Pending", "Paid", "Overdue" };
        public List<string> EditorStatusOptions { get; } = new List<string> { "Draft", "Pending", "Paid", "Overdue" };

        public RelayCommand NewInvoiceCommand { get; }
        public RelayCommand EditInvoiceCommand { get; }
        public RelayCommand SaveInvoiceCommand { get; }
        public RelayCommand DeleteInvoiceCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand AddItemCommand { get; }
        public RelayCommand RemoveItemCommand { get; }
        public RelayCommand ExportPdfCommand { get; }
        public RelayCommand ClearFiltersCommand { get; }

        public InvoiceViewModel(JsonDataService dataService, InvoiceNumberService invoiceNumberService, PdfExportService pdfExportService)
        {
            _dataService = dataService;
            _invoiceNumberService = invoiceNumberService;
            _pdfExportService = pdfExportService;
            _allInvoices = new ObservableCollection<Invoice>();
            _filteredInvoices = new ObservableCollection<Invoice>();
            _editorItems = new ObservableCollection<InvoiceItem>();
            _customers = new List<Customer>();

            NewInvoiceCommand = new RelayCommand(async _ => await NewInvoiceAsync(), _ => !IsEditing);
            EditInvoiceCommand = new RelayCommand(_ => EditInvoice(), _ => SelectedInvoice != null && !IsEditing);
            SaveInvoiceCommand = new RelayCommand(async _ => await SaveInvoiceAsync(), _ => IsEditing);
            DeleteInvoiceCommand = new RelayCommand(async _ => await DeleteInvoiceAsync(), _ => SelectedInvoice != null && !IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            AddItemCommand = new RelayCommand(_ => AddItem(), _ => IsEditing && !string.IsNullOrWhiteSpace(NewItemName));
            RemoveItemCommand = new RelayCommand(item => RemoveItem(item as InvoiceItem), item => IsEditing && item != null);
            ExportPdfCommand = new RelayCommand(async _ => await ExportPdfAsync(), _ => SelectedInvoice != null && !IsEditing);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
        }

        public async Task LoadAsync(List<Customer> customers)
        {
            Customers = customers;
            var list = await _dataService.LoadInvoicesAsync();
            _allInvoices = new ObservableCollection<Invoice>(list);
            ApplyFilter();
        }

        public void RefreshCustomers(List<Customer> customers)
        {
            Customers = customers;
        }

        private void ApplyFilter()
        {
            var filtered = _allInvoices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(i =>
                {
                    var customer = _customers.FirstOrDefault(c => c.Id == i.CustomerId);
                    return i.InvoiceNumber.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           (customer != null && customer.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
                });
            }

            if (StatusFilter != "All")
                filtered = filtered.Where(i => i.Status == StatusFilter);

            if (FromDate.HasValue)
                filtered = filtered.Where(i => i.Date >= FromDate.Value);

            if (ToDate.HasValue)
                filtered = filtered.Where(i => i.Date <= ToDate.Value);

            FilteredInvoices = new ObservableCollection<Invoice>(filtered.OrderByDescending(i => i.Date));
        }

        private void ClearFilters()
        {
            SearchText = "";
            StatusFilter = "All";
            FromDate = null;
            ToDate = null;
        }

        private async Task NewInvoiceAsync()
        {
            var number = await _invoiceNumberService.GenerateNextNumberAsync();
            EditorInvoiceNumber = number;
            EditorDate = DateTime.Now;
            EditorCustomer = null;
            EditorTaxPercent = 18;
            EditorStatus = "Draft";
            EditorNotes = "";
            EditorItems = new ObservableCollection<InvoiceItem>();
            EditorItems.CollectionChanged += (s, e) => RecalculateTotals();
            RecalculateTotals();
            NewItemName = ""; NewItemQty = 1; NewItemPrice = 0;
            SelectedInvoice = null;
            IsEditing = true;
        }

        private void EditInvoice()
        {
            if (SelectedInvoice == null) return;
            EditorInvoiceNumber = SelectedInvoice.InvoiceNumber;
            EditorDate = SelectedInvoice.Date;
            EditorCustomer = _customers.FirstOrDefault(c => c.Id == SelectedInvoice.CustomerId);
            EditorTaxPercent = SelectedInvoice.TaxPercent;
            EditorStatus = SelectedInvoice.Status;
            EditorNotes = SelectedInvoice.Notes;
            EditorItems = new ObservableCollection<InvoiceItem>(SelectedInvoice.Items);
            EditorItems.CollectionChanged += (s, e) => RecalculateTotals();
            foreach (var item in EditorItems)
                item.PropertyChanged += (s, e) => RecalculateTotals();
            RecalculateTotals();
            NewItemName = ""; NewItemQty = 1; NewItemPrice = 0;
            IsEditing = true;
        }

        private async Task SaveInvoiceAsync()
        {
            if (EditorCustomer == null) { MessageBox.Show("Please select a customer.", "Validation"); return; }
            if (!EditorItems.Any()) { MessageBox.Show("Add at least one item.", "Validation"); return; }
            if (EditorItems.Any(i => i.Quantity <= 0)) { MessageBox.Show("All quantities must be > 0.", "Validation"); return; }
            if (EditorItems.Any(i => i.UnitPrice < 0)) { MessageBox.Show("Prices cannot be negative.", "Validation"); return; }

            RecalculateTotals();

            var existing = _allInvoices.FirstOrDefault(i => i.InvoiceNumber == EditorInvoiceNumber);
            if (existing != null)
            {
                existing.Date = EditorDate;
                existing.CustomerId = EditorCustomer.Id;
                existing.Items = new ObservableCollection<InvoiceItem>(EditorItems);
                existing.Subtotal = EditorSubtotal;
                existing.TaxPercent = EditorTaxPercent;
                existing.TaxAmount = EditorTaxAmount;
                existing.GrandTotal = EditorGrandTotal;
                existing.Status = EditorStatus;
                existing.Notes = EditorNotes;
            }
            else
            {
                var invoice = new Invoice
                {
                    InvoiceNumber = EditorInvoiceNumber,
                    Date = EditorDate,
                    CustomerId = EditorCustomer.Id,
                    Items = new ObservableCollection<InvoiceItem>(EditorItems),
                    Subtotal = EditorSubtotal,
                    TaxPercent = EditorTaxPercent,
                    TaxAmount = EditorTaxAmount,
                    GrandTotal = EditorGrandTotal,
                    Status = EditorStatus,
                    Notes = EditorNotes,
                    CreatedOn = DateTime.Now
                };
                _allInvoices.Add(invoice);
            }

            await _dataService.SaveInvoicesAsync(new List<Invoice>(_allInvoices));
            ApplyFilter();
            CancelEdit();
        }

        private async Task DeleteInvoiceAsync()
        {
            if (SelectedInvoice == null) return;
            var result = MessageBox.Show($"Delete invoice {SelectedInvoice.InvoiceNumber}?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            _allInvoices.Remove(SelectedInvoice);
            SelectedInvoice = null;
            await _dataService.SaveInvoicesAsync(new List<Invoice>(_allInvoices));
            ApplyFilter();
        }

        private void CancelEdit()
        {
            IsEditing = false;
            EditorItems = new ObservableCollection<InvoiceItem>();
        }

        private void AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewItemName) || NewItemQty <= 0 || NewItemPrice < 0) return;
            var item = new InvoiceItem { ItemName = NewItemName, Quantity = NewItemQty, UnitPrice = NewItemPrice };
            item.PropertyChanged += (s, e) => RecalculateTotals();
            EditorItems.Add(item);
            NewItemName = ""; NewItemQty = 1; NewItemPrice = 0;
        }

        private void RemoveItem(InvoiceItem item)
        {
            if (item != null) EditorItems.Remove(item);
        }

        private void RecalculateTotals()
        {
            EditorSubtotal = EditorItems?.Sum(i => i.Total) ?? 0;
            EditorTaxAmount = Math.Round(EditorSubtotal * EditorTaxPercent / 100, 2);
            EditorGrandTotal = EditorSubtotal + EditorTaxAmount;
        }

        private async Task ExportPdfAsync()
        {
            if (SelectedInvoice == null) return;
            var customer = _customers.FirstOrDefault(c => c.Id == SelectedInvoice.CustomerId);
            if (customer == null) { MessageBox.Show("Customer not found."); return; }

            try
            {
                var path = await _pdfExportService.ExportAsync(SelectedInvoice, customer);
                MessageBox.Show($"PDF exported to:\n{path}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string GetCustomerName(int customerId)
        {
            return _customers.FirstOrDefault(c => c.Id == customerId)?.Name ?? "Unknown";
        }

        public string this[string columnName] => null;
        public string Error => null;

        public List<Invoice> AllInvoices => _allInvoices.ToList();
    }
}
