namespace ChatForge.Models;

public class ForbiddenToken : IModel
{
    public int Id { get; set; }
    public string TokenString { get; set; }
}