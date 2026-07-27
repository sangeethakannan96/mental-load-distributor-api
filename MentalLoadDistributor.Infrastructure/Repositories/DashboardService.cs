using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.Dashboards;
using MentalLoadDistributor.Core.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.Infrastructure.Repositories
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;

        public DashboardService(
            IUserRepository userRepository,
            ITaskRepository taskRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }


        public async Task<DashboardDto> GetDashboardAsync(Guid userId)
        {
            var user = await GetCurrentUserAsync(userId);
            var familyTasks = await GetTodayFamilyTasksAsync(user.FamilyId!.Value);

            return new DashboardDto
            {
                Summary = BuildSummary(familyTasks),
                FamilyProgress = BuildFamilyProgress(user.Family!, familyTasks),
                RecentCompletedTasks = BuildRecentCompletedTasks(familyTasks)
            };
        }


        private async Task<User> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.GetAsync(userId);

            if (user == null)
                throw new Exception("User not found.");

            return user;
        }

        private async Task<List<TaskItem>> GetTodayFamilyTasksAsync(Guid familyId)
        {
            var today = DateTime.Today;

            return await _taskRepository.GetByFamilyIdAsync(
                familyId,
                today,
                today.AddDays(1));
        }

        private DashboardSummaryDto BuildSummary(List<TaskItem> tasks)
        {
            return new DashboardSummaryDto
            {
                TotalTasks = tasks.Count,

                PendingTasks = tasks.Count(t =>
                    t.Status == TaskStatus.Pending),

                CompletedTasks = tasks.Count(t =>
                    t.Status == TaskStatus.Completed),

                UnassignedTasks = tasks.Count(t =>
                    t.AssignedToId == null)
            };
        }

        private List<MemberTodayDto> BuildFamilyProgress(
    Family family,
    List<TaskItem> tasks)
        {
            return family.Members
                .Select(member => new MemberTodayDto
                {
                    UserId = member.Id,
                    UserName = member.Name,

                    CompletedTasks = tasks.Count(t =>
                        t.AssignedToId == member.Id &&
                        t.Status == TaskStatus.Completed),

                    PendingTasks = tasks.Count(t =>
                        t.AssignedToId == member.Id &&
                        t.Status == TaskStatus.Pending)
                })
                .ToList();
        }

        private List<RecentCompletedTaskDto> BuildRecentCompletedTasks(
    List<TaskItem> tasks)
        {
            return tasks
                .Where(t => t.Status == TaskStatus.Completed)
                .OrderByDescending(t => t.CompletedAt)
                .Take(10)
                .Select(t => new RecentCompletedTaskDto
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    CompletedBy = t.AssignedTo?.Name ?? "Unknown",
                    CompletedAt = t.CompletedAt!.Value,
                    Priority = t.Priority
                })
                .ToList();
        }
      
    }
}
