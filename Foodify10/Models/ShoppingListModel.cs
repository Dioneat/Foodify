using CommunityToolkit.Mvvm.ComponentModel;

namespace Foodify10.Models
{
    public partial class ShoppingItem : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private bool isBought;
    }

    public class ShoppingListModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public List<ShoppingItem> Items { get; set; } = new();
        public DateTime? ReminderDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}