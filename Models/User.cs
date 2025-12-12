namespace TradingSimulator_Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public decimal InvestedAmount { get; set;}
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }

        public Friends? Friends { get; set; }
    }
}
