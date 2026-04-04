using Foodify10.Models;
using Foodify10.Models.JsonModels;
using Foodify10.Services.Interfaces;

namespace Foodify10.Services
{
    public class ComparisonFlowService : IComparisonFlowService
    {
        public async Task AddProductToComparisonAsync(ProductData product)
        {
            ComparisonManager.EnsureLoaded();

            if (product == null)
            {
                await Shell.Current.DisplayAlertAsync("Загрузка", "Пожалуйста, подождите завершения загрузки данных.", "OK");
                return;
            }

            var comparisonProduct = MapToComparisonProduct(product);

            if (ComparisonManager.Groups.Count == 0)
            {
                string groupName = await Shell.Current.DisplayPromptAsync(
                    "Сравнение",
                    "Введите название для новой группы (например, 'Йогурты'):",
                    "Создать",
                    "Отмена");

                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    await CreateGroupAndAddProduct(groupName, comparisonProduct);
                }

                return;
            }

            var options = ComparisonManager.Groups.Select(g => g.Name).ToList();
            options.Add("➕ Создать новую группу");

            string action = await Shell.Current.DisplayActionSheetAsync(
                "Добавить в группу",
                "Отмена",
                null,
                options.ToArray());

            if (action == "Отмена" || action == null)
                return;

            if (action == "➕ Создать новую группу")
            {
                string groupName = await Shell.Current.DisplayPromptAsync(
                    "Новая группа",
                    "Введите название:",
                    "Создать",
                    "Отмена");

                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    await CreateGroupAndAddProduct(groupName, comparisonProduct);
                }

                return;
            }

            var selectedGroup = ComparisonManager.Groups.FirstOrDefault(g => g.Name == action);
            if (selectedGroup == null)
                return;

            if (selectedGroup.Products.Count >= 3)
            {
                await Shell.Current.DisplayAlertAsync("Лимит", "В группе не может быть больше 3 продуктов.", "OK");
                return;
            }

            if (selectedGroup.Products.Any(p => p.Barcode == comparisonProduct.Barcode))
            {
                await Shell.Current.DisplayAlertAsync("Инфо", "Этот продукт уже есть в списке.", "OK");
                return;
            }

            selectedGroup.Products.Add(comparisonProduct);
            ComparisonManager.Save();

            await Shell.Current.DisplayAlertAsync("Готово", $"Добавлено в '{selectedGroup.Name}'", "OK");
        }

        private async Task CreateGroupAndAddProduct(string name, ComparisonProduct product)
        {
            ComparisonManager.EnsureLoaded();

            var newGroup = new ComparisonGroup { Name = name };
            newGroup.Products.Add(product);
            ComparisonManager.Groups.Add(newGroup);
            ComparisonManager.Save();

            await Shell.Current.DisplayAlertAsync("Успешно", $"Группа '{name}' создана и продукт добавлен.", "OK");
        }

        private static ComparisonProduct MapToComparisonProduct(ProductData product)
        {
            return new ComparisonProduct
            {
                Name = product.Name ?? string.Empty,
                Barcode = product.Barcode ?? string.Empty,
                Composition = product.Composition ?? string.Empty,
                Calories = product.NutrinitionalValue?.Calories ?? "0",
                Proteins = product.NutrinitionalValue?.Proteins ?? "0",
                Fats = product.NutrinitionalValue?.Fats ?? "0",
                Carbohydrates = product.NutrinitionalValue?.Carbohydrates ?? "0"
            };
        }
    }
}