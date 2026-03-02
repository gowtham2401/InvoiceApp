namespace InvoiceApp.Models
{
    public class AppSettings
    {
        public int LastInvoiceSequence { get; set; } = 0;
        public int LastInvoiceYear { get; set; } = 0;
        public string CompanyName { get; set; } = "Tomato";
        public string CompanyAddress { get; set; } = "";
        public string CompanyPhone { get; set; } = "";
        public string CompanyEmail { get; set; } = "";
        public string CompanyGst { get; set; } = "";
    }
}
