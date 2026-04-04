namespace Foodify10.Models
{
    public class ComparisonCellModel
    {
        public string MainText { get; set; } = string.Empty;
        public string DiffText { get; set; } = string.Empty;
        public Color BackgroundColor { get; set; } = Colors.Transparent;
        public Color DiffTextColor { get; set; } = Colors.Transparent;

        public double CellWidth { get; set; } = 110;
    }

    public class ComparisonRowModel
    {
        public string Title { get; set; } = string.Empty;
        public List<ComparisonCellModel> Cells { get; set; } = new();
        public bool IsHeader { get; set; }
    }
}