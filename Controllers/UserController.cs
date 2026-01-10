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
    











}
}

