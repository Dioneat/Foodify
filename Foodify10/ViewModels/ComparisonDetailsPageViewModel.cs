using CommunityToolkit.Mvvm.ComponentModel;
using Foodify10.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Foodify10.ViewModels
{
    public partial class ComparisonDetailsPageViewModel : ObservableObject
    {
        public ObservableCollection<ComparisonRowModel> Rows { get; } = new();

        [ObservableProperty]
        private int productCount;
        [ObservableProperty]
        private string title = string.Empty;
        public void Load(ComparisonGroup group)
        {
            ComparisonManager.EnsureLoaded();

            Rows.Clear();

            Title = group?.Name ?? "Сравнение";

            var products = group?.Products;
            if (products == null || products.Count == 0)
                return;

            ProductCount = products.Count;
            var cellWidth = GetCellWidth(ProductCount);

            Rows.Add(new ComparisonRowModel
            {
                Title = "Продукт",
                IsHeader = true,
                Cells = products.Select(p => new ComparisonCellModel
                {
                    MainText = p.Name,
                    CellWidth = cellWidth
                }).ToList()
            });

            AddNutritionRow("Ккал", products.Select(p => p.Calories).ToList(), true, cellWidth);
            AddNutritionRow("Белки", products.Select(p => p.Proteins).ToList(), false, cellWidth);
            AddNutritionRow("Жиры", products.Select(p => p.Fats).ToList(), true, cellWidth);
            AddNutritionRow("Углев.", products.Select(p => p.Carbohydrates).ToList(), true, cellWidth);

            var additivesCount = products
                .Select(p => CountAdditives(p.Composition).ToString())
                .ToList();
            AddNutritionRow("Добавок", additivesCount, true, cellWidth);
        }
        private double GetCellWidth(int productCount)
        {
            return productCount switch
            {
                1 => 260,
                2 => 150,
                3 => 120,
                _ => 120
            };
        }
        private void AddNutritionRow(string title, List<string?> rawValues, bool isLowerBetter, double cellWidth)
        {
            var values = rawValues.Select(ParseDouble).ToList();

            if (values.All(v => v == 0))
            {
                Rows.Add(new ComparisonRowModel
                {
                    Title = title,
                    Cells = values.Select(v => new ComparisonCellModel
                    {
                        MainText = "0",
                        CellWidth = cellWidth
                    }).ToList()
                });
                return;
            }

            double bestValue = isLowerBetter ? values.Min() : values.Max();
            double baseValue = values[0];

            var cells = new List<ComparisonCellModel>();

            for (int i = 0; i < values.Count; i++)
            {
                string diffText = string.Empty;
                Color diffColor = Colors.Transparent;

                if (i > 0 && baseValue > 0)
                {
                    double diffPercent = ((values[i] - baseValue) / baseValue) * 100;
                    string sign = diffPercent > 0 ? "+" : string.Empty;
                    diffText = $"{sign}{Math.Round(diffPercent, 1)}%";
                    diffColor = diffText.StartsWith("-")
                        ? Color.FromArgb("#27AE60")
                        : Color.FromArgb("#E74C3C");
                }

                cells.Add(new ComparisonCellModel
                {
                    MainText = values[i].ToString(),
                    DiffText = diffText,
                    DiffTextColor = diffColor,
                    BackgroundColor = (values[i] == bestValue && values.Distinct().Count() > 1)
                        ? Color.FromArgb("#E8F5E9")
                        : Colors.Transparent,
                    CellWidth = cellWidth
                });
            }

            Rows.Add(new ComparisonRowModel
            {
                Title = title,
                Cells = cells
            });
        }

        private static int CountAdditives(string? composition)
        {
            if (string.IsNullOrWhiteSpace(composition))
                return 0;

            return Regex.Matches(composition, @"[ЕEеe]\s*-?\d{3,4}").Count;
        }

        private static double ParseDouble(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            string cleanInput = input.Replace(",", ".").Trim();

            if (double.TryParse(
                cleanInput,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double result))
            {
                return result;
            }

            return 0;
        }
    }
}