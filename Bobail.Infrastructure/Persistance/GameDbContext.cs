using Bobail.Infrastructure.Persistance.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public DbSet<GameEntity> Games => Set<GameEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<GamePlayerEntity> GamePlayers => Set<GamePlayerEntity>();

    public DbSet<GameStateEntity> GameStates => Set<GameStateEntity>();

    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens => Set<EmailVerificationTokenEntity>();

    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

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
            entity.Property(x => x.Nickname).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.IsEmailVerified).HasDefaultValue(false);
            entity.Property(x => x.EmailVerifiedAtUtc);
        });

        modelBuilder.Entity<EmailVerificationTokenEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetTokenEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.Used).HasDefaultValue(false);
            entity.Property(x => x.UsedAtUtc);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GamePlayerEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId);

            
            entity.HasIndex(x => new { x.GameId, x.Color })
                .IsUnique()
                .HasDatabaseName("IX_GamePlayers_GameId_Color");

            entity.HasIndex(x => new { x.GameId, x.UserId })
                .IsUnique()
                .HasFilter("[UserId] IS NOT NULL")
                .HasDatabaseName("IX_GamePlayers_GameId_UserId_Unique");

            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(x => x.Color);
            entity.Property(x => x.IsBot);
        });

        modelBuilder.Entity<GameStateEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.MoveNumber);
            entity.Property(x => x.StateJson).IsRequired();
            entity.Property(x => x.CreatedAt);

            entity.HasIndex(x => new { x.GameId, x.MoveNumber })
                .IsUnique();
        });

        modelBuilder.Entity<GameEntity>(entity =>
        {
            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(x => x.WinnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
