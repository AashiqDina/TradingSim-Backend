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

        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        // {
        //     return await _context.Users.ToListAsync();
        // }

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
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == model.Username &&
                    u.Password == model.Password);
        
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

        private async Task<User?> LoadUserWithRelations(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;
        
            user.FriendsList = await _context.UsersFriendsList
                .Where(f => f.UserId == userId)
                .ToListAsync();
        
            user.SentRequests = await _context.UsersSentRequests
                .Where(r => r.UserId == userId)
                .ToListAsync();
        
            user.ReceivedRequests = await _context.UsersReceivedRequests
                .Where(r => r.UserId == userId)
                .ToListAsync();
        
            return user;
        }

        [HttpPost("Send-Friend-Request/{userId}/{friendId}")]
        public async Task<IActionResult> SendFriendRequest(long userId, long friendId)
        {
            if (userId == friendId)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Cannot send request to yourself." });
        
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });
        
            if (user.FriendsList.Any(f => f.FriendsUserId == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Already friends." });
        
            if (user.SentRequests.Any(r => r.FriendsUserId == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Request already sent." });
        
            if (user.ReceivedRequests.Any(r => r.FriendsUserId == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "You already have a pending request from this user." });
        
            _context.UsersSentRequests.Add(new UserSentRequest
            {
                UserId = user.Id,
                FriendsUserId = friend.Id,
                Username = friend.Username,
                ProfitLoss = friend.ProfitLoss
            });
        
            _context.UsersReceivedRequests.Add(new UserReceivedRequest
            {
                UserId = friend.Id,
                FriendsUserId = user.Id,
                Username = user.Username,
                ProfitLoss = user.ProfitLoss
            });
        
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request sent successfully." });
        }

        [HttpPost("Accept-Request/{userId}/{friendId}")]
        public async Task<IActionResult> AcceptFriendRequest(long userId, long friendId)
        {
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });
        
            var received = user.ReceivedRequests.FirstOrDefault(r => r.FriendsUserId == friendId);
            var sent = friend.SentRequests.FirstOrDefault(r => r.FriendsUserId == userId);
        
            if (received == null || sent == null)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "No pending request found." });
        
            _context.UsersReceivedRequests.Remove(received);
            _context.UsersSentRequests.Remove(sent);
        
            _context.UsersFriendsList.Add(new UserFriend
            {
                UserId = user.Id,
                FriendsUserId = friend.Id,
                Username = friend.Username,
                ProfitLoss = friend.ProfitLoss
            });
        
            _context.UsersFriendsList.Add(new UserFriend
            {
                UserId = friend.Id,
                FriendsUserId = user.Id,
                Username = user.Username,
                ProfitLoss = user.ProfitLoss
            });
        
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request accepted successfully." });
        }

        [HttpPost("Decline-Request/{userId}/{friendId}")]
        public async Task<IActionResult> DeclineFriendRequest(long userId, long friendId)
        {
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });
        
            var received = user.ReceivedRequests.FirstOrDefault(r => r.FriendsUserId == friendId);
            var sent = friend.SentRequests.FirstOrDefault(r => r.FriendsUserId == userId);
        
            if (received != null) _context.UsersReceivedRequests.Remove(received);
            if (sent != null) _context.UsersSentRequests.Remove(sent);
        
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request declined successfully." });
        }

        [HttpDelete("Delete-Friend/{userId}/{friendId}")]
        public async Task<IActionResult> DeleteFriend(long userId, long friendId)
        {
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });
        
            var f1 = user.FriendsList.FirstOrDefault(f => f.FriendsUserId == friendId);
            var f2 = friend.FriendsList.FirstOrDefault(f => f.FriendsUserId == userId);
        
            if (f1 != null) _context.UsersFriendsList.Remove(f1);
            if (f2 != null) _context.UsersFriendsList.Remove(f2);
        
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend deleted successfully." });
        }

        // [HttpGet("Get-Friends/{userId}")]
        // public async Task<IActionResult> GetFriends(long userId)
        // {
        //     var user = await LoadUserWithRelations(userId);
        //     if (user == null) return NotFound();

            
        
        //     return Ok(new ApiResponse<List<UserFriend>> { HasError = false, Data = user.FriendsList.ToList() });
        // }

        [HttpGet("Get-Friends/{userId}")]
        public async Task<IActionResult> GetFriends(long userId)
        {
            var user = await LoadUserWithRelations(userId);
            if (user == null)
                return NotFound();
        
            var friendIds = user.FriendsList.Select(f => f.FriendsUserId).ToList();
        
            // Load each friend + their single portfolio + stocks
            var friends = await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .Include(u => u.Portfolio)
                    .ThenInclude(p => p.Stocks)
                .ToListAsync();
        
            foreach (var friendEntry in user.FriendsList)
            {
                var friendUser = friends.FirstOrDefault(f => f.Id == friendEntry.FriendsUserId);
        
                if (friendUser?.Portfolio != null)
                {
                    friendEntry.ProfitLoss = (float)friendUser.Portfolio.ProfitLoss;
                }
            }
        
            await _context.SaveChangesAsync();
        
            return Ok(new ApiResponse<List<UserFriend>>
            {
                HasError = false,
                Data = user.FriendsList.ToList()
            });
        }
        
        [HttpGet("Get-Sent-Request/{userId}")]
        public async Task<IActionResult> GetSentRequests(long userId)
        {
            var user = await LoadUserWithRelations(userId);
            if (user == null) return NotFound();
        
            return Ok(new ApiResponse<List<UserSentRequest>> { HasError = false, Data = user.SentRequests.ToList() });
        }
        
        [HttpGet("Get-Received-Request/{userId}")]
        public async Task<IActionResult> GetReceivedRequests(long userId)
        {
            var user = await LoadUserWithRelations(userId);
            if (user == null) return NotFound();
        
            return Ok(new ApiResponse<List<UserReceivedRequest>> { HasError = false, Data = user.ReceivedRequests.ToList() });
        }

        
    }
}








