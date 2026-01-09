using System.ComponentModel.DataAnnotations.Schema;

namespace TradingSimulator_Backend.Models{
    [Table("users")]
    public class User{
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public required string Username { get; set; }

        [Column("password")]
        public required string Password { get; set; }

        [Column("investedamount")]
        public decimal InvestedAmount { get; set; }

        [Column("currentvalue")]
        public decimal CurrentValue { get; set; }

        [Column("profitloss")]
        public decimal ProfitLoss { get; set; }

        public Friends? Friends { get; set; }
    }
}
