# 🧾 Invoice Generator Pro

A **production-quality, portfolio-ready WPF desktop application** for managing customers and invoices — built with strict MVVM architecture, JSON file persistence, and professional PDF export. No database required.

![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/UI-WPF-informational?style=flat-square)
![MVVM](https://img.shields.io/badge/Pattern-MVVM-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## ✨ Features

### 📊 Dashboard
- Total revenue, this month's revenue, paid/pending/overdue/draft counts
- 6-month revenue bar chart
- Customer and invoice summary stats

### 👥 Customer Management
- Add, edit, delete customers with confirmation dialog
- Auto-incrementing unique IDs (never duplicated, even after deletion)
- Search by name, email, or phone
- Fields: Name, Phone, Email, Address, GST Number
- Form validation with `IDataErrorInfo`

### 🧾 Invoice Management
- Auto-generated invoice numbers in `INV-YYYY-0001` format (yearly reset)
- Select customer, add/remove line items with live total calculation
- Tax percentage input with real-time subtotal, tax, and grand total
- Status tracking: **Draft → Pending → Paid → Overdue**
- Notes field, date picker, full edit and delete support
- Filter by status, date range, and customer search

### 📄 PDF Export
- Professional invoice PDF via **QuestPDF**
- Company header, customer details, itemized table, totals section
- Saved as `Invoice_<InvoiceNumber>.pdf` and auto-opened on export

---

## 🏗️ Architecture

```
InvoiceApp/
│
├── Models/
│   ├── Customer.cs
│   ├── Invoice.cs
│   ├── InvoiceItem.cs
│   └── AppSettings.cs
│
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── CustomerViewModel.cs
│   ├── InvoiceViewModel.cs
│   └── DashboardViewModel.cs
│
├── Views/
│   ├── MainWindow.xaml
│   ├── DashboardView.xaml
│   ├── CustomersView.xaml
│   ├── InvoiceListView.xaml
│   └── InvoiceEditorView.xaml
│
├── Services/
│   ├── JsonDataService.cs
│   ├── InvoiceNumberService.cs
│   └── PdfExportService.cs
│
├── Helpers/
│   ├── RelayCommand.cs
│   ├── ViewModelBase.cs
│   └── Converters.cs
│
├── Resources/
│   └── Styles.xaml
│
└── Data/
    ├── customers.json
    ├── invoices.json
    └── settings.json
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET Framework 4.8 |
| Language | C# 7.3 |
| UI | WPF (Windows Presentation Foundation) |
| Pattern | MVVM (strict — no business logic in code-behind) |
| Persistence | JSON via **Newtonsoft.Json** |
| PDF Export | **QuestPDF** (Community License) |
| Threading | `async/await` for all file I/O |

---

## 🚀 Getting Started

### Prerequisites

- Windows 10 / 11
- [Visual Studio 2019 or later](https://visualstudio.microsoft.com/) (Community edition is free)
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) — pre-installed on most modern Windows machines

### Installation

```bash
# Clone the repository
git clone https://github.com/your-username/InvoiceGeneratorPro.git

# Open in Visual Studio
start InvoiceGeneratorPro/InvoiceApp.csproj
```

1. Open `InvoiceApp.csproj` in Visual Studio
2. NuGet packages restore automatically on first build
3. Press **F5** to build and run

### Build via CLI

```bash
dotnet restore
dotnet build
dotnet run
```

---

## 💾 Data Storage

All data is stored as human-readable JSON files in the `Data/` folder alongside the executable:

| File | Contents |
|---|---|
| `customers.json` | All customer records |
| `invoices.json` | All invoice records with line items |
| `settings.json` | Invoice number sequence + company settings |

PDF exports are saved to an `Exports/` folder next to the executable.

---

## 🔢 Invoice Numbering

Invoices follow the format `INV-YYYY-0001`:
- Auto-increments on each new invoice
- Sequence resets to `0001` each calendar year
- Persisted across app restarts via `settings.json`
- Generated in the service layer — never in the UI

---

## 🔒 Validation

- Customer name is required
- Email format is validated
- Invoice must have at least one line item
- Quantity must be greater than 0
- Unit price cannot be negative
- Duplicate invoice numbers are prevented

---

## 📁 Output Folders

```
bin/
└── Debug/
    ├── InvoiceApp.exe
    ├── Data/
    │   ├── customers.json
    │   ├── invoices.json
    │   └── settings.json
    └── Exports/
        └── Invoice_INV-2025-0001.pdf
```

> **Note:** `bin/` and `obj/` are generated on build and are excluded from source control. The `runtimes/` folder inside `bin/` is placed there by QuestPDF and is required for PDF generation.

---

## 🧹 Cleaning the Project

**In Visual Studio:** Build → Clean Solution

**Manually:** Delete `bin/` and `obj/` folders — they are safe to remove and will regenerate on the next build.

**.gitignore** (recommended):
```
bin/
obj/
Exports/
```

---

## 📸 Screenshots

> *Add screenshots here after running the application.*

| Dashboard | Customers | Invoice Editor |
|---|---|---|
| *(screenshot)* | *(screenshot)* | *(screenshot)* |

---

## 🗺️ Roadmap / Bonus Features

- [ ] Dark / Light theme toggle
- [ ] Company logo on PDF invoices
- [ ] Auto backup on exit
- [ ] Import / export JSON
- [ ] Print support
- [ ] Recent activity panel

---

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you'd like to change.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

> Built as a portfolio-quality desktop application demonstrating clean MVVM architecture, professional WPF UI design, and offline-first data management.
