using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Data;
using TradingSimulator_Backend.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using TradingSimulator_Backend.Services;


[Route("api/portfolio")]
[ApiController]
public class PortfolioController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IStockService _stockService; 

    public PortfolioController(AppDbContext context, IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetPortfolio(int userId)
    {
        var portfolio = await _context.Portfolios
                                      .Include(p => p.User)
                                      .Include(p => p.Stocks)
                                      .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            return NotFound("Portfolio not found.");
        }



        return Ok(portfolio);
    }

    [HttpGet("{userId}/stocks")]
    public async Task<IActionResult> GetStocksInPortfolio(int userId)
    {
        var portfolio = await _context.Portfolios
                                    .Include(p => p.Stocks)
                                    .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            return NotFound("Portfolio not found.");
        }

        return Ok(portfolio.Stocks); // Return the stocks
    }

    [HttpPost]
    public async Task<IActionResult> CreatePortfolio([FromBody] Portfolio portfolio)
    {
        // Check if the user exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == portfolio.UserId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // If the user exists, associate the portfolio with the user
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPortfolio), new { userId = portfolio.UserId }, portfolio);
    }

    

    // Buy Stock (Updated)
    [HttpPost("{userId}/stocks")]
    public async Task<IActionResult> BuyStock(int userId, [FromBody] StockPurchaseRequest request)
    {
        var portfolio = await _context.Portfolios
                                    .Include(p => p.Stocks)
                                    .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            return NotFound("Portfolio not found.");
        }

        var response = await _stockService.GetStockPriceAsync(request.Symbol);

        // Handle API errors or missing data
        if (response.HasError || !response.Data.HasValue)
        {
            return BadRequest(new
            {
                Message = "Failed to fetch stock price.",
                Symbol = request.Symbol,
                ErrorCode = response.ErrorCode
            });
        }

        _stockService.updateTrendingMap(request.Symbol);

        var stockPrice = response.Data.Value;

        var stock = new Stock
        {
            Symbol = request.Symbol,
            PurchasePrice = stockPrice,
            Quantity = request.Quantity,
            CurrentPrice = stockPrice
        };

        portfolio.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Stock purchased successfully.",
            Stock = stock
        });
    }

    // Change this
// public class StockPriceUpdaterService : BackgroundService

    // [HttpPut("{userId}/stocks/update")]
    // public async Task<IActionResult> UpdateStocksInPortfolio(int userId)
    // {
    //     var portfolio = await _context.Portfolios
    //                                 .Include(p => p.User)
    //                                 .Include(p => p.Stocks)
    //                                 .FirstOrDefaultAsync(p => p.UserId == userId);

    //     if (portfolio == null)
    //     {
    //         return NotFound("Portfolio not found.");
    //     }

    //     var stockSymbols = portfolio.Stocks.Select(s => s.Symbol).ToList();

    //     var stockPrices = await _stockService.GetMultipleStockPricesAsync(stockSymbols);

    //     foreach (var stock in portfolio.Stocks)
    //     {
    //         if (stockPrices.TryGetValue(stock.Symbol, out var stockPrice))
    //         {
    //             if (!stockPrice.HasError && stockPrice.Data.HasValue)
    //             {
    //                 stock.CurrentPrice = stockPrice.Data.Value;

    //                 var utcToday = DateTime.UtcNow.Date;
    //                 var utcTomorrow = utcToday.AddDays(1);

    //                 var existingHistory = await _context.StockHistory
    //                     .FirstOrDefaultAsync(h => 
    //                         h.StockId == stock.Id && 
    //                         h.Timestamp >= utcToday && 
    //                         h.Timestamp < utcTomorrow);

    //                 if (existingHistory == null)
    //                 {
    //                     _context.StockHistory.Add(new StockHistory
    //                     {
    //                         StockId = stock.Id,
    //                         Timestamp = DateTime.UtcNow,
    //                         Price = stockPrice.Data.Value,
    //                         Quantity = stock.Quantity
    //                     });
    //                 }
    //                 else
    //                 {
    //                     existingHistory.Price = stockPrice.Data.Value;
    //                     existingHistory.Quantity = stock.Quantity;
    //                     existingHistory.Timestamp = DateTime.UtcNow;
    //                 }

    //             }
    //             else
    //             {
    //                 Console.WriteLine($"Skipping update for {stock.Symbol} due to API error or missing data.");
    //             }
    //         }
    //     }

    //     await _context.SaveChangesAsync();

    //     if (portfolio.User != null)
    //     {
    //         portfolio.User.InvestedAmount = portfolio.TotalInvested;
    //         portfolio.User.CurrentValue = portfolio.CurrentValue;
    //         portfolio.User.ProfitLoss = portfolio.ProfitLoss;
    //     }

    //     await _context.SaveChangesAsync();

    //     return Ok(portfolio);
    // }

    [HttpPut("{userId}/stocks/update")]
    public async Task<IActionResult> UpdateStocksInPortfolio(int userId)
    {
        var portfolio = await _context.Portfolios
            .Include(p => p.User)
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    
        if (portfolio == null)
            return NotFound("Portfolio not found.");
    
        var stockSymbols = portfolio.Stocks.Select(s => s.Symbol).ToList();
        var stockPrices = await _stockService.GetMultipleStockPricesAsync(stockSymbols);
    
        foreach (var stock in portfolio.Stocks)
        {
            if (stockPrices.TryGetValue(stock.Symbol, out var stockPrice) &&
                !stockPrice.HasError &&
                stockPrice.Data.HasValue)
            {
                stock.CurrentPrice = stockPrice.Data.Value;
    
                var utcToday = DateTime.UtcNow.Date;
                var utcTomorrow = utcToday.AddDays(1);
    
                var existingHistory = await _context.StockHistory
                    .FirstOrDefaultAsync(h =>
                        h.StockId == stock.Id &&
                        h.Timestamp >= utcToday &&
                        h.Timestamp < utcTomorrow);
    
                if (existingHistory == null)
                {
                    _context.StockHistory.Add(new StockHistory
                    {
                        StockId = stock.Id,
                        Timestamp = DateTime.UtcNow,
                        Price = stockPrice.Data.Value,
                        Quantity = stock.Quantity
                    });
                }
                else
                {
                    existingHistory.Price = stockPrice.Data.Value;
                    existingHistory.Quantity = stock.Quantity;
                    existingHistory.Timestamp = DateTime.UtcNow;
                }
            }
        }
    
        // Update user totals AFTER stock prices are updated
        if (portfolio.User != null)
        {
            portfolio.User.InvestedAmount = portfolio.TotalInvested;
            portfolio.User.CurrentValue = portfolio.CurrentValue;
            portfolio.User.ProfitLoss = portfolio.ProfitLoss;
        }
    
        await _context.SaveChangesAsync();
    
        return Ok(portfolio);
    }

    [HttpDelete("{userId}/stocks/delete/{stockId}")]
    public async Task<IActionResult> RemoveStockFromPortfolio(int userId, int stockId)
    {
        var portfolio = await _context.Portfolios
                                      .Include(p => p.Stocks)
                                      .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
        {
            return NotFound("Portfolio not found.");
        }

        var stock = portfolio.Stocks.FirstOrDefault(s => s.Id == stockId);
        if (stock == null)
        {
            return NotFound("Stock not found in portfolio.");
        }

        portfolio.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        return Ok(portfolio);
    }

    [HttpGet("stocks/getHistory/{userId}")]
    public async Task<IActionResult> GetPortfolioHistory(int userId, [FromQuery] string range = "all")
    {
        Console.WriteLine("Range of History: " + range);
        var portfolio = await _context.Portfolios
            .Include(p => p.Stocks)
                .ThenInclude(s => s.History)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
            return NotFound("Portfolio not found.");

        DateTime cutoff = DateTime.MinValue;
        
        switch (range.ToLower())
        {
            case "week":
                cutoff = DateTime.UtcNow.AddDays(-7);
                break;
            case "month":
                cutoff = DateTime.UtcNow.AddMonths(-1);
                break;
            case "year":
                cutoff = DateTime.UtcNow.AddYears(-1);
                break;
        }

        var result = portfolio.Stocks.Select(stock =>
            {
                var filtered = stock.History
                    .Where(h => h.Timestamp >= cutoff)
                    .OrderBy(h => h.Timestamp)
                    .Select(h => new
                    {
                        h.Timestamp,
                        h.Price,
                        Quantity = (double)h.Quantity
                    })
                    .ToList();

                if (range.ToLower() == "year" || range.ToLower() == "all")
                {
                    int targetPoints = 200;
                    int totalPoints = filtered.Count;
                    int bucketSize = Math.Max(1, totalPoints / targetPoints);

                    filtered = filtered
                        .Select((h, i) => new { h, i })
                        .GroupBy(x => x.i / bucketSize)
                        .Select(g => new
                        {
                            Timestamp = g.First().h.Timestamp,
                            Price = g.Average(x => x.h.Price),
                            Quantity = g.Average(x => x.h.Quantity)
                        })
                        .ToList();
                }

                Console.WriteLine("DaHistory:" + filtered);

                return new
                {
                    StockId = stock.Id,
                    Symbol = stock.Symbol,
                    History = filtered
                };
            });

        return Ok(result);
    }

    [HttpDelete("{userId}/stocks/history/today")]
    public async Task<IActionResult> DeleteTodaysHistory(int userId)
    {
        var portfolio = await _context.Portfolios
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (portfolio == null)
            return NotFound("Portfolio not found.");

        var utcToday = DateTime.UtcNow.Date;
        var utcTomorrow = utcToday.AddDays(1);

        var stockIds = portfolio.Stocks.Select(s => s.Id).ToList();

        var historiesToDelete = await _context.StockHistory
            .Where(h => stockIds.Contains(h.StockId) && h.Timestamp >= utcToday && h.Timestamp < utcTomorrow)
            .ToListAsync();

        _context.StockHistory.RemoveRange(historiesToDelete);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            DeletedCount = historiesToDelete.Count,
            Message = $"Deleted {historiesToDelete.Count} history records for {utcToday:yyyy-MM-dd}"
        });
    }


}



