using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

public class Portfolio{
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public List<Stock> Stocks { get; set; } = new();

    [NotMapped]
    public decimal TotalInvested => Stocks.Sum(stock => stock.PurchasePrice * stock.Quantity);

    [NotMapped]
    public decimal CurrentValue => Stocks.Sum(stock => stock.TotalValue);

    [NotMapped]
    public decimal ProfitLoss => CurrentValue - TotalInvested;
}

