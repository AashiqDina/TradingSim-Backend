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

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/user/List
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

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/user
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

        // ---------------- FRIENDS ---------------- //

        [HttpPost("Send-Friend-Request/{userId}/{friendId}")]
        public async Task<IActionResult> SendFriendRequest(long userId, long friendId)
        {
            if (userId == friendId)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Cannot send request to yourself." });

            var user = await _context.Users.FindAsync(userId);
            var friend = await _context.Users.FindAsync(friendId);
            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var alreadyFriends = await _context.UsersFriendsList
                .AnyAsync(f => f.FriendsUserId == userId && f.Id == friendId);
            if (alreadyFriends)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Already friends." });

            var alreadySent = await _context.UsersSentRequests
                .AnyAsync(f => f.FriendsUserId == userId && f.Id == friendId);
            if (alreadySent)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Request already sent." });

            _context.UsersSentRequests.Add(new UserSentRequest
            {
                FriendsUserId = userId,
                Id = friend.Id,
                Username = friend.Username,
                ProfitLoss = friend.ProfitLoss
            });

            _context.UsersReceivedRequests.Add(new UserReceivedRequest
            {
                FriendsUserId = friendId,
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
            var sent = await _context.UsersSentRequests
                .FirstOrDefaultAsync(f => f.FriendsUserId == friendId && f.Id == userId);

            var received = await _context.UsersReceivedRequests
                .FirstOrDefaultAsync(f => f.FriendsUserId == userId && f.Id == friendId);

            if (sent == null || received == null)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "No pending request found." });

            _context.UsersSentRequests.Remove(sent);
            _context.UsersReceivedRequests.Remove(received);

            _context.UsersFriendsList.Add(new UserFriend
            {
                FriendsUserId = userId,
                Id = friendId,
                Username = (await _context.Users.FindAsync(friendId))!.Username,
                ProfitLoss = (await _context.Users.FindAsync(friendId))!.ProfitLoss
            });

            _context.UsersFriendsList.Add(new UserFriend
            {
                FriendsUserId = friendId,
                Id = userId,
                Username = (await _context.Users.FindAsync(userId))!.Username,
                ProfitLoss = (await _context.Users.FindAsync(userId))!.ProfitLoss
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request accepted successfully." });
        }

        [HttpPost("Decline-Request/{userId}/{friendId}")]
        public async Task<IActionResult> DeclineFriendRequest(long userId, long friendId)
        {
            var sent = await _context.UsersSentRequests
                .FirstOrDefaultAsync(f => f.FriendsUserId == userId && f.Id == friendId);
            var received = await _context.UsersReceivedRequests
                .FirstOrDefaultAsync(f => f.FriendsUserId == friendId && f.Id == userId);

            if (sent != null) _context.UsersSentRequests.Remove(sent);
            if (received != null) _context.UsersReceivedRequests.Remove(received);

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request declined successfully." });
        }

        [HttpDelete("Delete-Friend/{userId}/{friendId}")]
        public async Task<IActionResult> DeleteFriend(long userId, long friendId)
        {
            var friend1 = await _context.UsersFriendsList
                .FirstOrDefaultAsync(f => f.FriendsUserId == userId && f.Id == friendId);
            var friend2 = await _context.UsersFriendsList
                .FirstOrDefaultAsync(f => f.FriendsUserId == friendId && f.Id == userId);

            if (friend1 != null) _context.UsersFriendsList.Remove(friend1);
            if (friend2 != null) _context.UsersFriendsList.Remove(friend2);

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend deleted successfully." });
        }

        [HttpGet("Get-Friends/{userId}")]
        public async Task<IActionResult> GetFriends(long userId)
        {
            var friends = await _context.UsersFriendsList
                .Where(f => f.FriendsUserId == userId)
                .ToListAsync();

            return Ok(new ApiResponse<List<UserObj>> { HasError = false, Data = friends });
        }

        [HttpGet("Get-Sent-Request/{userId}")]
        public async Task<IActionResult> GetSentRequests(long userId)
        {
            var sent = await _context.UsersSentRequests
                .Where(f => f.FriendsUserId == userId)
                .ToListAsync();

            return Ok(new ApiResponse<List<UserObj>> { HasError = false, Data = sent });
        }

        [HttpGet("Get-Received-Request/{userId}")]
        public async Task<IActionResult> GetReceivedRequests(long userId)
        {
            var received = await _context.UsersReceivedRequests
                .Where(f => f.FriendsUserId == userId)
                .ToListAsync();

            return Ok(new ApiResponse<List<UserObj>> { HasError = false, Data = received });
        }
    }
}
