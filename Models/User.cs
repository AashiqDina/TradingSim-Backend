using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradingSimulator_Backend.Models
{
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

        // --- Navigation properties ---

        // Portfolio (1:1)
        public Portfolio? Portfolio { get; set; }

        // Friends (1:M)
        public ICollection<UserFriend> FriendsList { get; set; } = new List<UserFriend>();

        // Sent requests
        public ICollection<UserSentRequest> SentRequests { get; set; } = new List<UserSentRequest>();

        // Received requests
        public ICollection<UserReceivedRequest> ReceivedRequests { get; set; } = new List<UserReceivedRequest>();
    }

    public class UserFriend
    {
        public long Id { get; set; }

        // Foreign key to parent User
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // Friend info
        public long FriendId { get; set; }
        public string Username { get; set; } = string.Empty;
        public float ProfitLoss { get; set; }
    }

    public class UserSentRequest
    {
        public long Id { get; set; }

        // Foreign key to parent User
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // Target user info
        public long TargetUserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public float ProfitLoss { get; set; }
    }

    public class UserReceivedRequest
    {
        public long Id { get; set; }

        // Foreign key to parent User
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // Request sender info
        public long FromUserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public float ProfitLoss { get; set; }
    }
}
