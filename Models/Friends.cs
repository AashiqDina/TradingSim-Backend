public class Friends
{
    public long UserId { get; set; }
    public User User { get; set; }

    public List<UserObj> FriendsList { get; set; }
    public List<UserObj> SentRequests { get; set; }
    public List<UserObj> ReceivedRequests { get; set; }
}
