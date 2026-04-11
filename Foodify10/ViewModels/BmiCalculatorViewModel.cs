using CommunityToolkit.Mvvm.ComponentModel;

namespace Foodify10.ViewModels
{
    public partial class BmiCalculatorViewModel : ObservableObject
    {
        private bool _isSyncing;

        private double _height = 170;
        public double Height
        {
            get => _height;
            set
            {
                if (SetProperty(ref _height, Math.Round(value)))
                {
                    if (!_isSyncing)
                    {
                        _isSyncing = true;
                        HeightText = _height.ToString();
                        _isSyncing = false;
                    }
                    CalculateBmi();
                }
            }
        }

        private string _heightText = "170";
        public string HeightText
        {
            get => _heightText;
            set
            {
                if (SetProperty(ref _heightText, value))
                {
                    if (!_isSyncing && double.TryParse(value, out double result))
                    {
                        _isSyncing = true;
                        Height = result;
                        _isSyncing = false;
                    }
                    CalculateBmi();
                }
            }
        }

        // --- БЛОК ВЕСА ---
        private double _weight = 70;
        public double Weight
        {
            get => _weight;
            set
            {
                if (SetProperty(ref _weight, Math.Round(value)))
                {
                    if (!_isSyncing)
                    {
                        _isSyncing = true;
                        WeightText = _weight.ToString();
                        _isSyncing = false;
                    }
                    CalculateBmi();
                }
            }
        }

        private string _weightText = "70";
        public string WeightText
        {
            get => _weightText;
            set
            {
                if (SetProperty(ref _weightText, value))
                {
                    if (!_isSyncing && double.TryParse(value, out double result))
                    {
                        _isSyncing = true;
                        Weight = result;
                        _isSyncing = false;
                    }
                    CalculateBmi();
                }
            }
        }

        [ObservableProperty]
        private double bmiScore;

        [ObservableProperty]
        private string bmiCategory = string.Empty;

        [ObservableProperty]
        private string bmiDescription = string.Empty;

        [ObservableProperty]
        private Color statusColor = Colors.Gray;

        public BmiCalculatorViewModel()
        {
            CalculateBmi();
        }

        private void CalculateBmi()
        {
            if (Height <= 0 || Weight <= 0 || string.IsNullOrWhiteSpace(HeightText) || string.IsNullOrWhiteSpace(WeightText))
            {
                BmiScore = 0;
                BmiCategory = "Введите данные";
                BmiDescription = "Пожалуйста, укажите ваш корректный рост и вес.";
                StatusColor = Colors.Gray;
                return;
            }

            double heightInMeters = Height / 100.0;
            BmiScore = Math.Round(Weight / (heightInMeters * heightInMeters), 1);

            UpdateBmiCategory();
        }

        private void UpdateBmiCategory()
        {
            if (BmiScore < 18.5)
            {
                BmiCategory = "Недостаточный вес";
                BmiDescription = "Вам стоит немного набрать массу. Добавьте в рацион больше питательных продуктов.";
                StatusColor = Color.FromArgb("#3498DB");
            }
            else if (BmiScore >= 18.5 && BmiScore < 25)
            {
                BmiCategory = "Норма";
                BmiDescription = "Отличный результат! Ваш вес находится в здоровом диапазоне, так держать.";
                StatusColor = Color.FromArgb("#27AE60");
            }
            else if (BmiScore >= 25 && BmiScore < 30)
            {
                BmiCategory = "Избыточный вес";
                BmiDescription = "Рекомендуется немного снизить калорийность рациона и добавить активности.";
                StatusColor = Color.FromArgb("#F39C12");
            }
            else
            {
                BmiCategory = "Ожирение";
                BmiDescription = "Риск для здоровья повышен. Желательно проконсультироваться с врачом-диетологом.";
                StatusColor = Color.FromArgb("#E74C3C");
            }
        }
    }
}