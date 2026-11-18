using CareFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Data;

public class CareFlowDbContext : DbContext
{
    public CareFlowDbContext(
        DbContextOptions<CareFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<DiagnosticService> DiagnosticServices
        => Set<DiagnosticService>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingService> BookingServices
        => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fictional patients for development and demonstration.
        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                PatientId = 1,
                FullName = "Alex Demo"
            },
            new Patient
            {
                PatientId = 2,
                FullName = "Jamie Sample"
            },
            new Patient
            {
                PatientId = 3,
                FullName = "Taylor Test"
            }
        );

        // Diagnostic services with sample daily capacities.
        modelBuilder.Entity<DiagnosticService>().HasData(
            new DiagnosticService
            {
                ServiceId = 1,
                Name = "Complete Blood Count",
                DailyCapacity = 5
            },
            new DiagnosticService
            {
                ServiceId = 2,
                Name = "Urinalysis",
                DailyCapacity = 5
            },
            new DiagnosticService
            {
                ServiceId = 3,
                Name = "Chest X-Ray",
                DailyCapacity = 3
            },
            new DiagnosticService
            {
                ServiceId = 4,
                Name = "ECG",
                DailyCapacity = 1
            }
        );

        // Patients
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.PatientId);

            entity.Property(p => p.FullName)
                .IsRequired()
                .HasMaxLength(150);
        });

        // Diagnostic services
        modelBuilder.Entity<DiagnosticService>(entity =>
        {
            entity.HasKey(s => s.ServiceId);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.ToTable("DiagnosticServices", table =>
            {
                table.HasCheckConstraint(
                    "CK_DiagnosticServices_DailyCapacity",
                    "\"DailyCapacity\" >= 0");
            });
        });

        // Bookings
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.BookingId);

            entity.Property(b => b.BookingDate)
                .IsRequired();

            entity.HasIndex(b => b.BookingDate);

            entity.HasOne(b => b.Patient)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Services included in each booking
        modelBuilder.Entity<BookingService>(entity =>
        {
            entity.HasKey(bs => new
            {
                bs.BookingId,
                bs.ServiceId
            });

            entity.HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(bs => bs.DiagnosticService)
                .WithMany(s => s.BookingServices)
                .HasForeignKey(bs => bs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}