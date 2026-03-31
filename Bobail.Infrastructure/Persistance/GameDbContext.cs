using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public DbSet<GameEntity> Games => Set<GameEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<GamePlayerEntity> GamePlayers => Set<GamePlayerEntity>();

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StateJson).IsRequired();

            entity.Property(x => x.Status);
            entity.Property(x => x.CurrentTurn);
            entity.Property(x => x.Mode);
            entity.Property(x => x.BotDifficulty);

            entity.Property(x => x.CreatedAt);
            entity.Property(x => x.UpdatedAt);

        });

        modelBuilder.Entity<UserEntity>(entity =>
{
    entity.HasKey(x => x.Id);

    entity.Property(x => x.Email).IsRequired();
    entity.HasIndex(x => x.Email).IsUnique();

    entity.Property(x => x.PasswordHash).IsRequired();

    entity.Property(x => x.Role);

    entity.Property(x => x.CreatedAt);
});

        modelBuilder.Entity<GamePlayerEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId);

            entity.Property(x => x.Color);
            entity.Property(x => x.IsBot);
        });
    }
}