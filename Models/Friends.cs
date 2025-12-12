namespace TradingSimulator_Backend.Models
{
    public class Friends
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<UserObj> FriendsList { get; set; } = new List<UserObj>();
        public List<UserObj> SentRequests { get; set; } = new List<UserObj>();
        public List<UserObj> ReceivedRequests { get; set; } = new List<UserObj>();
    }
}
