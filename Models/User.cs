using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Column("id")]
    public long Id { get; set; } 

    [Column("username")]
    public required string Username { get; set; }

    [Column("password")]
    public required string Password { get; set; }

    [Column("investedamount")]
    public float InvestedAmount { get; set; } 

    [Column("currentvalue")]
    public float CurrentValue { get; set; }

    [Column("profitloss")]
    public float ProfitLoss { get; set; }

    // Navigation properties to relational tables
    public ICollection<UserFriend> FriendsList { get; set; } = new List<UserFriend>();
    public ICollection<UserSentRequest> SentRequests { get; set; } = new List<UserSentRequest>();
    public ICollection<UserReceivedRequest> ReceivedRequests { get; set; } = new List<UserReceivedRequest>();
}

// Friend entity
public class UserFriend
{
    public long Id { get; set; } // Identity column
    public long FriendsUserId { get; set; } // FK to User
    public User FriendsUser { get; set; }   // Navigation back to User

    public string Username { get; set; }
    public float ProfitLoss { get; set; }
}

// Sent request entity
public class UserSentRequest
{
    public long Id { get; set; } // Identity column
    public long FriendsUserId { get; set; } // FK to User
    public User FriendsUser { get; set; }

    public string Username { get; set; }
    public float ProfitLoss { get; set; }
}

// Received request entity
public class UserReceivedRequest
{
    public long Id { get; set; } // Identity column
    public long FriendsUserId { get; set; } // FK to User
    public User FriendsUser { get; set; }

    public string Username { get; set; }
    public float ProfitLoss { get; set; }
}
