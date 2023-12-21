using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
         .ReadFrom.Configuration(configuration)
         .CreateLogger();

        try
        {
            Log.Information("-----------------Starting application----------------------------");

            var url = configuration["DownloadSettings:Url"];
            var filePath = configuration["DownloadSettings:FilePath"];

            await DownloadPdfAsync(url, filePath);

            Log.Information("-------Download complete.---------------");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task DownloadPdfAsync(string url, string filePath)
    {
        using (var httpClient = new HttpClient())
        {
            using (var response = await httpClient.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error: Unable to download the file.");
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
    }
}
