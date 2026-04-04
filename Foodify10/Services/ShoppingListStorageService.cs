using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Text.Json;

namespace Foodify10.Services
{
    public class ShoppingListStorageService : IShoppingListStorageService
    {
        private const string StorageKey = "shopping_lists";

        public List<ShoppingListModel> GetAll()
        {
            try
            {
                string json = Preferences.Default.Get(StorageKey, string.Empty);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<ShoppingListModel>();

                return JsonSerializer.Deserialize<List<ShoppingListModel>>(json) ?? new List<ShoppingListModel>();
            }
            catch
            {
                return new List<ShoppingListModel>();
            }
        }

        public ShoppingListModel? GetById(string id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public void SaveAll(List<ShoppingListModel> lists)
        {
            string json = JsonSerializer.Serialize(lists);
            Preferences.Default.Set(StorageKey, json);
        }

        public void Save(ShoppingListModel list)
        {
            var lists = GetAll();

            var existing = lists.FirstOrDefault(x => x.Id == list.Id);
            if (existing == null)
            {
                lists.Add(list);
            }
            else
            {
                existing.Title = list.Title;
                existing.Items = list.Items;
                existing.ReminderDate = list.ReminderDate;
                existing.CreatedAt = list.CreatedAt;
            }

            SaveAll(lists);
        }

        public void Delete(string id)
        {
            var lists = GetAll();
            var target = lists.FirstOrDefault(x => x.Id == id);
            if (target != null)
            {
                lists.Remove(target);
                SaveAll(lists);
            }
        }
    }
}