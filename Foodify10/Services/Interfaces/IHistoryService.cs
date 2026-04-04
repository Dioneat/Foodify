using Foodify10.Models;

namespace Foodify10.Services.Interfaces
{
    public interface IHistoryService
    {
        Task AddToHistoryAsync(HistoryItem item);
        Task RemoveFromHistoryAsync(Guid id); 
        List<HistoryItem> GetHistory();
        void ClearHistory();
    }
}
