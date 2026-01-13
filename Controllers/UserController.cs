using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Data;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------ BASIC USER OPERATIONS ------------------ //

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<UserObj>>> GetUsersList()
        {
            var users = await _context.Users
                .Select(u => new UserObj
                {
                    Id = u.Id,
                    Username = u.Username,
                    ProfitLoss = u.ProfitLoss
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest(new { success = false, message = "Username already taken." });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var portfolio = new Portfolio
            {
                UserId = user.Id,
                Stocks = new List<Stock>()
            };
            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User registered successfully and portfolio created." });
        }

        [HttpPost("checkUsername")]
        public async Task<IActionResult> CheckUsername([FromBody] UsernameCheckRequest request)
        {
            var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            return Ok(new { exists });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
                return Unauthorized(new { success = false, message = "Invalid username or password" });

            return Ok(new
            {
                success = true,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.InvestedAmount,
                    user.CurrentValue,
                    user.ProfitLoss
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Not logged in" });

            return Ok(new { username });
        }

        // ------------------ FRIENDS & REQUESTS ------------------ //

        // Helper method to load a user with friends/requests
        private async Task<User?> LoadUserWithFriends(long userId)
        {
            return await _context.Users
                .Include(u => u.Friends)
                    .ThenInclude(f => f.FriendsList)
                .Include(u => u.Friends)
                    .ThenInclude(f => f.SentRequests)
                .Include(u => u.Friends)
                    .ThenInclude(f => f.ReceivedRequests)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        [HttpPost("Send-Friend-Request/{userId}/{friendId}")]
        public async Task<IActionResult> SendFriendRequest(long userId, long friendId)
        {
            if (userId == friendId)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Cannot send request to yourself." });

            var user = await LoadUserWithFriends(userId);
            var friend = await LoadUserWithFriends(friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            if (user.Friends.FriendsList.Any(f => f.Id == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Already friends." });

            if (user.Friends.SentRequests.Any(f => f.Id == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Request already sent." });

            // Add to sent and received
            user.Friends.SentRequests.Add(new UserSentRequest
            {
                Id = friend.Id,
                Username = friend.Username,
                ProfitLoss = friend.ProfitLoss
            });

            friend.Friends.ReceivedRequests.Add(new UserReceivedRequest
            {
                Id = user.Id,
                Username = user.Username,
                ProfitLoss = user.ProfitLoss
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request sent successfully." });
        }

        [HttpPost("Accept-Request/{userId}/{friendId}")]
        public async Task<IActionResult> AcceptFriendRequest(long userId, long friendId)
        {
            var user = await LoadUserWithFriends(userId);
            var friend = await LoadUserWithFriends(friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var sentRequest = friend.Friends.SentRequests.FirstOrDefault(f => f.Id == userId);
            var receivedRequest = user.Friends.ReceivedRequests.FirstOrDefault(f => f.Id == friendId);

            if (sentRequest == null || receivedRequest == null)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "No pending request found." });

            // Remove pending requests
            friend.Friends.SentRequests.Remove(sentRequest);
            user.Friends.ReceivedRequests.Remove(receivedRequest);

            // Add to friends list both sides
            user.Friends.FriendsList.Add(new UserFriend
            {
                Id = friend.Id,
                Username = friend.Username,
                ProfitLoss = friend.ProfitLoss
            });

            friend.Friends.FriendsList.Add(new UserFriend
            {
                Id = user.Id,
                Username = user.Username,
                ProfitLoss = user.ProfitLoss
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request accepted successfully." });
        }

        [HttpPost("Decline-Request/{userId}/{friendId}")]
        public async Task<IActionResult> DeclineFriendRequest(long userId, long friendId)
        {
            var user = await LoadUserWithFriends(userId);
            var friend = await LoadUserWithFriends(friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var sent = user.Friends.SentRequests.FirstOrDefault(f => f.Id == friendId);
            var received = user.Friends.ReceivedRequests.FirstOrDefault(f => f.Id == friendId);

            if (sent != null) user.Friends.SentRequests.Remove(sent);
            if (received != null) user.Friends.ReceivedRequests.Remove(received);

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request declined successfully." });
        }

        [HttpDelete("Delete-Friend/{userId}/{friendId}")]
        public async Task<IActionResult> DeleteFriend(long userId, long friendId)
        {
            var user = await LoadUserWithFriends(userId);
            var friend = await LoadUserWithFriends(friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var friend1 = user.Friends.FriendsList.FirstOrDefault(f => f.Id == friendId);
            var friend2 = friend.Friends.FriendsList.FirstOrDefault(f => f.Id == userId);

            if (friend1 != null) user.Friends.FriendsList.Remove(friend1);
            if (friend2 != null) friend.Friends.FriendsList.Remove(friend2);

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend deleted successfully." });
        }

        [HttpGet("Get-Friends/{userId}")]
        public async Task<IActionResult> GetFriends(long userId)
        {
            var user = await LoadUserWithFriends(userId);
            if (user == null) return NotFound();

            return Ok(new ApiResponse<List<UserFriend>> { HasError = false, Data = user.Friends.FriendsList });
        }

        [HttpGet("Get-Sent-Request/{userId}")]
        public async Task<IActionResult> GetSentRequests(long userId)
        {
            var user = await LoadUserWithFriends(userId);
            if (user == null) return NotFound();

            return Ok(new ApiResponse<List<UserSentRequest>> { HasError = false, Data = user.Friends.SentRequests });
        }

        [HttpGet("Get-Received-Request/{userId}")]
        public async Task<IActionResult> GetReceivedRequests(long userId)
        {
            var user = await LoadUserWithFriends(userId);
            if (user == null) return NotFound();

            return Ok(new ApiResponse<List<UserReceivedRequest>> { HasError = false, Data = user.Friends.ReceivedRequests });
        }
    }
}
