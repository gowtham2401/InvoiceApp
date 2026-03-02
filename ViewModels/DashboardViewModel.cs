using System;
using System.Collections.Generic;
using System.Linq;
using InvoiceApp.Helpers;
using InvoiceApp.Models;

namespace InvoiceApp.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private decimal _totalRevenue;
        private decimal _monthRevenue;
        private int _pendingCount;
        private int _paidCount;
        private int _overdueCount;
        private int _draftCount;
        private int _totalCustomers;
        private int _totalInvoices;
        private List<MonthStat> _monthlyStats;

        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }
        public decimal MonthRevenue { get => _monthRevenue; set => SetProperty(ref _monthRevenue, value); }
        public int PendingCount { get => _pendingCount; set => SetProperty(ref _pendingCount, value); }
        public int PaidCount { get => _paidCount; set => SetProperty(ref _paidCount, value); }
        public int OverdueCount { get => _overdueCount; set => SetProperty(ref _overdueCount, value); }
        public int DraftCount { get => _draftCount; set => SetProperty(ref _draftCount, value); }
        public int TotalCustomers { get => _totalCustomers; set => SetProperty(ref _totalCustomers, value); }
        public int TotalInvoices { get => _totalInvoices; set => SetProperty(ref _totalInvoices, value); }
        public List<MonthStat> MonthlyStats { get => _monthlyStats; set => SetProperty(ref _monthlyStats, value); }

        public void Refresh(List<Invoice> invoices, List<Customer> customers)
        {
            TotalCustomers = customers.Count;
            TotalInvoices = invoices.Count;

            var paid = invoices.Where(i => i.Status == "Paid").ToList();
            TotalRevenue = paid.Sum(i => i.GrandTotal);

            var now = DateTime.Now;
            MonthRevenue = paid.Where(i => i.Date.Year == now.Year && i.Date.Month == now.Month)
                               .Sum(i => i.GrandTotal);

            PaidCount = paid.Count;
            PendingCount = invoices.Count(i => i.Status == "Pending");
            OverdueCount = invoices.Count(i => i.Status == "Overdue");
            DraftCount = invoices.Count(i => i.Status == "Draft");

            // Last 6 months stats
            var stats = new List<MonthStat>();
            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var monthTotal = paid
                    .Where(inv => inv.Date.Year == date.Year && inv.Date.Month == date.Month)
                    .Sum(inv => inv.GrandTotal);
                stats.Add(new MonthStat { Month = date.ToString("MMM"), Total = monthTotal });
            }
            MonthlyStats = stats;
        }
    }

    public class MonthStat
    {
        public string Month { get; set; }
        public decimal Total { get; set; }
    }
}
