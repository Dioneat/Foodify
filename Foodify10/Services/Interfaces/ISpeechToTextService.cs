namespace Foodify10.Services.Interfaces
{
    public interface ISpeechToTextService
    {
        Task<string?> ListenOnceAsync(string cultureName, CancellationToken cancellationToken = default);
    }
}