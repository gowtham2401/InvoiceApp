# Invoice Generator Pro

A professional WPF Invoice Generator using .NET Framework 4.8, MVVM, and JSON persistence.

## Requirements

- Visual Studio 2019 or later (Community edition is free)
- .NET Framework 4.8 SDK
- Windows OS (WPF is Windows-only)

## Build & Run

1. Open `InvoiceApp.csproj` in Visual Studio
2. Restore NuGet packages (automatic on first build, or right-click solution > Restore NuGet Packages)
3. Press F5 to build and run

Or via command line:
```
dotnet restore
dotnet build
dotnet run
```

## Features

- **Dashboard**: Real-time stats — revenue, pending/paid/overdue counts, 6-month chart
- **Customers**: Add/edit/delete with search, auto-incrementing IDs, form validation
- **Invoices**: Full invoice editor with line items, live tax calculation, status tracking
- **PDF Export**: Professional PDF via QuestPDF saved to `Exports/` folder

## Project Structure

```
InvoiceApp/
├── Models/          Customer, Invoice, InvoiceItem, AppSettings
├── ViewModels/      MainViewModel, CustomerViewModel, InvoiceViewModel, DashboardViewModel
├── Views/           MainWindow, Dashboard, Customers, InvoiceList, InvoiceEditor
├── Services/        JsonDataService, InvoiceNumberService, PdfExportService
├── Helpers/         RelayCommand, ViewModelBase, Converters
├── Resources/       Styles.xaml (full professional theme)
└── Data/            customers.json, invoices.json, settings.json
```

## Data Storage

All data is saved as JSON in the `Data/` folder next to the executable.
Exports are saved to the `Exports/` folder.

## Invoice Number Format

`INV-YYYY-0001` — auto-increments per year, resets each calendar year.
