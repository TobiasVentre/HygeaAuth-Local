using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserId)
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.Role)
                    .HasMaxLength(25)
                    .HasColumnType("varchar")
                    .IsRequired();

                entity.Property(u => u.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                entity.Property(u => u.IsEmailVerified)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(u => u.FirstName)
                    .HasMaxLength(255)
                    .HasColumnType("varchar")
                    .IsRequired();

                entity.Property(u => u.LastName)
                    .HasMaxLength(255)
                    .HasColumnType("varchar")
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(255)
                    .HasColumnType("varchar")
                    .IsRequired();

                entity.Property(u => u.Dni)
                    .HasMaxLength(20) // Aumentado para permitir DNI más largos
                    .HasColumnType("varchar")
                    .IsRequired();

                entity.Property(u => u.Password)
                    .HasMaxLength(255)
                    .HasColumnType("varchar")
                    .IsRequired();

                entity.Property(u => u.AccessFailedCount)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(u => u.LockoutEndDate)
                    .IsRequired(false);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.RefreshTokenId);
                entity.Property(r => r.RefreshTokenId)
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.Token)
                    .HasColumnType("varchar(max)")
                    .IsRequired();

                entity.Property(r => r.CreateDate)
                    .IsRequired();

                entity.Property(r => r.ExpireDate)
                    .IsRequired();

                entity.Property(r => r.IsActive)
                    .IsRequired();

                entity.Property(r => r.LastUsed)
                    .HasColumnType("datetime2")
                    .IsRequired();

                entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId);
                
            });

            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Email)
                      .HasMaxLength(255)
                      .HasColumnType("varchar")
                      .IsRequired();

                entity.Property(e => e.Token)
                      .HasColumnType("varchar(max)")
                      .IsRequired();

                entity.Property(e => e.Expiration)
                      .IsRequired();
            });

            modelBuilder.Entity<Notification>(b =>
            {
                b.ToTable("Notifications");
                b.HasKey(x => x.NotificationId);
                b.HasOne(x => x.User)
                    .WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.Property(x => x.Type).HasConversion<string>();
                b.Property(x => x.Status).HasConversion<string>();
                b.Property(x => x.Payload).HasColumnType("NVARCHAR(MAX)");
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.SentAt).IsRequired(false);
            });
        }
    }
}
