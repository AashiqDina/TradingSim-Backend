
public class Stock{
    [Column("id")]
    public long Id { get; set; }

    [Column("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [Column("purchase_price")]
    public decimal PurchasePrice { get; set; }

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("current_price")]
    public decimal CurrentPrice { get; set; }

    [Column("portfolio_id")]
    public long PortfolioId { get; set; }

    [ForeignKey("PortfolioId")]
    [System.Text.Json.Serialization.JsonIgnore]
    public Portfolio Portfolio { get; set; } = null!;

    public decimal TotalValue => CurrentPrice * Quantity;
    public decimal ProfitLoss => (CurrentPrice - PurchasePrice) * Quantity;

    public ICollection<StockHistory> History { get; set; } = new List<StockHistory>();
}


