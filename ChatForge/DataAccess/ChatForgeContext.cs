using ChatForge.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatForge.DataAccess;

public class ChatForgeContext : DbContext
{
    private string connectionString;
    public DbSet<User> User { get; set; }
    public DbSet<Chatroom> Chatroom { get; set; }
    public DbSet<UserChatroom> UserChatroom { get; set; }
    public DbSet<Message> Message { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<ForbiddenToken> ForbiddenToken { get; set; }
    public DbSet<JoinRequest> JoinRequest { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Chatroom>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chatrooms)
            .UsingEntity<UserChatroom>();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Chatrooms)
            .WithMany(c => c.Users);

    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        connectionString = Environment.GetEnvironmentVariable("CHATFORGEAUTH");

        optionsBuilder.UseMySql(connectionString,
            ServerVersion.AutoDetect(connectionString));
        optionsBuilder.UseLazyLoadingProxies();
    }
}