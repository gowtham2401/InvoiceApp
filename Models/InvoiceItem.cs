using System.ComponentModel;

namespace InvoiceApp.Models
{
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _itemName;
        private int _quantity;
        private decimal _unitPrice;

        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(nameof(ItemName)); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(Total)); }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); OnPropertyChanged(nameof(Total)); }
        }

        public decimal Total => Quantity * UnitPrice;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
