namespace Foodify10.Models
{
    public class LoyaltyCard
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string CardNumber { get; set; }
        public bool IsGenerated { get; set; } = true;
        public string ImagePath { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public string CardColor { get; set; } = "#F9F9F9";
    }
}
