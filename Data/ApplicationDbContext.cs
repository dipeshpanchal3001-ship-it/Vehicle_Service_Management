using AutoCare.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleService> Services => Set<VehicleService>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Vehicle Configuration
        builder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(v => v.RegistrationNumber)
                  .IsUnique();

            entity.HasOne(v => v.User)
                  .WithMany(u => u.Vehicles)
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Service Configuration
        builder.Entity<VehicleService>(entity =>
        {
            entity.Property(s => s.Price)
                  .HasPrecision(18, 2);
        });

        // ServiceRequest Configuration
        builder.Entity<ServiceRequest>(entity =>
        {
            entity.HasIndex(sr => sr.RequestNumber)
                  .IsUnique();

            entity.Property(sr => sr.ServicePriceSnapshot)
                  .HasPrecision(18, 2);

            entity.HasOne(sr => sr.User)
                  .WithMany(u => u.ServiceRequests)
                  .HasForeignKey(sr => sr.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sr => sr.Vehicle)
                  .WithMany(v => v.ServiceRequests)
                  .HasForeignKey(sr => sr.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sr => sr.Service)
                  .WithMany(s => s.ServiceRequests)
                  .HasForeignKey(sr => sr.ServiceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Invoice Configuration
        builder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(i => i.InvoiceNumber)
                  .IsUnique();

            entity.HasIndex(i => i.RequestId)
                  .IsUnique();

            entity.Property(i => i.ServiceCharge)
                  .HasPrecision(18, 2);

            entity.Property(i => i.AdditionalCharges)
                  .HasPrecision(18, 2);

            entity.Property(i => i.TotalAmount)
                  .HasPrecision(18, 2);

            entity.HasOne(i => i.ServiceRequest)
                  .WithOne(sr => sr.Invoice)
                  .HasForeignKey<Invoice>(i => i.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
