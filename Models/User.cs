using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradingSimulator_Backend.Models
{
     public class User
     {
         public long Id { get; set; }
     
         public string Username { get; set; } = null!;
         public string Password { get; set; } = null!;
     
         public float InvestedAmount { get; set; }
         public float CurrentValue { get; set; }
         public float ProfitLoss { get; set; }
     
         public Portfolio Portfolio { get; set; } = null!;
     
         public ICollection<UserFriend> FriendsList { get; set; } = new List<UserFriend>();
         public ICollection<UserSentRequest> SentRequests { get; set; } = new List<UserSentRequest>();
         public ICollection<UserReceivedRequest> ReceivedRequests { get; set; } = new List<UserReceivedRequest>();
     }

     public class UserFriend
     {
         public long Id { get; set; }
         public long FriendsUserId { get; set; }
     
         public string Username { get; set; } = string.Empty;
         public float ProfitLoss { get; set; }
     }

     public class UserSentRequest
     {
         public long Id { get; set; }
         public long FriendsUserId { get; set; }
     
         public string Username { get; set; } = string.Empty;
         public float ProfitLoss { get; set; }
     }

     public class UserReceivedRequest
     {
         public long Id { get; set; }
         public long FriendsUserId { get; set; }
     
         public string Username { get; set; } = string.Empty;
         public float ProfitLoss { get; set; }
     }
}







