public class Stock
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal TotalValue => CurrentPrice * Quantity;
    public decimal ProfitLoss => (CurrentPrice - PurchasePrice) * Quantity;

    public int PortfolioId { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public Portfolio Portfolio { get; set; }

    public ICollection<StockHistory> History { get; set; } = new List<StockHistory>();
}
