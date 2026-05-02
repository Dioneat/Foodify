using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Foodify10.Services
{
    public class ProductJsonExportService : IProductJsonExportService
    {
        public async Task ExportAndShareAsync(ProductExportJsonModel model, string fileNameWithoutExtension)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(model, options);

            var safeName = MakeSafeFileName(fileNameWithoutExtension);
            var filePath = Path.Combine(FileSystem.CacheDirectory, $"{safeName}.json");

            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Экспорт JSON",
                File = new ShareFile(filePath)
            });
        }

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "product_export";

            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value.Trim();
        }
    }
}