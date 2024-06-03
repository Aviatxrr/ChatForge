namespace ChatForge.Models;

public class UserChatroom : IModel
{
    
    public int Id { get; set; }
    
    // Is user a moderator/owner in the associated chatroom
    public bool ChatroomModerator { get; set; }
    public bool ChatroomOwner { get; set; }
    
    // Associated user
    public int UserId { get; set; }
    public virtual User User { get; set; }
    
    // Associated Chatroom
    public int ChatroomId { get; set; }
    public virtual Chatroom Chatroom { get; set; }
    
    
}