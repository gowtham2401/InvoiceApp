using System;

namespace InvoiceApp.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string GstNumber { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
