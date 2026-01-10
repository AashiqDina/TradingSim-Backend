using System.ComponentModel.DataAnnotations.Schema;

public class Friends
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public List<User> FriendsList { get; set; } = new();
    public List<User> SentRequests { get; set; } = new();
    public List<User> ReceivedRequests { get; set; } = new();
}

