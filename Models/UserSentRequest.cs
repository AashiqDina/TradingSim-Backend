using System.ComponentModel.DataAnnotations.Schema;

namespace TradingSimulator_Backend.Models
{
    public class UserSentRequest
    {
        [Column("UserId")]
        public long UserId { get; set; }

        [Column("FriendsUserId")]
        public long FriendsUserId { get; set; }

        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Column("ProfitLoss")]
        public float ProfitLoss { get; set; }
    }
}
