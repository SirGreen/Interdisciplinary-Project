# DADN - Gearbox Design Assistant

An ASP.NET Core MVC web application for **mechanical gearbox design calculations** and **motor catalog management**. The system helps engineers input conveyor belt parameters, automatically compute gearbox specifications (gear ratios, shaft dimensions, torque, etc.), select suitable motors from a database, and export professional PDF design reports.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9.0 (MVC) |
| Database | MongoDB Atlas |
| Image Storage | Cloudinary |
| AI Extraction | HuggingFace API / Custom T5 model |
| PDF Generation | QuestPDF |
| Web Scraping | HtmlAgilityPack |
| Frontend | Bootstrap, jQuery |

## Project Structure

```
DADN/
├── Controllers/
│   ├── HomeController.cs         # Landing page
│   ├── InputController.cs        # Parameter input & motor suggestion
│   ├── DashboardController.cs    # Analytics dashboard & charts
│   ├── AdminController.cs        # Catalog upload & management
│   ├── Calculation.cs            # Gearbox design calculations (Phase 1)
│   ├── Calculation2.cs           # Shaft dimensioning calculations (Phase 2)
│   └── exportPDF.cs              # PDF report generation with QuestPDF
├── Models/
│   ├── InputModel.cs             # Input parameters & request DTOs
│   ├── MotorCatalog.cs           # MongoDB motor catalog entity
│   └── ErrorViewModel.cs
├── Services/
│   ├── CatalogService.cs         # Motor catalog business logic
│   ├── ExtractionService.cs      # Web scraping & AI data extraction
│   ├── ApiConfig.cs              # AI API endpoint configuration
│   └── I*Service.cs              # Service interfaces
├── Repositories/
│   ├── CatalogRepository.cs      # MongoDB data access
│   └── ICatalogRepository.cs
├── Observers/
│   ├── IObserver.cs              # Observer pattern interface
│   └── AdminNotifier.cs          # Admin notification observer
├── Data/
│   └── AppDbContext.cs
├── Views/
│   ├── Input/                    # Parameter input form & results
│   ├── Admin/                    # Catalog management views
│   ├── Dashboard/                # Analytics dashboard
│   └── Shared/                   # Layout, navbar, partials
└── wwwroot/                      # Static assets (CSS, JS, images)
```

## Features

### Gearbox Design Calculation
- Input conveyor belt parameters: force (N), speed (m/s), drum diameter (mm), service life, work schedule, load profiles
- Automatic calculation of: overall efficiency, required motor power, working shaft speed, preliminary motor speed, transmission ratios, torque distribution across shafts
- Shaft dimensioning with lookup tables for standard bearing and keyway sizes

### Motor Selection
- Query matching motors from the MongoDB catalog based on calculated requirements (power, speed)
- Interactive results table with motor details (brand, frame size, output, efficiency, torque characteristics)

### Motor Catalog Management (Admin)
- Upload motor data by providing a product page URL
- AI-powered data extraction from web pages using a fine-tuned T5 model
- Manual review/edit form before saving to the database
- Image upload via Cloudinary
- Full CRUD operations on the catalog

### Dashboard & Analytics
- Motor count statistics grouped by type
- Visual charts and data overview

### PDF Export
- Generate professional A4 design reports with all calculation results using QuestPDF

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- MongoDB Atlas connection (configured in `appsettings.json`)
- Cloudinary account (configured in `appsettings.json`)

## Getting Started

```bash
# Clone the repository
git clone <repository-url>
cd DADN

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Or run with hot-reload
dotnet watch run
```

The app will be available at `https://localhost:5001` (or the port configured in [Properties/launchSettings.json](Properties/launchSettings.json)).

## Configuration

Update [appsettings.json](appsettings.json) with your own credentials:

```jsonc
{
  "MongoDB": {
    "ConnectionString": "<your-mongodb-connection-string>",
    "DatabaseName": "MotorDB"
  },
  "Cloudinary": {
    "CloudName": "<your-cloud-name>",
    "ApiKey": "<your-api-key>",
    "ApiSecret": "<your-api-secret>"
  }
}
```

The AI extraction API endpoint can be configured in [Services/ApiConfig.cs](Services/ApiConfig.cs).

## Design Patterns

- **Repository Pattern** – `CatalogRepository` abstracts MongoDB data access
- **Service Layer** – `CatalogService`, `ExtractionService` encapsulate business logic
- **Observer Pattern** – `IObserver` / `AdminNotifier` for notification events
- **Dependency Injection** – All services registered and injected via ASP.NET Core DI
