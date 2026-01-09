using System.ComponentModel.DataAnnotations.Schema;

    public class Friends{
        [Column("userid")]
        public long UserId { get; set; }

        public User User { get; set; } = null!;

        public List<UserObj> FriendsList { get; set; } = new();
        public List<UserObj> SentRequests { get; set; } = new();
        public List<UserObj> ReceivedRequests { get; set; } = new();
    }


