using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradingSimulator_Backend.Data;
using TradingSimulator_Backend.Services;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Controllers
{
    [Route("api/stocks")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly INewsService _newsService;
        private readonly AppDbContext _context; 

        public StockController(IStockService stockService, INewsService newsService, AppDbContext context)
        {
            _stockService = stockService;
            _newsService = newsService;
            _context = context; 
        }

        [HttpGet("{*symbol}")]
        public async Task<IActionResult> GetStockPrice(string symbol)
        {
            var response = await _stockService.GetStockPriceAsync(symbol);

            return Ok(new { Symbol = symbol, response = response });
        }

        [HttpGet("StockImage/{*symbol}")]
        public async Task<IActionResult> GetStockImage(string symbol)
        {
            var response = await _stockService.GetStockImage(symbol);

            return Ok(new { Symbol = symbol, Image = response });
        }

        [HttpGet("GetStockName/{*symbol}")]
        public async Task<IActionResult> GetStockName(string symbol)
        {
            var name = await _stockService.ConvertSymbolToName(symbol);
            Console.WriteLine(name);
            return Ok(name);
        }

        // [HttpGet("GetStockInfo/{*symbol}")]
        // public async Task<IActionResult> GetStockInfo(string symbol)
        // {
        //     var stockData = await _stockService.GetQuickData(symbol);

        //     if (stockData == (null, null, null, null, null, null))
        //     {
        //         Console.WriteLine("Data is unavailable.");
        //     }

        //     return Ok(new
        //     {
        //         LastUpdated = stockData.LastUpdated,
        //         LowPrice = stockData.LowPrice,
        //         HighPrice = stockData.HighPrice,
        //         FiftyTwoWeekRange = stockData.FiftyTwoWeekRange,
        //         ClosePrice = stockData.ClosePrice,
        //         PercentChange = stockData.PercentChange
        //     });
        // }

        [HttpGet("GetStockQuoteInfo/{*symbol}")]
        public async Task<IActionResult> GetStockQuoteInfo(string symbol)
        {
            var quoteData = await _stockService.FetchStockInfo(symbol);

            if (quoteData == null)
            {
                Console.WriteLine("No Data available");
            }

            return Ok(new {
                quoteData
            });
        }

        [HttpGet("GetAllStockLastUpdated")]
        public IActionResult GetAllStockLastUpdated()
        {

            var data = _stockService.GetAllLastUpdated();

            if (data == null)
            {
                Console.WriteLine("No data available");
            }

            return Ok(new
            {
                data
            });
        }

        [HttpGet("GetTrendingStocks")]
        public IActionResult GetTrendingStocks(){

            string[] trendingStocks = _stockService.getTrendingList();

            if (trendingStocks == null)
            {
                Console.WriteLine("An Error has occured");
                return BadRequest("An Unknown Error has occured");
            }

            return Ok(new{
                trendingStocks
            });
        }

        [HttpGet("GetStockLastUpdated/{*symbol}")]
        public IActionResult GetAllStockLastUpdated(string symbol){

            DateTime? data = _stockService.GetStockLastUpdated(symbol);

            if(data == null){
                Console.WriteLine("No data available");
            }

            return Ok(new {
                data
            });
        }

        [HttpGet("GetStockInfoLastUpdated/{*symbol}")]
        public IActionResult GetAllStockInfoLastUpdated(string symbol){

            var data = _stockService.GetStockInfoLastUpdated(symbol);

            if(data == null){
                Console.WriteLine("No data available");
            }

            return Ok(new {
                data
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStocks(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) 
            {
                return BadRequest("Symbol query is required.");
            }

            var stocks = await _context.Stocks
                                        .Where(s => s.Symbol.Contains(symbol, System.StringComparison.OrdinalIgnoreCase))
                                        .ToListAsync();

            if (stocks.Count == 0)
            {
                return NotFound("No stocks found.");
            }

            return Ok(stocks);
        }

        [HttpGet("GetCompanyDetails/{*symbol}")]
        public async Task<IActionResult> GetCompanyProfile(string symbol){

            var Profile = await _stockService.GetStockCompanyProfile(symbol);

            return Ok(new {
                Profile
            });
        }

        [HttpGet("GetStockNews/{*symbol}")]
        public async Task<IActionResult> getCompanyNews(string symbol)
        {

            var news = await _newsService.getCompanyNews(symbol);

            return Ok(news);
        }

        [HttpGet("GetMarketNews")]
        public async Task<IActionResult> getMarketNews()
        {
            var news = await _newsService.getMarketNews();
            return Ok(news);
        }

        [HttpGet("GetStockNewsLastUpdated/{*symbol}")]
        public IActionResult GetStockNewsLastUpdated(string symbol){

            DateTime? data = _newsService.getCompanyNewsLastUpdated(symbol);

            if(data == null){
                Console.WriteLine("No data available");
            }

            return Ok(new {
                data
            });
        }


        public class StockInfo
        {
            public string? Symbol { get; set; }
            public string? Logo { get; set; }
        }

        [HttpGet("GetStockList")]
        public async Task<ActionResult<Dictionary<string, StockInfo>>> GetStockMap()
        {
            var stocks = await _context.StockLogoName.ToListAsync();

            var stockMap = stocks
                .Where(s => !string.IsNullOrEmpty(s.Name))
                .ToDictionary(
                    s => s.Name!,
                    s => new StockInfo { Symbol = s.Symbol, Logo = s.Logo }
                );

            return Ok(stockMap);
        }

        [HttpGet("GetStocksFullHistory/{*symbol}")]
        public async Task<ActionResult> GetStocksFullHistory(string symbol){

            var History = await _stockService.GetFullStockHistory(symbol);

            return Ok(History);
        }
    }
}
