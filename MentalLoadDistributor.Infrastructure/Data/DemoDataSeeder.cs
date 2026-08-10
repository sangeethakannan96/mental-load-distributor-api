

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MentalLoadDistributor.Infrastructure.Data;

public class DemoDataSeeder
{
    private readonly AppDbContext _context;

    public DemoDataSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Avoid duplicating demo data
        if (await _context.DailyReflections.AnyAsync())
            return;

        // Try to reuse the seeded Test Family & users if present
        var family = await _context.Families.FirstOrDefaultAsync(f => f.Name == "Test Family");

        Guid momId;
        Guid dadId;

        if (family == null)
        {
            family = new Core.Domain.Models.Family { Name = "Demo Family" };
            _context.Families.Add(family);
            await _context.SaveChangesAsync();
        }

        var users = await _context.Users.Where(u => u.FamilyId == family.Id).ToListAsync();

        var mom = users.FirstOrDefault(u => u.Role == Core.Domain.Models.ParentRole.Mom);
        var dad = users.FirstOrDefault(u => u.Role == Core.Domain.Models.ParentRole.Dad);

        if (mom == null)
        {
            mom = new Core.Domain.Models.User
            {
                Name = "Alice",
                Email = "alice@demo.local",
                FamilyId = family.Id,
                Role = Core.Domain.Models.ParentRole.Mom,
                AvailabilityScore = 75,
                Skills = new List<string> { "cooking", "planning" }
            };
            _context.Users.Add(mom);
        }

        if (dad == null)
        {
            dad = new Core.Domain.Models.User
            {
                Name = "Bob",
                Email = "bob@demo.local",
                FamilyId = family.Id,
                Role = Core.Domain.Models.ParentRole.Dad,
                AvailabilityScore = 85,
                Skills = new List<string> { "shopping", "maintenance" }
            };
            _context.Users.Add(dad);
        }

        await _context.SaveChangesAsync();

        momId = mom.Id;
        dadId = dad.Id;

        // Ensure users have a rich set of skills used to match task tags
        var requiredMomSkills = new[] { "cooking", "planning", "childcare", "cleaning" };
        var requiredDadSkills = new[] { "shopping", "maintenance", "finance", "transportation" };

        mom.Skills = mom.Skills.Union(requiredMomSkills).ToList();
        dad.Skills = dad.Skills.Union(requiredDadSkills).ToList();

        await _context.SaveChangesAsync();

        // Create or update family profile
        var existingProfile = await _context.FamilyProfiles.FirstOrDefaultAsync(fp => fp.FamilyId == family.Id);
        if (existingProfile == null)
        {
            existingProfile = new Core.Domain.Models.FamilyProfile
            {
                Id = Guid.NewGuid(),
                FamilyId = family.Id,
                HouseholdDescription = "Two-parent household with one school-aged child. Both parents share household and childcare duties.",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            _context.FamilyProfiles.Add(existingProfile);
            await _context.SaveChangesAsync();
        }

        // Create some family tasks
        var rng = new Random(42);

        // Create 10 tasks per day for the 30-day period, split evenly between mom and dad.
        var taskTemplates = new[]
        {
            new { Title = "Buy groceries", Tags = new[] { "shopping" }, Priority = Core.Domain.Models.TaskPriority.Medium, Category = Core.Domain.Enums.TaskCategory.Shopping },
            new { Title = "Laundry", Tags = new[] { "cleaning", "laundry" }, Priority = Core.Domain.Models.TaskPriority.Low, Category = Core.Domain.Enums.TaskCategory.Household },
            new { Title = "School permission form", Tags = new[] { "school" }, Priority = Core.Domain.Models.TaskPriority.High, Category = Core.Domain.Enums.TaskCategory.School },
            new { Title = "Weekly meal planning", Tags = new[] { "planning", "cooking" }, Priority = Core.Domain.Models.TaskPriority.Medium, Category = Core.Domain.Enums.TaskCategory.Cooking },
            new { Title = "Car maintenance check", Tags = new[] { "maintenance", "transportation" }, Priority = Core.Domain.Models.TaskPriority.Low, Category = Core.Domain.Enums.TaskCategory.Household },
            new { Title = "Pay utilities", Tags = new[] { "finance" }, Priority = Core.Domain.Models.TaskPriority.High, Category = Core.Domain.Enums.TaskCategory.Finance },
            new { Title = "Prepare school lunch", Tags = new[] { "cooking", "childcare" }, Priority = Core.Domain.Models.TaskPriority.Medium, Category = Core.Domain.Enums.TaskCategory.Childcare },
            new { Title = "Doctor appointment booking", Tags = new[] { "health", "planning" }, Priority = Core.Domain.Models.TaskPriority.High, Category = Core.Domain.Enums.TaskCategory.Health },
            new { Title = "Fix leaking tap", Tags = new[] { "maintenance" }, Priority = Core.Domain.Models.TaskPriority.Medium, Category = Core.Domain.Enums.TaskCategory.Household },
            new { Title = "Buy school supplies", Tags = new[] { "shopping", "school" }, Priority = Core.Domain.Models.TaskPriority.Medium, Category = Core.Domain.Enums.TaskCategory.Shopping }
        };

        var dailyTasks = new List<Core.Domain.Models.TaskItem>();

        var today = DateTime.UtcNow.Date;
        var start = today.AddDays(-29);

        for (int d = 0; d < 30; d++)
        {
            var date = start.AddDays(d);
            // create 10 tasks each day
            for (int i = 0; i < 10; i++)
            {
                var tpl = taskTemplates[i % taskTemplates.Length];
                // split first 5 to mom, next 5 to dad
                var assignedTo = (i < 5) ? momId : dadId;
                var createdBy = (i % 2 == 0) ? momId : dadId;

                var estimated = rng.Next(10, 121);
                var mental = Math.Min(100, Math.Max(1, (int)Math.Round(estimated * (0.5 + rng.NextDouble()))));

                var ti = new Core.Domain.Models.TaskItem
                {
                    Title = $"{tpl.Title} - {date:yyyy-MM-dd} #{i + 1}",
                    FamilyId = family.Id,
                    CreatedById = createdBy,
                    AssignedToId = assignedTo,
                    Priority = tpl.Priority,
                    Status = Core.Domain.Enums.TaskStatus.Pending,
                    MentalLoadEstimate = mental,
                    EstimatedMinutes = estimated,
                    Category = tpl.Category,
                    Tags = tpl.Tags.ToList(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "source", "demo" },
                        { "date", date.ToString("yyyy-MM-dd") },
                        { "dailyIndex", i.ToString() }
                    },
                    CreatedAt = date.AddHours(8)
                };

                dailyTasks.Add(ti);
            }
        }

        _context.Tasks.AddRange(dailyTasks);
        await _context.SaveChangesAsync();

        // Generate 30 days of reflections for both parents
        var activityCategories = Enum.GetValues(typeof(Core.Domain.Enums.ActivityCategory)).Cast<Core.Domain.Enums.ActivityCategory>().ToArray();

        var reflections = new List<Core.Domain.Models.DailyReflection>();
        var activityLogs = new List<Core.Domain.Models.ActivityLog>();

        for (int d = 0; d < 30; d++)
        {
            var date = start.AddDays(d);

            foreach (var user in new[] { mom, dad })
            {
                var reflection = new Core.Domain.Models.DailyReflection
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FamilyId = family.Id,
                    ReflectionDate = date,
                    CreatedAt = date.AddHours(21),
                    Content = $"Daily notes for {user.Name} on {date:yyyy-MM-dd}",
                };

                // More activities for mom than dad: mom 4-7, dad 1-3
                int activityCount;
                if (user.Id == mom.Id)
                {
                    activityCount = rng.Next(4, 8); // mom does majority
                }
                else
                {
                    activityCount = rng.Next(1, 4); // dad fewer activities
                }
                for (int i = 0; i < activityCount; i++)
                {
                    var category = activityCategories[rng.Next(activityCategories.Length)];
                    var minutes = rng.Next(5, 121);
                    var mentalLoad = Math.Min(100, Math.Max(1, (int)Math.Round(minutes * (0.6 + rng.NextDouble() * 0.8))));

                    var occurred = date.AddHours(rng.Next(7, 22)).AddMinutes(rng.Next(0, 60));

                    var a = new Core.Domain.Models.ActivityLog
                    {
                        Id = Guid.NewGuid(),
                        DailyReflectionId = reflection.Id,
                        UserId = user.Id,
                        Title = category.ToString() + " activity",
                        Description = $"{category} work taking approx {minutes} minutes.",
                        Category = category,
                        EstimatedMinutes = minutes,
                        MentalLoadScore = mentalLoad,
                        WasPlanned = rng.NextDouble() > 0.4,
                        OccurredAt = occurred
                    };

                    reflection.Activities.Add(a);
                    activityLogs.Add(a);
                }

                // simple summary
                reflection.Summary = $"Total activities: {reflection.Activities.Count}, approx mental load avg {(int)reflection.Activities.Average(x => x.MentalLoadScore)}";

                reflections.Add(reflection);
            }
        }

        _context.DailyReflections.AddRange(reflections);
        _context.ActivityLogs.AddRange(activityLogs);

        // Mark some tasks as completed on random days
        var allTasks = await _context.Tasks.Where(t => t.FamilyId == family.Id).ToListAsync();
        foreach (var t in allTasks)
        {
            if (rng.NextDouble() > 0.5)
            {
                var completedOn = start.AddDays(rng.Next(0, 30)).AddHours(rng.Next(8, 20));
                t.Status = Core.Domain.Enums.TaskStatus.Completed;
                t.CompletedAt = completedOn;
            }
        }

        await _context.SaveChangesAsync();
    }
}