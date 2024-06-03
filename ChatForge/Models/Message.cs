namespace ChatForge.Models;

public class Message : IModel
{
    public int Id { get; set; }
    
    // Associated Chatroom
    public int ChatroomId { get; set; }
    public virtual Chatroom Chatroom { get; set; }
    
    // Asssociated User
    public int UserId { get; set; }
    public virtual User Sender { get; set; }
    
    // Associated Media, if applicable
    public int? MediaId { get; set; }
    public virtual Media? Media { get; set; }
    
    public string Contents { get; set; }
    public virtual DateTime SentAt { get; set; }
    
}