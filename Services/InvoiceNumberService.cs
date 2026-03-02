using System;
using System.Threading.Tasks;
using InvoiceApp.Models;

namespace InvoiceApp.Services
{
    public class InvoiceNumberService
    {
        private readonly JsonDataService _dataService;

        public InvoiceNumberService(JsonDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<string> GenerateNextNumberAsync()
        {
            var settings = await _dataService.LoadSettingsAsync();
            int currentYear = DateTime.Now.Year;

            if (settings.LastInvoiceYear != currentYear)
            {
                settings.LastInvoiceSequence = 0;
                settings.LastInvoiceYear = currentYear;
            }

            settings.LastInvoiceSequence++;
            await _dataService.SaveSettingsAsync(settings);

            return $"INV-{currentYear}-{settings.LastInvoiceSequence:D4}";
        }
    }
}
