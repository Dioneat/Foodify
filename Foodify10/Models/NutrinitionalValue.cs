namespace Foodify10.Models
{
    public record NutrinitionalValue
    {
        public NutrinitionalValue(string calories, string proteins, string fats, string carbohydrates)
        {
            Calories = calories;
            Proteins = proteins;
            Fats = fats;
            Carbohydrates = carbohydrates;
        }
        public readonly string Proteins;
        public readonly string Fats;
        public readonly string Calories;
        public readonly string Carbohydrates;
    }
}