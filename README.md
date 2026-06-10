## To Generate .exe file

```
dotnet publish .\HadithGenerator.Desktop\HadithGenerator.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```


**Output Path: \net8.0-windows\win-x64\publish\HadithGenerator.Desktop.exe**