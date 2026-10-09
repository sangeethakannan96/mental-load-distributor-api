using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MentalLoadDistributor.Core.Domain.Models;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Family> Families { get; set; } = null!;
        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<FamilyProfile> FamilyProfiles { get; set; }

        public DbSet<HouseholdPlan> HouseholdPlans { get; set; }

        public DbSet<DailyReflection> DailyReflections { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<ApprovedPlan> ApprovedPlans { get; set; }

        public DbSet<ApprovedPlanItem> ApprovedPlanItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // User
            // -----------------------------
            modelBuilder.Entity<User>(eb =>
            {
                eb.HasKey(u => u.Id);

                eb.Property(u => u.Name).IsRequired();

                // ✅ JSON conversion (valid)
                eb.Property(u => u.Skills)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());

                // ✅ Relationship FIX
                eb.HasOne(u => u.Family)
                  .WithMany(f => f.Members)
                  .HasForeignKey(u => u.FamilyId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // -----------------------------
            // Family
            // -----------------------------
            modelBuilder.Entity<Family>(eb =>
            {
                eb.HasKey(f => f.Id);

                eb.Property(f => f.Name).IsRequired();

                eb.Property(f => f.Preferences)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new());
            });

            // -----------------------------
            // TaskItem
            // -----------------------------
            modelBuilder.Entity<TaskItem>(eb =>
            {
                eb.HasKey(t => t.Id);

                eb.Property(t => t.Title).IsRequired();

                // JSON conversions
                eb.Property(t => t.Tags)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());

                eb.Property(t => t.Metadata)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new());

                eb.HasOne(t => t.CreatedBy)
                  .WithMany()
                  .HasForeignKey(t => t.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

                eb.HasOne(t => t.AssignedTo)
                  .WithMany()
                  .HasForeignKey(t => t.AssignedToId)
                  .OnDelete(DeleteBehavior.SetNull);

                eb.HasOne(t => t.Family)
                  .WithMany(f => f.Tasks)
                  .HasForeignKey(t => t.FamilyId)
                  .OnDelete(DeleteBehavior.Restrict);



            });

            var familyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var momId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var dadId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var task1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var task2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

            modelBuilder.Entity<Family>().HasData(
                new Family
                {
                    Id = familyId,
                    Name = "Test Family"
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = momId,
                    Name = "Mom",
                    Email = "mom@test.com",
                    Role = ParentRole.Mom,
                    AvailabilityScore = 70,
                    FamilyId = familyId
                },
                new User
                {
                    Id = dadId,
                    Name = "Dad",
                    Email = "dad@test.com",
                    Role = ParentRole.Dad,
                    AvailabilityScore = 90,
                    FamilyId = familyId
                }
            );

            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    Id = task1Id,
                    Title = "Buy groceries",
                    FamilyId = familyId,
                    CreatedById = momId,
                    AssignedToId = dadId,
                    Priority = TaskPriority.Medium,
                    Status = TaskStatus.Pending,
                    MentalLoadEstimate = 30,
                    EstimatedMinutes = 45
                },
                new TaskItem
                {
                    Id = task2Id,
                    Title = "Doctor appointment",
                    FamilyId = familyId,
                    CreatedById = dadId,
                    AssignedToId = null,
                    Priority = TaskPriority.High,
                    Status = TaskStatus.Pending,
                    MentalLoadEstimate = 70,
                    EstimatedMinutes = 20
                }
            );

            modelBuilder.Entity<FamilyProfile>()
    .HasOne(fp => fp.Family)
    .WithMany()
    .HasForeignKey(fp => fp.FamilyId);


            modelBuilder.Entity<HouseholdPlan>()
    .HasOne(x => x.Family)
    .WithMany()
    .HasForeignKey(x => x.FamilyId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyReflection>()
    .HasOne(r => r.User)
    .WithMany()
    .HasForeignKey(r => r.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyReflection>()
                .HasOne(r => r.Family)
                .WithMany()
                .HasForeignKey(r => r.FamilyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyReflection>()
                .HasMany(r => r.Activities)
                .WithOne(a => a.DailyReflection)
                .HasForeignKey(a => a.DailyReflectionId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ActivityLog>()
    .HasOne(a => a.User)
    .WithMany()
    .HasForeignKey(a => a.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovedPlan>()
    .HasOne(x => x.Family)
    .WithMany()
    .HasForeignKey(x => x.FamilyId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApprovedPlanItem>()
    .HasOne(x => x.ApprovedPlan)
    .WithMany(x => x.Items)
    .HasForeignKey(x => x.ApprovedPlanId)
    .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<TaskItem>()
    .HasIndex(t => t.ApprovedPlanItemId)
    .IsUnique()
    .HasFilter("[ApprovedPlanItemId] IS NOT NULL");
        }
    }
}
