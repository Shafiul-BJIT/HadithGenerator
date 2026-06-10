# Hadith Generator Desktop

Windows desktop reader version of the ASP.NET Core Hadith Generator.

## Run

```powershell
dotnet run --project HadithGenerator.Desktop/HadithGenerator.Desktop.csproj
```

The app reads the existing `appsettings.json` for the Hadith API URL.

## Build

```powershell
dotnet build HadithGenerator.Desktop/HadithGenerator.Desktop.csproj
```

The desktop project links the hadith services from the sibling
`HadithGenerator.WebApp` project, so API behavior stays shared.
