using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Stock> Stocks { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Portfolio> Portfolios { get; set; } = default!;
        
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;        
       protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Stock>()
            .HasIndex(s => s.Symbol)
            .IsUnique();

            builder.Entity<Portfolio>(x => x.HasKey(p => new { p.AppUserId, p.StockId }));

            builder.Entity<Portfolio>()
                .HasOne(u => u.AppUser)
                .WithMany(u => u.Portfolios)
                .HasForeignKey(u => u.AppUserId);

                builder.Entity<Portfolio>()
                .HasOne(u => u.Stock)
                .WithMany(u => u.Portfolios)
                .HasForeignKey(u => u.StockId);

                builder.Entity<RefreshToken>(e =>
                {
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Token).IsRequired();
                    e.HasIndex(x => x.Token).IsUnique();
                    e.HasOne(x => x.User)
                        .WithMany(u => u.RefreshTokens)
                        .HasForeignKey(x => x.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

                List<IdentityRole> roles = new List<IdentityRole>
                {
                    new IdentityRole
                    {
                        Id = "8D12FA6F-9B3A-4AFC-9E35-47DCEF1B1111",
                        Name = "Admin",
                        NormalizedName = "ADMIN"
                    },
                    new IdentityRole
                    {
                        Id = "B9B07E7A-4BB0-4BD9-AEB6-242D2D4B2222",
                        Name = "User",
                        NormalizedName = "USER"
                    },
                };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}


    
