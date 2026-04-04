using Foodify10.Models;

namespace Foodify10.Services.Interfaces
{
    public interface IShoppingListStorageService
    {
        List<ShoppingListModel> GetAll();
        ShoppingListModel? GetById(string id);
        void Save(ShoppingListModel list);
        void SaveAll(List<ShoppingListModel> lists);
        void Delete(string id);
    }
}