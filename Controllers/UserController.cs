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
    }

    [HttpPost("Send-Friend-Request/{userId}/{friendId}")]
    public async Task<IActionResult> SendFriendRequest(int userId, int friendId)
    {
        var user = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var friend = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == friendId);

        if (user == null || friend == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Data = null,
                HasError = true,
                ErrorCode = 404
            });
        }

        if (user.Friends == null)
        {
            user.Friends = new Friends { UserId = user.Id };
            _context.Friends.Add(user.Friends);
        }

        if (friend.Friends == null)
        {
            friend.Friends = new Friends { UserId = friend.Id };
            _context.Friends.Add(friend.Friends);
        }

        if (user.Friends.SentRequests.Any(r => r.Id == friendId) ||
            friend.Friends.ReceivedRequests.Any(r => r.Id == userId))
        {
            return BadRequest(new ApiResponse<string>
            {
                Data = "Friend request already sent.",
                HasError = true,
                ErrorCode = 400
            });
        }

        user.Friends.SentRequests.Add(new UserObj
        {
            Id = friend.Id,
            Username = friend.Username,
            ProfitLoss = friend.ProfitLoss
        });

        friend.Friends.ReceivedRequests.Add(new UserObj
        {
            Id = user.Id,
            Username = user.Username,
            ProfitLoss = user.ProfitLoss
        });

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Data = "Friend request sent successfully.",
            HasError = false,
            ErrorCode = null
        });
    }

    private async Task<Friends> EnsureFriendsExists(User user)
    {
        if (user.Friends == null)
        {
            var friends = new Friends
            {
                UserId = user.Id,
                FriendsList = new List<UserObj>(),
                SentRequests = new List<UserObj>(),
                ReceivedRequests = new List<UserObj>()
            };

            _context.Friends.Add(friends);
            user.Friends = friends;
            await _context.SaveChangesAsync();
        }

        return user.Friends;
    }

    [HttpGet("Get-Sent-Request/{userId}")]
    public async Task<IActionResult> GetSentRequests(int userId){
        var user = await _context.Users
        .Include(u => u.Friends)
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new ApiResponse<string>
            {
                HasError = true,
                ErrorCode = 404,
                Data = "User not found."
            });
        }

        var friends = await EnsureFriendsExists(user);

        return Ok(new ApiResponse<List<UserObj>>
        {
            Data = friends.SentRequests,
            HasError = false
        });
    }

    [HttpGet("Get-Received-Request/{userId}")]
    public async Task<IActionResult> GetReceivedRequests(int userId){
        var user = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new ApiResponse<string>
            {
                HasError = true,
                ErrorCode = 404,
                Data = "User not found."
            });
        }

        var friends = await EnsureFriendsExists(user);

        return Ok(new ApiResponse<List<UserObj>>
        {
            Data = friends.ReceivedRequests,
            HasError = false
        });
    }

    [HttpGet("Get-Friends/{userId}")]
    public async Task<IActionResult> GetFriends(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new ApiResponse<string>
            {
                HasError = true,
                ErrorCode = 404,
                Data = "User not found."
            });
        }

        var friends = await EnsureFriendsExists(user);

        foreach (var friendObj in friends.FriendsList)
        {
            var dbFriend = await _context.Users.FirstOrDefaultAsync(u => u.Id == friendObj.Id);
            if (dbFriend != null)
            {
                friendObj.ProfitLoss = dbFriend.ProfitLoss;
            }
        }

        await _context.SaveChangesAsync();


        return Ok(new ApiResponse<List<UserObj>>
        {
            Data = friends.FriendsList,
            HasError = false
        });
    }

    [HttpPost("Accept-Request/{userId}/{friendId}")]
    public async Task<IActionResult> AcceptFriendRequest(int userId, int friendId){
        var user = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var friend = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == friendId);

        if (user == null || friend == null)
            return NotFound(new ApiResponse<string> { Data = "User not found", HasError = true, ErrorCode = 404});

        if(user.Friends == null){
            user.Friends = new Friends { UserId = user.Id };
        }
        if(user.Friends == null){
            friend.Friends = new Friends { UserId = friend.Id };
        }

        if (!_context.Friends.Local.Contains(user.Friends)){
            _context.Friends.Add(user.Friends);
        }
        if (!_context.Friends.Local.Contains(friend.Friends)){
            _context.Friends.Add(friend.Friends);
        }

        var requestInReceived = user.Friends.ReceivedRequests.FirstOrDefault(r => r.Id == friendId);
        var requestInSent = friend.Friends.SentRequests.FirstOrDefault(r => r.Id == userId);

        if (requestInReceived == null || requestInSent == null)
            return BadRequest(new ApiResponse<string> { HasError = true, ErrorCode = 400, Data = "No pending request found" });

        user.Friends.ReceivedRequests.Remove(requestInReceived);
        friend.Friends.SentRequests.Remove(requestInSent);

        user.Friends.FriendsList.Add(new UserObj { Id = friend.Id, Username = friend.Username, ProfitLoss = friend.ProfitLoss });
        friend.Friends.FriendsList.Add(new UserObj { Id = user.Id, Username = user.Username, ProfitLoss = user.ProfitLoss });

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Data = "Friend request accepted successfully",
            HasError = false,
            ErrorCode = null
        });
    }

    [HttpPost("Decline-Request/{userId}/{friendId}")]
    public async Task<IActionResult> DeclineFriendRequest(int userId, int friendId){
        var user = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var friend = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == friendId);

        if (user == null || friend == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Data = null,
                HasError = true,
                ErrorCode = 404
            });
        }

        if (user.Friends == null)
        {
            user.Friends = new Friends { UserId = user.Id };
            _context.Friends.Add(user.Friends);
        }
        if (friend.Friends == null)
        {
            friend.Friends = new Friends { UserId = friend.Id };
            _context.Friends.Add(friend.Friends);
        }

        user.Friends.ReceivedRequests.RemoveAll(r => r.Id == friendId);
        friend.Friends.SentRequests.RemoveAll(r => r.Id == userId);

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Data = "Friend request declined successfully.",
            HasError = false,
            ErrorCode = null
        });
    }

    [HttpDelete("Delete-Friend/{userId}/{friendId}")]
    public async Task<IActionResult> DeleteFriend(int userId, int friendId){
        var user = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var friend = await _context.Users
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == friendId);

        if (user == null || friend == null){
            return NotFound(new ApiResponse<string>
            {
                HasError = true,
                ErrorCode = 404,
                Data = "User not found."
            });
        }

        if (user.Friends == null || friend.Friends == null){
            return BadRequest(new ApiResponse<string>
            {
                HasError = true,
                ErrorCode = 400,
                Data = "No friendship exists."
            });
        }

        var removedFromUser = user.Friends.FriendsList.RemoveAll(u => u.Id == friendId);
        var removedFromFriend = friend.Friends.FriendsList.RemoveAll(u => u.Id == userId);
   
        if (removedFromUser == 0 && removedFromFriend == 0)
        {
            return BadRequest(new ApiResponse<string>
            {
                HasError = true,
                ErrorCode = 400,
                Data = "They are not friends."
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            HasError = false,
            Data = "Friend deleted successfully."
        });
    }
    











}
}


