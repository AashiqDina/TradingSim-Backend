using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Data;
using TradingSimulator_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using System.Security.Claims;
using TradingSimulator_Backend.Services;

namespace TradingSimulator_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public UserController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // simple ping to check if backend is currently running (since render spins down after periods of inactivity)
        [HttpGet("ping")]
        public ActionResult<bool> Ping()
        {
            return true;
        }

        // Register the user - ensures the password is hashed
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request){

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { success = false, message = "Username already taken." });

            var user = new User{
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Portfolio = new Portfolio{
                Stocks = new List<Stock>()
            };

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // logs the user in if the encoded password is equal to the user input's encoded password (using the stored encoded password's salt)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model){

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password)){
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new {
                success = true,
                token,
                user = new {
                    user.Id,
                    user.Username,
                    user.InvestedAmount,
                    user.CurrentValue,
                    user.ProfitLoss
                }
            });
        }

        // gets the users friends
        [Authorize]
        [HttpGet("Get-Friends")]
        public async Task<IActionResult> GetFriends(){

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);

            var user = await LoadUserWithRelations(userId);
            if (user == null) return NotFound();

            return Ok(ApiResponse<List<UserFriend>>.Success(user.FriendsList.ToList()));
        }

        // get the user's sent friend requests
        [Authorize]  
        [HttpGet("Get-Sent-Request")]
        public async Task<IActionResult> GetSentRequests(){

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);

            var user = await LoadUserWithRelations(userId);
            if (user == null) return NotFound();
        
            return Ok(ApiResponse<List<UserSentRequest>>.Success(user.SentRequests.ToList()));                    
        }
        
        // get the user's received friend requests
        [Authorize]
        [HttpGet("Get-Received-Request")]
        public async Task<IActionResult> GetReceivedRequests(){

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);

            var user = await LoadUserWithRelations(userId);
            if (user == null) return NotFound();
        
            return Ok(ApiResponse<List<UserReceivedRequest>>.Success(user.ReceivedRequests.ToList()));
        }

        // deletes user if token is valid
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(){
            try{

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null){
                    return Unauthorized();
                }

                var userId = long.Parse(userIdClaim);
                var exists = await _context.Users.AnyAsync(u => u.Id == userId);

                if (!exists)
                    return NotFound(new { message = "User not found" });

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM users WHERE id = {userId}"
                );

                return Ok(new { message = "User deleted successfully" });
            }
            catch(Exception ex){
                return StatusCode(500, new {
                    message = "Failed to delete user",
                    error = ex.Message
                });
            }
        }

        [HttpPost("checkUsername")]
        public async Task<IActionResult> CheckUsername([FromBody] UsernameCheckRequest request){
            var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            return Ok(new { exists });
        }

        [Authorize]
        [HttpPost("Send-Friend-Request/{friendId}")]
        public async Task<IActionResult> SendFriendRequest(long friendId){

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);

            if (userId == friendId)
                return BadRequest(ApiResponse<string>.Failure(400));
        
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(ApiResponse<string>.Failure(404));
        
            if (user.FriendsList.Any(f => f.FriendsUserId == friendId))
                return BadRequest(ApiResponse<string>.Failure(400));
        
            if (user.SentRequests.Any(r => r.FriendsUserId == friendId))
                return BadRequest(ApiResponse<string>.Failure(400));
        
            if (user.ReceivedRequests.Any(r => r.FriendsUserId == friendId))
                return BadRequest(ApiResponse<string>.Failure(400));
        
            _context.UsersSentRequests.Add(new UserSentRequest {
                UserId = user.Id,
                FriendsUserId = friend.Id,
                Username = friend.Username,
                ProfitLoss = friend.ProfitLoss
            });
        
            _context.UsersReceivedRequests.Add(new UserReceivedRequest {
                UserId = friend.Id,
                FriendsUserId = user.Id,
                Username = user.Username,
                ProfitLoss = user.ProfitLoss
            });
        
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("Friend request sent successfully"));    
        }

        [Authorize]
        [HttpPost("Accept-Request/{friendId}")]
        public async Task<IActionResult> AcceptFriendRequest(long friendId){

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(ApiResponse<string>.Failure(404));
        
            var received = user.ReceivedRequests.FirstOrDefault(r => r.FriendsUserId == friendId);
            var sent = friend.SentRequests.FirstOrDefault(r => r.FriendsUserId == userId);
        
            if (received == null || sent == null)
                return NotFound(ApiResponse<string>.Failure(400));

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
            return Ok(ApiResponse<string>.Success("Friend request accepted successfully"));    
        }

        [Authorize]
        [HttpPost("Decline-Request/{friendId}")]
        public async Task<IActionResult> DeclineFriendRequest(long friendId){
    
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(ApiResponse<string>.Failure(404));
        
            var received = user.ReceivedRequests.FirstOrDefault(r => r.FriendsUserId == friendId);
            var sent = friend.SentRequests.FirstOrDefault(r => r.FriendsUserId == userId);
        
            if (received != null) _context.UsersReceivedRequests.Remove(received);
            if (sent != null) _context.UsersSentRequests.Remove(sent);
        
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("Friend request declined successfully"));    
        }

        [Authorize]
        [HttpDelete("Delete-Friend/{friendId}")]
        public async Task<IActionResult> DeleteFriend(long friendId){

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null){
                return Unauthorized();
            }

            var userId = long.Parse(userIdClaim);
            var user = await LoadUserWithRelations(userId);
            var friend = await LoadUserWithRelations(friendId);
        
            if (user == null || friend == null)
                return NotFound(ApiResponse<string>.Failure(404));
        
            var f1 = user.FriendsList.FirstOrDefault(f => f.FriendsUserId == friendId);
            var f2 = friend.FriendsList.FirstOrDefault(f => f.FriendsUserId == userId);
        
            if (f1 != null) _context.UsersFriendsList.Remove(f1);
            if (f2 != null) _context.UsersFriendsList.Remove(f2);
        
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("Friend deleted successfully"));    
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












        // private functions ---------------------

        private async Task<User?> LoadUserWithRelations(long userId){
            
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








        // --------- old code  --------- 



        // [Authorize]
        // [HttpGet("{id:int}")]
        // public async Task<ActionResult<User>> GetUser(int id)
        // {
        //     var user = await _context.Users.FindAsync(id);
        //     if (user == null) return NotFound();
        //     return user;
        // }






        
    }
}











