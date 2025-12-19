using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SignalR.SampleProject.Models.Entities;

namespace SignalR.SampleProject.Models.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .HasOne(p => p.IdentityUser)
            .WithMany()
            .HasForeignKey(p => p.UserId);
        base.OnModelCreating(builder);
    }
}
