using Foodify10.Models;

namespace Foodify10.Services.Interfaces
{
    public interface IProductJsonExportService
    {
        Task ExportAndShareAsync(ProductExportJsonModel model, string fileNameWithoutExtension);
    }
}