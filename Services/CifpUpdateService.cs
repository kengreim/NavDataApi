using System.IO.Compression;
using System.Xml.Serialization;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using NavData.Services.Models;

namespace NavData.Services;

public class CifpUpdateService(CifpService cifpService, ILogger<CifpUpdateService> logger)
    : ICancellableInvocable, IInvocable
{
    private readonly HttpClient _httpClient = new();
    private string? _lastKnownUrl;

    public CancellationToken CancellationToken { get; set; }

    public async Task Invoke()
    {
        while (!CancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking for CIFP updates");
            }

            // Wait for an hour before checking again
            await Task.Delay(TimeSpan.FromHours(1), CancellationToken);
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var response =
                await _httpClient.GetAsync("https://external-api.faa.gov/apra/cifp/chart", CancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(CancellationToken);
            var serializer = new XmlSerializer(typeof(CifpEdition.ProductSet));
            var productSet = serializer.Deserialize(new StringReader(content)) as CifpEdition.ProductSet;
            var currentUrl = productSet?.Edition.Product.Url;

            if (currentUrl is not null && currentUrl != _lastKnownUrl)
            {
                logger.LogInformation("New CIFP data available. Downloading from {currentUrl}", currentUrl);
                await DownloadAndExtractCifpAsync(currentUrl);
                _lastKnownUrl = currentUrl;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check for CIFP updates");
            throw;
        }
    }

    private async Task DownloadAndExtractCifpAsync(string url)
    {
        try
        {
            // Download the zip file
            var response = await _httpClient.GetAsync(url, CancellationToken);
            response.EnsureSuccessStatusCode();


            // Extract the zip file
            using (var archive = new ZipArchive(await response.Content.ReadAsStreamAsync(CancellationToken),
                       ZipArchiveMode.Read))
            {
                var faacifpEntry = archive.GetEntry("FAACIFP18");
                if (faacifpEntry is null)
                {
                    throw new Exception("FAACIFP18 file not found in the downloaded zip");
                }

                using (var reader = new StreamReader(faacifpEntry.Open()))
                {
                    List<string> lines = [];
                    while (await reader.ReadLineAsync(CancellationToken) is { } line)
                    {
                        lines.Add(line);
                    }

                    cifpService.UpdateCifp(lines);
                }
            }

            logger.LogInformation("Successfully updated CIFP data");
            response.Content.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download and extract CIFP data");
            throw;
        }
    }
}
