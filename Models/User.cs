using System.ComponentModel.DataAnnotations.Schema;

namespace TradingSimulator_Backend.Models{
    [Table("users")]
    public class User{
        [Column("id")]
        public long Id { get; set; } 

        [Column("username")]
        public required string Username { get; set; }

        [Column("password")]
        public required string Password { get; set; }

        [Column("investedamount")]
        public float InvestedAmount { get; set; } 

        [Column("currentvalue")]
        public float CurrentValue { get; set; }

        [Column("profitloss")]
        public float ProfitLoss { get; set; }

        public Friends? Friends { get; set; }
    }
}

