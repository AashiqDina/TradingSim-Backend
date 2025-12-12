namespace TradingSimulator_Backend.Services
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using System;
    using TradingSimulator_Backend.Models;
    using TradingSimulator_Backend.Data;
    using System.Collections.Concurrent;

    public class NewsService : INewsService{
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly string _apiKey = Environment.GetEnvironmentVariable("NEWS_API_KEY");

        private static ConcurrentDictionary<string, (List<CompanyNews?> CompNews, DateTime Timestamp)> _CompanyNewsCache = new ConcurrentDictionary<string, (List<CompanyNews?> CompNews, DateTime Timestamp)>();
        private static (List<CompanyNews?> MarketNews, DateTime Timestamp)? _MarketNewsCache = null;

        public NewsService(HttpClient httpClient, AppDbContext appDbContext)
        {
            _httpClient = httpClient;
            _context = appDbContext;
        }

        public DateTime getCompanyNewsLastUpdated(string symbol){
            return _CompanyNewsCache[symbol].Timestamp;
        }

        public async Task<ApiResponse<List<CompanyNews?>?>> getCompanyNews(string symbol)
        {
            if (_CompanyNewsCache.ContainsKey(symbol) && (DateTime.Now - _CompanyNewsCache[symbol].Timestamp < TimeSpan.FromMinutes(720)))
            {
                var cachedData = _CompanyNewsCache[symbol].CompNews;
                Console.WriteLine("Company Cache", cachedData);
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = cachedData,
                    HasError = false,
                    ErrorCode = null
                };
            }

            var result = await FetchCompanyNews(symbol);

            if (result != null && result.Data != null && !result.HasError)
            {
                _CompanyNewsCache[symbol] = (result.Data, DateTime.Now);
            }

            return result;
        }
        
        public async Task<ApiResponse<List<CompanyNews?>?>> getMarketNews()
        {
            if (_MarketNewsCache != null && (DateTime.Now - _MarketNewsCache.Value.Timestamp < TimeSpan.FromHours(2)))
            {
                var cachedData = _MarketNewsCache.Value.MarketNews;
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = cachedData,
                    HasError = false,
                    ErrorCode = null
                };
            }

            var result = await FetchMarketNews();

            if (result != null && result.Data != null && !result.HasError)
            {
                _MarketNewsCache = (result.Data, DateTime.Now);
            }

            return result;
            
        }

        private async Task<ApiResponse<List<CompanyNews?>?>> FetchCompanyNews(string symbol)
        {
            var from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var to = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var url = $"https://finnhub.io/api/v1/company-news?symbol={symbol}&from={from}&to={to}&token={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Raw Company Profile API Response: {json}");

            try
            {
                var companyNews = JsonConvert.DeserializeObject<List<CompanyNews>>(json);
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = companyNews,
                    HasError = false,
                    ErrorCode = null
                };

            }
            catch
            {
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = -1
                };
            }

        }
        private async Task<ApiResponse<List<CompanyNews?>?>> FetchMarketNews()
        {

            var url = $"https://finnhub.io/api/v1/news?category=general&token={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Raw Company Profile API Response: {json}");

            try
            {
                var MarketNews = JsonConvert.DeserializeObject<List<CompanyNews>>(json);
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = MarketNews,
                    HasError = false,
                    ErrorCode = null
                };

            }
            catch
            {
                return new ApiResponse<List<CompanyNews?>?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = -1
                };
            }
        }

    }
}


