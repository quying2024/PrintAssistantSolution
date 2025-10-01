### **项目目录结构**

PrintAssistantSolution/
│
├── src/
│   └── PrintAssistant/
│       ├── PrintAssistant.csproj
│       ├── appsettings.json
│       ├── Program.cs
│       │
│       ├── Assets/
│       │   └── print\_icon.ico
│       │
│       ├── Configuration/
│       │   ├── AppSettings.cs
│       │   └── SettingsPoco.cs
│       │
│       ├── Core/
│       │   ├── PrintJob.cs
│       │   └── JobStatus.cs
│       │
│       ├── Services/
│       │   ├── Abstractions/
│       │   │   ├── ICoverPageGenerator.cs
│       │   │   ├── IFileArchiver.cs
│       │   │   ├── IFileConverter.cs
│       │   │   ├── IFileConverterFactory.cs
│       │   │   ├── IFileMonitor.cs
│       │   │   ├── IPdfMerger.cs
│       │   │   ├── IPrintQueue.cs
│       │   │   ├── IPrintService.cs
│       │   │   └── ITrayIconService.cs
│       │   │
│       │   ├── Converters/
│       │   │   ├── ExcelToPdfConverter.cs
│       │   │   ├── ImageToPdfConverter.cs
│       │   │   └── WordToPdfConverter.cs
│       │   │
│       │   ├── CoverPageGenerator.cs
│       │   ├── FileArchiver.cs
│       │   ├── FileConverterFactory.cs
│       │   ├── FileMonitorService.cs
│       │   ├── PdfMerger.cs
│       │   ├── PrintProcessorService.cs
│       │   ├── PrintQueueService.cs
│       │   ├── PrintService.cs
│       │   └── TrayIconService.cs
│       │
│       └── UI/
│           ├── PrinterSelectionForm.cs
│           ├── PrinterSelectionForm.Designer.cs
│           ├── SettingsForm.cs
│           └── SettingsForm.Designer.cs
│
└── tests/
    └── PrintAssistant.Tests/
    ├── PrintAssistant.Tests.csproj
    └── Services/
    ├── FileArchiverTests.cs
    └── FileMonitorServiceTests.cs

---
