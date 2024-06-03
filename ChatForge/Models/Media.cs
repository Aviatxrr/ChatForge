namespace ChatForge.Models;

public class Media : IModel
{
    public int Id { get; set; }
    
    // Media's Path, Type, and Name
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public string FileName { get; set; }
    
}