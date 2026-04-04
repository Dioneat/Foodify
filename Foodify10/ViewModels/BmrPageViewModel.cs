using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Foodify10.ViewModels
{
    public partial class BmrPageViewModel : ObservableObject
    {
        public ObservableCollection<string> ActivityOptions { get; } = new()
        {
            "Минимальная (сидячая работа)",
            "Низкая (тренировки 1-3 раза в неделю)",
            "Средняя (тренировки 3-5 раз в неделю)",
            "Высокая (ежедневные тренировки)"
        };

        [ObservableProperty]
        private bool isMale = true;

        [ObservableProperty]
        private string currentGoal = "Баланс";

        [ObservableProperty]
        private double height = 170;

        [ObservableProperty]
        private double weight = 70;

        [ObservableProperty]
        private double age = 25;

        [ObservableProperty]
        private int selectedActivityIndex = 0;

        [ObservableProperty]
        private string totalCaloriesText = "2100 ккал";

        [ObservableProperty]
        private string proteinsText = "120г";

        [ObservableProperty]
        private string fatsText = "70г";

        [ObservableProperty]
        private string carbsText = "250г";

        public string HeightText => $"{(int)Height} см";
        public string WeightText => $"{(int)Weight} кг";
        public string AgeText => $"{(int)Age} лет";

        public string MaleBackground => IsMale ? "#27AE60" : "#F5F5F5";
        public string MaleTextColor => IsMale ? "White" : "Black";

        public string FemaleBackground => !IsMale ? "#27AE60" : "#F5F5F5";
        public string FemaleTextColor => !IsMale ? "White" : "Black";

        public string LoseBackground => CurrentGoal == "Сброс" ? "#27AE60" : "#F5F5F5";
        public string LoseTextColor => CurrentGoal == "Сброс" ? "White" : "Black";

        public string KeepBackground => CurrentGoal == "Баланс" ? "#27AE60" : "#F5F5F5";
        public string KeepTextColor => CurrentGoal == "Баланс" ? "White" : "Black";

        public string GainBackground => CurrentGoal == "Набор" ? "#27AE60" : "#F5F5F5";
        public string GainTextColor => CurrentGoal == "Набор" ? "White" : "Black";

        public BmrPageViewModel()
        {
            Recalculate();
        }

        partial void OnIsMaleChanged(bool value)
        {
            OnPropertyChanged(nameof(MaleBackground));
            OnPropertyChanged(nameof(MaleTextColor));
            OnPropertyChanged(nameof(FemaleBackground));
            OnPropertyChanged(nameof(FemaleTextColor));
            Recalculate();
        }

        partial void OnCurrentGoalChanged(string value)
        {
            OnPropertyChanged(nameof(LoseBackground));
            OnPropertyChanged(nameof(LoseTextColor));
            OnPropertyChanged(nameof(KeepBackground));
            OnPropertyChanged(nameof(KeepTextColor));
            OnPropertyChanged(nameof(GainBackground));
            OnPropertyChanged(nameof(GainTextColor));
            Recalculate();
        }

        partial void OnHeightChanged(double value)
        {
            OnPropertyChanged(nameof(HeightText));
            Recalculate();
        }

        partial void OnWeightChanged(double value)
        {
            OnPropertyChanged(nameof(WeightText));
            Recalculate();
        }

        partial void OnAgeChanged(double value)
        {
            OnPropertyChanged(nameof(AgeText));
            Recalculate();
        }

        partial void OnSelectedActivityIndexChanged(int value)
        {
            Recalculate();
        }

        [RelayCommand]
        private void SetMale()
        {
            IsMale = true;
        }

        [RelayCommand]
        private void SetFemale()
        {
            IsMale = false;
        }

        [RelayCommand]
        private void SetGoal(string goal)
        {
            if (!string.IsNullOrWhiteSpace(goal))
                CurrentGoal = goal;
        }

        public void SetHeightFromText(string? text)
        {
            if (double.TryParse(text, out var value))
                Height = Math.Clamp(value, 100, 250);
        }

        public void SetWeightFromText(string? text)
        {
            if (double.TryParse(text, out var value))
                Weight = Math.Clamp(value, 30, 200);
        }

        public void SetAgeFromText(string? text)
        {
            if (double.TryParse(text, out var value))
                Age = Math.Clamp(value, 14, 100);
        }

        private void Recalculate()
        {
            double bmr = (10 * Weight) + (6.25 * Height) - (5 * Age);
            bmr = IsMale ? bmr + 5 : bmr - 161;

            double[] activityFactors = { 1.2, 1.375, 1.55, 1.725 };
            int index = SelectedActivityIndex >= 0 && SelectedActivityIndex < activityFactors.Length
                ? SelectedActivityIndex
                : 0;

            double maintenanceCalories = bmr * activityFactors[index];

            double targetCalories = maintenanceCalories;
            double pRatio = 0.3, fRatio = 0.3, cRatio = 0.4;

            if (CurrentGoal == "Сброс")
            {
                targetCalories = maintenanceCalories * 0.85;
                pRatio = 0.4; fRatio = 0.25; cRatio = 0.35;
            }
            else if (CurrentGoal == "Набор")
            {
                targetCalories = maintenanceCalories * 1.15;
                pRatio = 0.25; fRatio = 0.25; cRatio = 0.5;
            }

            double p = (targetCalories * pRatio) / 4;
            double f = (targetCalories * fRatio) / 9;
            double c = (targetCalories * cRatio) / 4;

            TotalCaloriesText = $"{(int)targetCalories} ккал";
            ProteinsText = $"{(int)p}г";
            FatsText = $"{(int)f}г";
            CarbsText = $"{(int)c}г";
        }
    }
}