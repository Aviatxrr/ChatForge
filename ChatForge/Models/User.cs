namespace ChatForge.Models;

public class User : IModel
{
    public int Id { get; set; }
    
    // Authentication stuff
    public string Username { get; set; }
    public  byte[] PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    
    // Is the user a server admin/owner
    public bool ServerAdmin { get; set; }
    public bool ServerOwner { get; set; }
    
    // User's Chatrooms
    public virtual ICollection<Chatroom> Chatrooms { get; set; }
    
    public string? RefreshToken { get; set; }
    
}