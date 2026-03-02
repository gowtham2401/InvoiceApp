using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InvoiceApp.Models;
using Newtonsoft.Json;

namespace InvoiceApp.Services
{
    public class JsonDataService
    {
        private readonly string _basePath;
        private readonly string _customersFile;
        private readonly string _invoicesFile;
        private readonly string _settingsFile;

        public JsonDataService()
        {
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(_basePath);
            _customersFile = Path.Combine(_basePath, "customers.json");
            _invoicesFile = Path.Combine(_basePath, "invoices.json");
            _settingsFile = Path.Combine(_basePath, "settings.json");
        }

        private async Task<T> LoadAsync<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                    return new T();

                var json = await Task.Run(() => File.ReadAllText(filePath));

                var result = JsonConvert.DeserializeObject<T>(json);
                return result != null ? result : new T();
            }
            catch
            {
                return new T();
            }
        }
        private async Task SaveAsync<T>(string filePath, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(filePath, json));
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to save data: {ex.Message}", ex);
            }
        }

        public Task<List<Customer>> LoadCustomersAsync() => LoadAsync<List<Customer>>(_customersFile);
        public Task SaveCustomersAsync(List<Customer> customers) => SaveAsync(_customersFile, customers);
        public Task<List<Invoice>> LoadInvoicesAsync() => LoadAsync<List<Invoice>>(_invoicesFile);
        public Task SaveInvoicesAsync(List<Invoice> invoices) => SaveAsync(_invoicesFile, invoices);
        public Task<AppSettings> LoadSettingsAsync() => LoadAsync<AppSettings>(_settingsFile);
        public Task SaveSettingsAsync(AppSettings settings) => SaveAsync(_settingsFile, settings);
    }
}
