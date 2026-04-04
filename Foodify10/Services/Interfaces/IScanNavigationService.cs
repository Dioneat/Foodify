namespace Foodify10.Services.Interfaces
{
    public interface IScanNavigationService
    {
        Task OpenProductPageAsync(string barcode);
        Task GoBackAsync();
    }
}