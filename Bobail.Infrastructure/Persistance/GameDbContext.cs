using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public DbSet<GameEntity> Games => Set<GameEntity>();

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
        });
    }
}