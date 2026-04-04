namespace Foodify10.Models
{
    public class HistoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public Color StatusColor { get; set; }
        public string ScanTime { get; set; }
    }
}
