using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradingSimulator_Backend.Models
{
    public class User
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("username")]
        public string Username { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("investedamount")]
        public float InvestedAmount { get; set; }

        [Column("currentvalue")]
        public float CurrentValue { get; set; }

        [Column("profitloss")]
        public float ProfitLoss { get; set; }

        public Portfolio Portfolio { get; set; } = null!;

        [NotMapped]
        public ICollection<UserFriend> FriendsList { get; set; } = new List<UserFriend>();
        
        [NotMapped]
        public ICollection<UserSentRequest> SentRequests { get; set; } = new List<UserSentRequest>();
        
        [NotMapped]
        public ICollection<UserReceivedRequest> ReceivedRequests { get; set; } = new List<UserReceivedRequest>();
    }

    public class UserFriend
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("friendsuserid")]
        public long FriendsUserId { get; set; }

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("profitloss")]
        public float ProfitLoss { get; set; }
    }

    public class UserSentRequest
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("friendsuserid")]
        public long FriendsUserId { get; set; }

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("profitloss")]
        public float ProfitLoss { get; set; }
    }

    public class UserReceivedRequest
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("friendsuserid")]
        public long FriendsUserId { get; set; }

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("profitloss")]
        public float ProfitLoss { get; set; }
    }
}

