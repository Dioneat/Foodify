using System.Text.RegularExpressions;

namespace Foodify10.Services
{
    public static class BarcodeNormalizationService
    {
        /// <summary>
        /// Приводит входную строку к коду товара для поиска.
        /// - Если это обычный EAN/GTIN в цифрах -> возвращает как есть.
        /// - Если это GS1 DataMatrix / Честный Знак -> пытается извлечь AI 01 (GTIN-14).
        ///   Для EAN-13 убирает ведущий 0.
        /// </summary>
        public static string NormalizeForProductSearch(string? rawBarcode)
        {
            if (string.IsNullOrWhiteSpace(rawBarcode))
                return string.Empty;

            string input = rawBarcode.Trim();

            if (Regex.IsMatch(input, @"^\d{8,14}$"))
            {
                if (input.Length == 14 && input.StartsWith("0"))
                    return input[1..];

                return input;
            }

            var match = Regex.Match(
                input,
                @"(?:^|[^\d])01(?<gtin>\d{14})(?!\d)",
                RegexOptions.Compiled);

            if (match.Success)
            {
                string gtin14 = match.Groups["gtin"].Value;

                if (gtin14.StartsWith("0"))
                    return gtin14[1..];

                return gtin14;
            }


            string digitsOnly = new string(input.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length >= 16 && digitsOnly.StartsWith("01"))
            {
                string gtin14 = digitsOnly.Substring(2, 14);

                if (gtin14.StartsWith("0"))
                    return gtin14[1..];

                return gtin14;
            }

            return input;
        }
    }
}