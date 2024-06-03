namespace ChatForge.Models;

public class JoinRequest : IModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ChatroomId { get; set; }
    public bool IsInvite { get; set; }
}