using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Text.Json;

namespace Foodify10.Services
{
    public class HistoryService : IHistoryService
    {
        private const string HistoryKey = "all_scans_history";

        public List<HistoryItem> GetHistory()
        {
            string json = Preferences.Default.Get(HistoryKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new List<HistoryItem>();

            try
            {
                return JsonSerializer.Deserialize<List<HistoryItem>>(json) ?? new List<HistoryItem>();
            }
            catch
            {
                return new List<HistoryItem>();
            }
        }

        public async Task AddToHistoryAsync(HistoryItem item)
        {
            await Task.Run(() => {
                var history = GetHistory();

                // Добавляем новое сканирование в начало
                history.Insert(0, item);

                Save(history);
            });
        }

        public async Task RemoveFromHistoryAsync(Guid id)
        {
            await Task.Run(() => {
                var history = GetHistory();
                var itemToRemove = history.FirstOrDefault(x => x.Id == id);

                if (itemToRemove != null)
                {
                    history.Remove(itemToRemove);
                    Save(history);
                }
            });
        }

        public void ClearHistory() => Preferences.Default.Remove(HistoryKey);

        private void Save(List<HistoryItem> history)
        {
            string json = JsonSerializer.Serialize(history);
            Preferences.Default.Set(HistoryKey, json);
        }
    }
}
