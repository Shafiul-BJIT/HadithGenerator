using HadithGenerator.Services;
using Microsoft.Extensions.Configuration;

namespace HadithGenerator.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var hadithClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        Application.Run(new MainForm(
            new HadithService(hadithClient, configuration)));
    }
}
