using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TradingSimulator_Backend.Models
{
    public class Stock
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("symbol")]
        public required string Symbol { get; set; }

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("current_price")]
        public decimal CurrentPrice { get; set; }

        [Column("portfolio_id")]
        public long PortfolioId { get; set; }

        [ForeignKey("PortfolioId")]
        [JsonIgnore]
        public Portfolio? Portfolio { get; set; }

        [NotMapped]
        public decimal TotalValue => CurrentPrice * Quantity;

        [NotMapped]
        public decimal ProfitLoss => (CurrentPrice - PurchasePrice) * Quantity;

        public ICollection<StockHistory> History { get; set; } = new List<StockHistory>();
    }
}
