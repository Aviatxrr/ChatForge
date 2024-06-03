namespace ChatForge.Models;

public class Chatroom : IModel
{
   public int Id { get; set; } 
   public string Name { get; set; }
   public bool Private { get; set; }
   
   // List of users in chatroom
   public virtual ICollection<User> Users { get; set; }
   
   // List of messages in chatroom
   public virtual ICollection<Message> Messages { get; set; }
   
}