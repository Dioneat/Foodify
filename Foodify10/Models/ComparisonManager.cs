using System.Diagnostics;
using System.Text.Json;

namespace Foodify10.Models
{
    public static class ComparisonManager
    {
        private const string StorageKey = "comparison_groups_v2";
        private static bool _isLoaded;

        public static List<ComparisonGroup> Groups { get; private set; } = new();

        public static void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            Load();
            _isLoaded = true;
        }

        public static void Load()
        {
            try
            {
                var json = Preferences.Default.Get(StorageKey, string.Empty);
                Debug.WriteLine($"[ComparisonManager.Load] JSON length: {json?.Length ?? 0}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    Groups = new List<ComparisonGroup>();
                    return;
                }

                Groups = JsonSerializer.Deserialize<List<ComparisonGroup>>(json) ?? new List<ComparisonGroup>();
                Debug.WriteLine($"[ComparisonManager.Load] Groups loaded: {Groups.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ComparisonManager.Load] ERROR: {ex}");
                Groups = new List<ComparisonGroup>();
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Groups);
                Debug.WriteLine($"[ComparisonManager.Save] JSON length: {json.Length}");
                Preferences.Default.Set(StorageKey, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ComparisonManager.Save] ERROR: {ex}");
            }
        }

        public static void Clear()
        {
            Groups.Clear();
            Save();
        }
    }
}