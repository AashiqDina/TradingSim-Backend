using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Models
{
    public class Portfolio
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public List<Stock> Stocks { get; set; } = new List<Stock>();

        [NotMapped]
        public decimal TotalInvested => Stocks.Sum(stock => stock.PurchasePrice * stock.Quantity);

        [NotMapped]
        public decimal CurrentValue => Stocks.Sum(stock => stock.TotalValue);

        [NotMapped]
        public decimal ProfitLoss => CurrentValue - TotalInvested;
    }
}
