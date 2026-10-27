# TheToby's Personal Website

## This website was created using the following

* id.ie for domain
* github for hosting
* cloudflare for ssl https://www.cloudflare.com and https://www.goyllo.com/cloudflare-ssl-for-github-pages/
* razor for the static page generation https://swimburger.net/blog/dotnet/how-to-deploy-aspnet-blazor-webassembly-to-github-pages

## Technology Stack

* **Frontend**: Blazor WebAssembly (.NET 10) with MudBlazor 9.11
* **Backend**: Azure Functions (.NET 10, Isolated Worker Model) with Azure.Functions.Sdk 1.0.0
* **Shared**: .NET 10 class library

## Development

### Prerequisites
- .NET 10 SDK
- Azure Functions Core Tools v4

### Building
```bash
dotnet build
```

### Running Locally
Use the VS Code tasks (Ctrl+Shift+P > Tasks: Run Task > run-all) or take the commands from inside it.