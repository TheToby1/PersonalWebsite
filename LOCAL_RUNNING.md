# Running PersonalWebsite Locally

This guide explains how to run the PersonalWebsite solution locally without any cloud services.

## Overview

The solution consists of three projects:

| Project | Type | Description |
|---------|------|-------------|
| **BackendFunctions** | .NET 8.0 Azure Functions v4 | HTTP-triggered API (`SendCommunication` endpoint) |
| **Client** | .NET 8.0 Blazor WebAssembly | Frontend SPA using MudBlazor |
| **Shared** | .NET 8.0 Class Library | Shared models and services |

The Client calls the Backend API at `http://localhost:7071` (configured in `Client/wwwroot/appsettings.development.json`).

## Prerequisites

- **.NET 8.0 SDK** or later — [Download](https://dotnet.microsoft.com/download)
- **Azure Functions Core Tools** (v4) — [Install guide](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- **curl** (for health checks in the script)

Verify your installation:

```bash
dotnet --version    # Should be 8.0.x or later
func --version      # Should be 4.x
```

## Quick Start

### Option 1: One-command script

```bash
./run-local.sh
```

This script will:
1. Check prerequisites
2. Restore LibMan packages (Bootstrap CSS)
3. Build the solution
4. Start the BackendFunctions (Azure Functions) on port 7071
5. Start the Client (Blazor WebAssembly) on port 5242
6. Wait for both services to be healthy
7. Keep running until you press `Ctrl+C`

### Option 2: Manual step-by-step

#### 1. Build the solution

```bash
dotnet build PersonalWebsite.sln --configuration Release
```

#### 2. Start the Backend API (Azure Functions)

```bash
cd BackendFunctions
func start --port 7071
```

The API will be available at `http://localhost:7071`.

#### 3. Start the Client (Blazor WebAssembly)

In a separate terminal:

```bash
cd Client
dotnet run --configuration Release
```

The client will be available at `http://localhost:5242` (HTTP) or `https://localhost:7177` (HTTPS).

## Configuration

### Backend Functions

The backend uses `local.settings.json` for local configuration. This file is **git-ignored** and contains:

- `FUNCTIONS_WORKER_RUNTIME`: Set to `dotnet`
- `AzureWebJobsStorage`: Required by Azure Functions runtime (can use a dummy value for local dev)

If you need to add SMTP settings for the contact form email functionality, add them to `local.settings.json` under `Values`:

```json
{
  "IsEncrypted": true,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "SmtpOptions:Host": "smtp.your-provider.com",
    "SmtpOptions:Port": "587",
    "SmtpOptions:Username": "your-email@example.com",
    "SmtpOptions:Password": "your-password",
    "EmailSenderOptions:ToAddress": "your-email@example.com",
    "EmailSenderOptions:ToDisplayName": "TheToby",
    "EmailSenderOptions:NoReplyAddress": "noreply@yourdomain.com"
  }
}
```

> **Note:** The contact form will return an error if SMTP settings are not configured. The frontend will still load and display correctly.

### Client App

The client configuration is in `Client/wwwroot/appsettings.json` and `Client/wwwroot/appsettings.development.json`:

- `appsettings.json` — Production config (API URI defaults to base address)
- `appsettings.development.json` — Development config (API URI set to `http://localhost:7071`)

## Project Structure

```
PersonalWebsite/
├── PersonalWebsite.sln          # Solution file
├── README.md                    # Original README
├── run-local.sh                 # Local run script
├── LOCAL_RUNNING.md             # This file
├── CNAME                        # Custom domain (www.thetoby.eu)
├── .github/
│   ├── dependabot.yml
│   └── workflows/               # CI/CD (Azure Static Web Apps)
├── BackendFunctions/
│   ├── PersonalWebsite.BackendFunctions.csproj
│   ├── Program.cs               # Function host startup
│   ├── host.json                # Functions host config
│   ├── local.settings.json      # Local settings (git-ignored)
│   ├── EndPoints/
│   │   └── SendCommunication.cs # HTTP POST /api/sendcommunication
│   ├── Shared/
│   │   └── EmailValidator.cs
│   └── Properties/
│       ├── launchSettings.json
│       └── serviceDependencies*.json
├── Client/
│   ├── PersonalWebsite.Client.csproj
│   ├── App.razor
│   ├── _Imports.razor
│   ├── libman.json              # LibMan config (Bootstrap CSS)
│   ├── Model/
│   │   └── Program.cs           # DI registration
│   ├── Pages/
│   │   ├── Index.razor          # Home page (CV display)
│   │   ├── Contact.razor        # Contact form
│   │   ├── Blog.razor
│   │   └── Projects.razor
│   ├── Shared/
│   │   ├── MainLayout.razor
│   │   ├── CV/
│   │   │   ├── CVView.razor
│   │   │   ├── CVServiceExtensions.cs
│   │   │   └── CV_temp.razor
│   │   └── Socials/
│   │       ├── Socials.razor
│   │       └── MudDrawerWithSocials.razor
│   └── wwwroot/
│       ├── index.html
│       ├── appsettings.json
│       ├── appsettings.development.json
│       ├── staticwebapp.config.json
│       ├── css/app.css
│       ├── lib/bootstrap/       # Restored by LibMan
│       └── staticData/
│           ├── CVSection.json   # CV data
│           ├── profile.jpg
│           └── thetoby.svg
└── Shared/
    ├── PersonalWebsite.Shared.csproj
    ├── Contact/
    │   ├── ContactService.cs    # HttpContactService, StubContactService
    │   ├── EmailSender.cs       # EmailSender, EmailSenderOptions
    │   └── Messaging.cs         # ContactRequest, ContactResponse
    └── CV/
        ├── CVSection.cs         # CV data model
        └── CVService.cs         # JsonCVService, StubCVService
```

## Troubleshooting

### Port conflicts

If ports 7071, 5242, or 7177 are already in use, modify the port numbers in `run-local.sh` or pass custom URLs:

```bash
cd Client
dotnet run --urls "http://localhost:5000;https://localhost:5001"
```

### LibMan packages not restored

The Bootstrap CSS is managed by LibMan. If `Client/wwwroot/lib/bootstrap/` is empty:

```bash
cd Client
dotnet build  # LibMan runs during build
```

Or install the LibMan CLI:

```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman restore
```

### Backend won't start

Ensure `local.settings.json` exists in `BackendFunctions/`. If missing, create it with at minimum:

```json
{
  "IsEncrypted": true,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true"
  }
}
```

### Contact form returns errors

The contact form requires SMTP configuration in `local.settings.json`. Without it, the API will return a 500 error. The frontend will still work for browsing the CV and other pages.

## Stopping the services

Press `Ctrl+C` in the terminal where `run-local.sh` is running, or kill the processes manually:

```bash
# If using the script, Ctrl+C handles cleanup automatically
# Manual approach:
kill $(lsof -t -i:7071)  # Backend
kill $(lsof -t -i:5242)  # Client
```
