using System;
using System.Collections.ObjectModel;

namespace InvoiceApp.Models
{
    public class Invoice
    {
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int CustomerId { get; set; }
        public ObservableCollection<InvoiceItem> Items { get; set; } = new ObservableCollection<InvoiceItem>();
        public decimal Subtotal { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
