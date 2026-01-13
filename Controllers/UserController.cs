using Microsoft.AspNetCore.Mvc;
using TradingSimulator_Backend.Models;
using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Data;


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
    public async Task<ActionResult<IEnumerable<User>>> GetUsersList()
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

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent(); // This means the user was deleted successfully.
    }

[HttpPost]
public async Task<IActionResult> RegisterUser([FromBody] User user)
{
    // Check if the user already exists
    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
    if (existingUser != null)
    {
        return BadRequest(new { success = false, message = "Username already taken." });
    }


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
        var userExists = await _context.Users.AnyAsync(u => u.Username == request.Username);

        return Ok(new { exists = userExists });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User model)
    {
        var user = await _context.Users
                                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

        if (user == null)
        {
            return Unauthorized(new { success = false, message = "Invalid username or password" });
        }

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
        {
            return Unauthorized(new { message = "Not logged in" });
        }

        return Ok(new { username });
    } [HttpPost("Send-Friend-Request/{userId}/{friendId}")]
        public async Task<IActionResult> SendFriendRequest(int userId, int friendId)
        {
            if (userId == friendId)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Cannot send request to yourself." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);
            var friendFriends = EnsureFriendsExists(friend);

            if (userFriends.FriendsList.Any(f => f.Id == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Already friends." });

            if (userFriends.SentRequests.Any(f => f.Id == friendId))
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "Request already sent." });

            userFriends.SentRequests.Add(new UserObj { Id = friend.Id, Username = friend.Username, ProfitLoss = friend.ProfitLoss });
            friendFriends.ReceivedRequests.Add(new UserObj { Id = user.Id, Username = user.Username, ProfitLoss = user.ProfitLoss });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request sent successfully." });
        }

        [HttpPost("Accept-Request/{userId}/{friendId}")]
        public async Task<IActionResult> AcceptFriendRequest(int userId, int friendId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);
            var friendFriends = EnsureFriendsExists(friend);

            var requestInReceived = userFriends.ReceivedRequests.FirstOrDefault(r => r.Id == friendId);
            var requestInSent = friendFriends.SentRequests.FirstOrDefault(r => r.Id == userId);

            if (requestInReceived == null || requestInSent == null)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "No pending request found." });

            userFriends.ReceivedRequests.Remove(requestInReceived);
            friendFriends.SentRequests.Remove(requestInSent);

            userFriends.FriendsList.Add(new UserObj { Id = friend.Id, Username = friend.Username, ProfitLoss = friend.ProfitLoss });
            friendFriends.FriendsList.Add(new UserObj { Id = user.Id, Username = user.Username, ProfitLoss = user.ProfitLoss });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request accepted successfully." });
        }

        [HttpPost("Decline-Request/{userId}/{friendId}")]
        public async Task<IActionResult> DeclineFriendRequest(int userId, int friendId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);
            var friendFriends = EnsureFriendsExists(friend);

            userFriends.ReceivedRequests.RemoveAll(r => r.Id == friendId);
            friendFriends.SentRequests.RemoveAll(r => r.Id == userId);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend request declined successfully." });
        }

        [HttpDelete("Delete-Friend/{userId}/{friendId}")]
        public async Task<IActionResult> DeleteFriend(int userId, int friendId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendId);

            if (user == null || friend == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);
            var friendFriends = EnsureFriendsExists(friend);

            var removedFromUser = userFriends.FriendsList.RemoveAll(f => f.Id == friendId);
            var removedFromFriend = friendFriends.FriendsList.RemoveAll(f => f.Id == userId);

            if (removedFromUser == 0 && removedFromFriend == 0)
                return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "They are not friends." });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { HasError = false, Data = "Friend deleted successfully." });
        }

        [HttpGet("Get-Friends/{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);

            // Update profit/loss
            var friendIds = userFriends.FriendsList.Select(f => f.Id).ToList();
            var dbFriends = await _context.Users.Where(u => friendIds.Contains(u.Id)).ToListAsync();

            foreach (var f in userFriends.FriendsList)
            {
                var dbFriend = dbFriends.FirstOrDefault(x => x.Id == f.Id);
                if (dbFriend != null)
                    f.ProfitLoss = dbFriend.ProfitLoss;
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<List<UserObj>> { HasError = false, Data = userFriends.FriendsList });
        }

        [HttpGet("Get-Sent-Request/{userId}")]
        public async Task<IActionResult> GetSentRequests(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);

            return Ok(new ApiResponse<List<UserObj>> { HasError = false, Data = userFriends.SentRequests });
        }

        [HttpGet("Get-Received-Request/{userId}")]
        public async Task<IActionResult> GetReceivedRequests(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new ApiResponse<string> { HasError = true, ErrorCode = 404, Data = "User not found." });

            var userFriends = EnsureFriendsExists(user);

            return Ok(new ApiResponse<List<UserObj>> { HasError = false, Data = userFriends.ReceivedRequests });
        }

        // ===================== HELPER ===================== //
        private Friends EnsureFriendsExists(User user)
        {
            if (user.Friends == null)
            {
                user.Friends = new Friends
                {
                    FriendsList = new List<UserObj>(),
                    SentRequests = new List<UserObj>(),
                    ReceivedRequests = new List<UserObj>()
                };
            }

            return user.Friends;
        }

    










}
}










