using System.Text.RegularExpressions;

namespace Foodify10.Services
{
    public enum ProductKind
    {
        Food,
        Drink
    }

    public enum TrafficLightNutrient
    {
        Fat,
        Saturates,
        Sugars,
        Salt
    }

    public enum ScoreConfidence
    {
        High,
        Medium,
        Low
    }

    public sealed record NovaResult(
        int Group,
        Color BadgeColor,
        string Title,
        string Explanation,
        bool IsEstimated);

    public sealed record NutriScoreEstimateResult(
        string DisplayGrade,
        string BestGrade,
        string WorstGrade,
        int BestRawScore,
        int WorstRawScore,
        Color BadgeColor,
        string Explanation,
        bool IsEstimated,
        ScoreConfidence Confidence);

    public sealed record TrafficLightResult(
        Color Color,
        string Status,
        string Explanation,
        bool IsEstimated = false);

    public static class ScoringService
    {
        private static readonly Color GreenColor = Color.FromArgb("#27AE60");
        private static readonly Color AmberColor = Color.FromArgb("#F39C12");
        private static readonly Color RedColor = Color.FromArgb("#E74C3C");
        private static readonly Color GrayColor = Colors.Gray;

        // =========================
        // NOVA
        // =========================

        public static NovaResult CalculateNovaEstimate(string? composition)
        {
            if (string.IsNullOrWhiteSpace(composition))
            {
                return new NovaResult(
                    0,
                    GrayColor,
                    "Нет данных",
                    "Состав не указан, степень обработки оценить нельзя.",
                    true);
            }

            var source = NormalizeComposition(composition);

            var cosmeticAdditives = new[]
            {
                "ароматизатор", "ароматизаторы",
                "краситель", "красители",
                "подсластитель", "подсластители",
                "усилитель вкуса", "усилители вкуса",
                "эмульгатор", "эмульгаторы",
                "стабилизатор", "стабилизаторы",
                "загуститель", "загустители",
                "консервант", "консерванты",
                "антиокислитель", "антиокислители",
                "глазирователь", "глазирователи",
                "пеногаситель",
                "разрыхлитель"
            };

            var industrialMarkers = new[]
            {
                "гидрогенизирован",
                "частично гидрогенизирован",
                "мальтодекстрин",
                "глюкозно-фруктозный сироп",
                "фруктозный сироп",
                "инвертный сироп",
                "модифицированный крахмал",
                "изолят",
                "изолят белка",
                "концентрат белка"
            };

            var culinaryIngredients = new[]
            {
                "соль", "сахар", "масло", "уксус", "мед", "сироп"
            };

            bool hasEAdditives = Regex.IsMatch(source, @"\b[еe]\s*-?\d{3,4}\b", RegexOptions.IgnoreCase);
            int cosmeticHits = cosmeticAdditives.Count(m => source.Contains(m));
            int industrialHits = industrialMarkers.Count(m => source.Contains(m));
            int culinaryHits = culinaryIngredients.Count(m => source.Contains(m));

            int ingredientCount = composition
                .Split(',', ';')
                .Select(x => x.Trim())
                .Count(x => !string.IsNullOrWhiteSpace(x));

            if (hasEAdditives || cosmeticHits > 0 || industrialHits > 0)
            {
                var reasons = new List<string>();

                if (hasEAdditives)
                    reasons.Add("обнаружены E-добавки");

                if (cosmeticHits > 0)
                    reasons.Add($"найдены пищевые добавки/маркеры промышленной переработки ({cosmeticHits})");

                if (industrialHits > 0)
                    reasons.Add($"найдены промышленные ингредиенты ({industrialHits})");

                return new NovaResult(
                    4,
                    RedColor,
                    "Ультра-обработанный",
                    $"Оценка NOVA-4: {string.Join(", ", reasons)}.",
                    true);
            }

            if (culinaryHits > 0)
            {
                return new NovaResult(
                    3,
                    AmberColor,
                    "Обработанный",
                    "Есть кулинарные ингредиенты вроде соли, сахара, масла или сиропов. Это ближе к NOVA-3.",
                    true);
            }

            if (ingredientCount >= 2)
            {
                return new NovaResult(
                    2,
                    Color.FromArgb("#F1C40F"),
                    "Минимально обработанный",
                    "Состав короткий и без явных признаков глубокой промышленной переработки.",
                    true);
            }

            return new NovaResult(
                1,
                GreenColor,
                "Необработанный / минимально обработанный",
                "Состав очень простой и без явных промышленных маркеров.",
                true);
        }

        // Старый метод — оставлен для совместимости
        public static (int Score, Color BadgeColor, string Title, string Explanation) CalculateNova(string composition)
        {
            var result = CalculateNovaEstimate(composition);
            return (result.Group, result.BadgeColor, result.Title, result.Explanation);
        }

        // =========================
        // NUTRI-SCORE (умная estimate-модель)
        // =========================

        public static NutriScoreEstimateResult CalculateNutriScoreSmart(
            double? calories,
            double? proteins,
            string? composition,
            double? carbs = null,
            double? fats = null,
            double? sugars = null,
            double? saturatedFats = null,
            double? salt = null,
            double? fiber = null,
            ProductKind kind = ProductKind.Food)
        {
            if (!calories.HasValue || !proteins.HasValue)
            {
                return new NutriScoreEstimateResult(
                    "-",
                    "-",
                    "-",
                    0,
                    0,
                    GrayColor,
                    "Недостаточно базовых данных даже для приблизительной оценки.",
                    true,
                    ScoreConfidence.Low);
            }

            var normalizedComposition = NormalizeComposition(composition);

            var sugarRange = EstimateSugarRange(carbs, sugars, normalizedComposition);
            var satFatRange = EstimateSatFatRange(fats, saturatedFats, normalizedComposition);
            var saltRange = EstimateSaltRange(salt, normalizedComposition);

            int bestScore = CalculateNutriRawScore(
                calories.Value,
                sugarRange.Min,
                satFatRange.Min,
                saltRange.Min,
                proteins.Value,
                fiber,
                kind);

            int worstScore = CalculateNutriRawScore(
                calories.Value,
                sugarRange.Max,
                satFatRange.Max,
                saltRange.Max,
                proteins.Value,
                fiber,
                kind);

            var bestGrade = GradeFromScore(bestScore, kind);
            var worstGrade = GradeFromScore(worstScore, kind);

            var displayGrade = bestGrade == worstGrade
                ? bestGrade
                : $"{bestGrade}–{worstGrade}";

            var confidence = GetConfidence(
                sugars.HasValue,
                saturatedFats.HasValue,
                salt.HasValue,
                fiber.HasValue);

            var badgeColor = GradeColor(bestGrade == worstGrade ? bestGrade : worstGrade);

            string explanation =
                $"Оценка {displayGrade}. " +
                $"Сахара: {FormatRange(sugarRange.Min, sugarRange.Max)} г, " +
                $"насыщенные жиры: {FormatRange(satFatRange.Min, satFatRange.Max)} г, " +
                $"соль: {FormatRange(saltRange.Min, saltRange.Max)} г. " +
                $"Белок: {proteins.Value:0.#} г, энергия: {calories.Value:0.#} ккал. " +
                $"Уверенность: {ConfidenceToText(confidence)}.";

            return new NutriScoreEstimateResult(
                displayGrade,
                bestGrade,
                worstGrade,
                bestScore,
                worstScore,
                badgeColor,
                explanation,
                true,
                confidence);
        }

        // Старый метод — оставлен для совместимости.
        // ВАЖНО: параметр sodium здесь трактуется как СОЛЬ (г), потому что в проекте у вас именно так и используется.
        public static (string Score, Color BadgeColor, string Explanation) CalculateNutriScore(
            double calories,
            double sugar,
            double satFats,
            double sodium,
            double proteins)
        {
            var result = CalculateNutriScoreSmart(
                calories: calories,
                proteins: proteins,
                composition: null,
                carbs: null,
                fats: null,
                sugars: sugar,
                saturatedFats: satFats,
                salt: sodium,
                fiber: null,
                kind: ProductKind.Food);

            return (result.DisplayGrade, result.BadgeColor, result.Explanation);
        }

        private static int CalculateNutriRawScore(
            double calories,
            double sugars,
            double saturatedFats,
            double salt,
            double proteins,
            double? fiber,
            ProductKind kind)
        {
            // Это estimate-модель, близкая к классической логике Nutri-Score для foods.
            // Для напитков используем те же thresholds только как грубое приближение.
            double energyKJ = calories * 4.184;
            double sodiumMg = salt * 400.0;

            int negativePoints =
                PointsByThreshold(energyKJ, new[] { 335d, 670d, 1005d, 1340d, 1675d, 2010d, 2345d, 2680d, 3015d, 3350d }) +
                PointsByThreshold(sugars, new[] { 4.5, 9d, 13.5, 18d, 22.5, 27d, 31d, 36d, 40d, 45d }) +
                PointsByThreshold(saturatedFats, new[] { 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d }) +
                PointsByThreshold(sodiumMg, new[] { 90d, 180d, 270d, 360d, 450d, 540d, 630d, 720d, 810d, 900d });

            int positiveProtein = PointsByThreshold(proteins, new[] { 1.6, 3.2, 4.8, 6.4, 8.0 });
            int positiveFiber = fiber.HasValue
                ? PointsByThreshold(fiber.Value, new[] { 0.9, 1.9, 2.8, 3.7, 4.7 })
                : 0;

            int positivePoints = positiveProtein + positiveFiber;

            if (negativePoints >= 11 && !fiber.HasValue)
            {
                // Если данных по клетчатке нет, не даём белку "слишком сильно вытягивать" плохой профиль.
                positivePoints = Math.Min(positivePoints, 2);
            }

            return negativePoints - positivePoints;
        }

        private static (double Min, double Max) EstimateSugarRange(
            double? carbs,
            double? sugars,
            string composition)
        {
            if (sugars.HasValue)
                return (Math.Max(0, sugars.Value), Math.Max(0, sugars.Value));

            if (!carbs.HasValue)
                return (0, 30);

            double min = 0;
            double max = Math.Max(0, carbs.Value);

            if (ContainsAny(composition,
                    "сахар", "глюкозно-фруктозный сироп", "сироп",
                    "фруктоза", "декстроза", "патока", "мед"))
            {
                min = Math.Max(min, carbs.Value * 0.15);
            }

            if (ContainsEarlyIngredient(composition, "сахар", "глюкозно-фруктозный сироп", "сироп"))
            {
                min = Math.Max(min, carbs.Value * 0.35);
            }

            return (Math.Min(min, max), max);
        }

        private static (double Min, double Max) EstimateSatFatRange(
            double? fats,
            double? saturatedFats,
            string composition)
        {
            if (saturatedFats.HasValue)
                return (Math.Max(0, saturatedFats.Value), Math.Max(0, saturatedFats.Value));

            if (!fats.HasValue)
                return (0, 12);

            double min = 0;
            double max = Math.Max(0, fats.Value);

            if (ContainsAny(composition,
                    "пальмовое масло", "кокосовое масло", "сливочное масло",
                    "сливки", "молочный жир", "животный жир"))
            {
                min = Math.Max(min, fats.Value * 0.25);
            }

            return (Math.Min(min, max), max);
        }

        private static (double Min, double Max) EstimateSaltRange(
            double? salt,
            string composition)
        {
            if (salt.HasValue)
                return (Math.Max(0, salt.Value), Math.Max(0, salt.Value));

            double min = 0;
            double max = 3.0;

            if (ContainsAny(composition, "соль"))
                min = 0.05;

            if (ContainsEarlyIngredient(composition, "соль"))
                min = 0.3;

            return (min, max);
        }

        private static string GradeFromScore(int rawScore, ProductKind kind)
        {
            // Для напитков здесь тоже используется food-like approximation,
            // чтобы не ломать проект при неполных данных.
            if (rawScore <= -1) return "A";
            if (rawScore <= 2) return "B";
            if (rawScore <= 10) return "C";
            if (rawScore <= 18) return "D";
            return "E";
        }

        private static Color GradeColor(string grade)
        {
            return grade switch
            {
                "A" => Color.FromArgb("#008137"),
                "B" => Color.FromArgb("#85BB2F"),
                "C" => Color.FromArgb("#FECB02"),
                "D" => Color.FromArgb("#EE8100"),
                "E" => Color.FromArgb("#E63E11"),
                _ => GrayColor
            };
        }

        private static ScoreConfidence GetConfidence(
            bool hasSugar,
            bool hasSatFat,
            bool hasSalt,
            bool hasFiber)
        {
            int known = 0;
            if (hasSugar) known++;
            if (hasSatFat) known++;
            if (hasSalt) known++;
            if (hasFiber) known++;

            if (known >= 3) return ScoreConfidence.High;
            if (known >= 1) return ScoreConfidence.Medium;
            return ScoreConfidence.Low;
        }

        private static string ConfidenceToText(ScoreConfidence confidence)
        {
            return confidence switch
            {
                ScoreConfidence.High => "высокая",
                ScoreConfidence.Medium => "средняя",
                ScoreConfidence.Low => "низкая",
                _ => "неизвестно"
            };
        }

        private static string FormatRange(double min, double max)
        {
            if (Math.Abs(min - max) < 0.001)
                return $"{min:0.##}";

            return $"{min:0.##}–{max:0.##}";
        }

        // =========================
        // TRAFFIC LIGHT
        // =========================

        public static TrafficLightResult GetTrafficLight(
            TrafficLightNutrient nutrient,
            double? valuePer100,
            ProductKind kind = ProductKind.Food)
        {
            if (!valuePer100.HasValue)
            {
                return new TrafficLightResult(
                    GrayColor,
                    "Нет данных",
                    "Недостаточно данных для классификации.",
                    true);
            }

            var value = valuePer100.Value;
            var (greenLimit, redLimit, label) = GetThresholds(nutrient, kind);

            if (value <= greenLimit)
            {
                return new TrafficLightResult(
                    GreenColor,
                    "Низкое",
                    $"{label}: низкое содержание на 100{(kind == ProductKind.Food ? " г" : " мл")}.");
            }

            if (value <= redLimit)
            {
                return new TrafficLightResult(
                    AmberColor,
                    "Среднее",
                    $"{label}: среднее содержание на 100{(kind == ProductKind.Food ? " г" : " мл")}.");
            }

            return new TrafficLightResult(
                RedColor,
                "Высокое",
                $"{label}: высокое содержание на 100{(kind == ProductKind.Food ? " г" : " мл")}.");
        }

        // Старый метод — оставлен для совместимости
        public static (Color Color, string Status) GetTrafficLight(double value, double limitGreen, double limitRed)
        {
            if (value <= limitGreen) return (GreenColor, "Норма");
            if (value <= limitRed) return (AmberColor, "Средне");
            return (RedColor, "Много");
        }

        private static (double Green, double Red, string Label) GetThresholds(
            TrafficLightNutrient nutrient,
            ProductKind kind)
        {
            if (kind == ProductKind.Food)
            {
                return nutrient switch
                {
                    TrafficLightNutrient.Fat => (3.0, 17.5, "Жир"),
                    TrafficLightNutrient.Saturates => (1.5, 5.0, "Насыщенные жиры"),
                    TrafficLightNutrient.Sugars => (5.0, 22.5, "Сахара"),
                    TrafficLightNutrient.Salt => (0.3, 1.5, "Соль"),
                    _ => throw new ArgumentOutOfRangeException(nameof(nutrient))
                };
            }

            return nutrient switch
            {
                TrafficLightNutrient.Fat => (1.5, 8.75, "Жир"),
                TrafficLightNutrient.Saturates => (0.75, 2.5, "Насыщенные жиры"),
                TrafficLightNutrient.Sugars => (2.5, 11.25, "Сахара"),
                TrafficLightNutrient.Salt => (0.3, 0.75, "Соль"),
                _ => throw new ArgumentOutOfRangeException(nameof(nutrient))
            };
        }

        // =========================
        // HELPERS
        // =========================

        private static int PointsByThreshold(double value, IReadOnlyList<double> thresholds)
        {
            int points = 0;

            foreach (var threshold in thresholds)
            {
                if (value > threshold)
                    points++;
            }

            return points;
        }

        private static string NormalizeComposition(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input
                .ToLowerInvariant()
                .Replace('ё', 'е');
        }

        private static bool ContainsAny(string source, params string[] markers)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            return markers.Any(source.Contains);
        }

        private static bool ContainsEarlyIngredient(string source, params string[] markers)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            var ingredients = source
                .Split(',', ';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(3)
                .ToList();

            return ingredients.Any(i => markers.Any(i.Contains));
        }
    }
}