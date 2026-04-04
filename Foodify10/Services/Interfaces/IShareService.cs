namespace Foodify10.Services.Interfaces
{
    public interface IShareService
    {
        Task ShareTextAsync(string title, string text);
    }
}