using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.Insights;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Core.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.Infrastructure.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IReflectionRepository _reflectionRepository;

        public InsightsService(
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IFamilyRepository familyRepository,
            IReflectionRepository reflectionRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _familyRepository = familyRepository;
            _reflectionRepository = reflectionRepository;
        }

        public async Task<InsightsDto> GetInsightsAsync(
     Guid familyId,
     string period)
        {
            var (startDate, endDate) = GetDateRange(period);

            var family = await _familyRepository
                .GetWithMembersAsync(familyId);

            if (family == null)
                throw new Exception("Family not found.");

            var tasks = (await _taskRepository
                .GetByFamilyIdAsync(familyId))
                .ToList();

            var reflections = await _reflectionRepository
                .GetByFamilyAsync(familyId);

            var filteredTasks = tasks
    .Where(t => t.CreatedAt >= startDate &&
                t.CreatedAt < endDate)
    .ToList();

            var filteredReflections = reflections
                .Where(r => r.ReflectionDate >= startDate &&
                            r.ReflectionDate < endDate)
                .ToList();

            return new InsightsDto
            {
                
                Overview = BuildOverview(filteredTasks, filteredReflections),

                Tasks = BuildTaskInsights(filteredTasks),

                Family = BuildMemberInsights(
                            family.Members.ToList(),
                            filteredTasks,
                            filteredReflections),

                Reflections = BuildReflectionInsights(filteredReflections),

                ReflectionSummary = BuildReflectionSummary(filteredReflections)
            };
        }

        private (DateTime Start, DateTime End) GetDateRange(string period)
        {
            var today = DateTime.Today;

            period = period.ToLower();

            return period switch
            {
                "today" =>
                    (today, today.AddDays(1)),

                "yesterday" =>
                    (today.AddDays(-1), today),

                "week" =>
                    (today.AddDays(-(int)today.DayOfWeek), today.AddDays(1)),

                "month" =>
                    (new DateTime(today.Year, today.Month, 1),
                     today.AddDays(1)),

                "quarter" =>
                    (
                        new DateTime(
                            today.Year,
                            ((today.Month - 1) / 3) * 3 + 1,
                            1),
                        today.AddDays(1)
                    ),

                "year" =>
                    (
                        new DateTime(today.Year, 1, 1),
                        today.AddDays(1)
                    ),

                _ =>
                    (
                        new DateTime(today.Year, today.Month, 1),
                        today.AddDays(1)
                    )
            };
        }

        private OverviewInsightsDto BuildOverview(
    List<TaskItem> tasks,
    List<DailyReflection> reflections)
        {
            var completedTasks = tasks.Count(t =>
                t.Status == TaskStatus.Completed);

            var activities = reflections
                .SelectMany(r => r.Activities)
                .ToList();

            var activitiesCaptured = activities.Count;

            var totalMinutes = activities.Sum(a => a.EstimatedMinutes);

            var mentalLoadScore = activities.Sum(a => a.MentalLoadScore);

            var divisionOfWork = tasks
                .Where(t =>
                    t.Status == TaskStatus.Completed &&
                    t.AssignedTo != null)
                .GroupBy(t => new
                {
                    t.AssignedToId,
                    t.AssignedTo!.Name
                })
                .Select(g => new MemberContributionDto
                {
                    UserId = g.Key.AssignedToId!.Value,
                    UserName = g.Key.Name,
                    CompletedTasks = g.Count(),
                    ActivitiesCaptured = activities.Count(a =>
                        a.UserId == g.Key.AssignedToId)
                })
                .OrderByDescending(x => x.TotalContributions)
                .ToList();

            var overview = new OverviewInsightsDto
            {
                TasksCompleted = completedTasks,
                ActivitiesCaptured = activitiesCaptured,
                TotalContributions = completedTasks + activitiesCaptured,
                TotalMinutes = totalMinutes,
                MentalLoadScore = mentalLoadScore,
                DivisionOfWork = divisionOfWork
            };

            overview.AIInsight = GenerateOverviewInsight(overview);

            return overview;
        }

        private string GenerateOverviewInsight(
    OverviewInsightsDto overview)
        {
            if (!overview.DivisionOfWork.Any())
                return "No contributions were recorded during this period.";

            var topContributor = overview.DivisionOfWork.First();

            return $"{topContributor.UserName} contributed the most with {topContributor.TotalContributions} contributions. The family completed {overview.TasksCompleted} tasks and captured {overview.ActivitiesCaptured} activities.";
     
     }


        private TaskInsightsDto BuildTaskInsights(
    List<TaskItem> tasks)
        {
            return new TaskInsightsDto
            {
                TotalTasks = tasks.Count,

                CompletedTasks = tasks.Count(t =>
                    t.Status == TaskStatus.Completed),

                PendingTasks = tasks.Count(t =>
                    t.Status == TaskStatus.Pending ||
                    t.Status == TaskStatus.InProgress ||
                    t.Status == TaskStatus.Deferred),

                CancelledTasks = tasks.Count(t =>
                    t.Status == TaskStatus.Cancelled),

                OverdueTasks = tasks.Count(t =>
                    t.Status != TaskStatus.Completed &&
                    t.Status != TaskStatus.Cancelled &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date < DateTime.Today)
            };
        }

        private List<MemberInsightsDto> BuildMemberInsights(
    List<User> members,
    List<TaskItem> tasks,
    List<DailyReflection> reflections)
        {
            var activities = reflections
                .SelectMany(r => r.Activities)
                .ToList();

            return members
                .Select(member => new MemberInsightsDto
                {
                    UserId = member.Id,

                    UserName = member.Name,

                    CompletedTasks = tasks.Count(t =>
                        t.AssignedToId == member.Id &&
                        t.Status == TaskStatus.Completed),

                    ActivitiesCaptured = activities.Count(a =>
                        a.UserId == member.Id)
                })
                .OrderByDescending(m => m.TotalContributions)
                .ThenByDescending(m => m.CompletedTasks)
                .ThenBy(m => m.UserName)
                .ToList();
        }

        private ReflectionInsightsDto BuildReflectionInsights(
      List<DailyReflection> reflections)
        {
            return new ReflectionInsightsDto
            {
                ShowReflectionList = reflections.Any(),

                Reflections = reflections
                    .OrderByDescending(r => r.ReflectionDate)
                    .Select(r => new ReflectionDto
                    {
                        Id = r.Id,
                        ReflectionDate = r.ReflectionDate,
                        Content = r.Content,
                        Summary = r.Summary
                    })
                    .ToList()
            };
        }

        private ReflectionSummaryDto BuildReflectionSummary(
    List<DailyReflection> reflections)
        {
            var activities = reflections
                .SelectMany(r => r.Activities)
                .ToList();

            var totalActivities = activities.Count;

            var topCategories = activities
                .GroupBy(a => a.Category)
                .Select(g => new ActivityCategorySummaryDto
                {
                    Category = g.Key.ToString(),
                    Count = g.Count(),
                    EstimatedMinutes = g.Sum(a => a.EstimatedMinutes)
                })
                .OrderByDescending(c => c.Count)
                .ToList();

            var summary = new ReflectionSummaryDto
            {
                TotalReflections = reflections.Count,

                TotalActivitiesCaptured = totalActivities,

                TotalEstimatedMinutes = activities.Sum(a => a.EstimatedMinutes),

                MentalLoadScore = activities.Sum(a => a.MentalLoadScore),

                // Since ActivityLog doesn't have an IsInvisibleWork property,
                // we can't calculate this yet.
                InvisibleWorkPercentage = 0,

                TopCategories = topCategories
            };

            summary.AiSummary = GenerateReflectionSummary(summary);

            return summary;
        }

        private string GenerateReflectionSummary(
    ReflectionSummaryDto summary)
        {
            if (summary.TotalReflections == 0)
                return "No reflections were recorded during this period.";

            if (!summary.TopCategories.Any())
                return $"{summary.TotalReflections} reflections were recorded during this period.";

            var topCategory = summary.TopCategories.First();

            return $"A total of {summary.TotalActivitiesCaptured} activities were captured. The most common category was '{topCategory.Category}' with {topCategory.Count} activities.";
        }




    }



}
