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
        public decimal InvestedAmount { get; set; }

        [Column("currentvalue")]
        public decimal CurrentValue { get; set; }

        [Column("profitloss")]
        public decimal ProfitLoss { get; set; }

        public Portfolio Portfolio { get; set; } = null!;

        public ICollection<UserFriend> FriendsList { get; set; } = new List<UserFriend>();
        public ICollection<UserFriend> ReceivedRequests { get; set; } = new List<UserFriend>();
        public ICollection<UserFriend> SentRequests { get; set; } = new List<UserFriend>();
    }

   public class UserFriend
     {
        public long Id { get; set; }
        public long FriendsUserId { get; set; }
        public User FriendsUser { get; set; } = null!;

        public string Username { get; set; } = string.Empty;
        public decimal ProfitLoss { get; set; }
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
        public decimal ProfitLoss { get; set; }
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
        public decimal ProfitLoss { get; set; }
    }
}





