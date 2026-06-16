using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO.Dashboards;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public DashboardController(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        [HttpGet()]
        public async Task<IActionResult> GetDashboard()
        {


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository
                .GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
            {
                return BadRequest("User has no family");
            }

            var tasks = await _taskRepository
                .GetByFamilyIdAsync(currentUser.FamilyId.Value);

            var myActiveTasks = tasks
                    .Where(t =>
                    t.AssignedToId == currentUser.Id &&
                    !t.IsCompleted)
                    .Select(t => new MyActiveTaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Priority = (int)t.Priority,
                        DueDate = t.DueDate
                    }).ToList();

            var userLoads = tasks
                .Where(t => t.AssignedTo != null)
                .GroupBy(t => t.AssignedTo.Name)
                .Select(g => new UserLoadDto
                {
                    UserName = g.Key,
                    TotalMinutes = g.Sum(t => t.EstimatedMinutes)
                })
                .ToList();

            var mostLoaded = userLoads
                 .OrderByDescending(u => u.TotalMinutes)
                 .FirstOrDefault();

            var leastLoaded = userLoads
                .OrderBy(u => u.TotalMinutes)
                .FirstOrDefault();

            var avgLoad = userLoads.Any()
                ? userLoads.Average(u => u.TotalMinutes)
                : 0;

            var overloaded = userLoads
                .Where(u => u.TotalMinutes > avgLoad * 1.2)
                .Select(u => u.UserName)
                .ToList();

            var underutilized = userLoads
                .Where(u => u.TotalMinutes < avgLoad * 0.8)
                .Select(u => u.UserName)
                .ToList();

            var maxLoad = userLoads.Any() ? userLoads.Max(u => u.TotalMinutes) : 0;
            var minLoad = userLoads.Any() ? userLoads.Min(u => u.TotalMinutes) : 0;

            var imbalance = maxLoad - minLoad;

            // 5️⃣ Recommendation
            string recommendation;

            if (imbalance > 60)
                recommendation = "Workload is highly imbalanced.";
            else if (overloaded.Any())
                recommendation = "Some users are overloaded.";
            else if (underutilized.Any())
                recommendation = "Some users have low workload.";
            else
                recommendation = "Workload is balanced.";

            var overdueTasks = tasks.Count(t =>
                !t.IsCompleted &&
                t.DueDate.HasValue &&
                t.DueDate.Value.Date
                < DateTime.UtcNow.Date);

            var dueTodayTasks = tasks.Count(t =>
                !t.IsCompleted &&
                t.DueDate.HasValue &&
                t.DueDate.Value.Date
                    == DateTime.UtcNow.Date);

            var urgentTasks = tasks.Count(t =>
                !t.IsCompleted &&
                t.Priority == TaskPriority.High);

            // 6️⃣ Final result
            var result = new DashboardDto
            {
                TotalTasks = tasks.Count(),
                CompletedTasks = tasks.Count(t => t.IsCompleted),
                PendingTasks = tasks.Count(t => !t.IsCompleted),

                UnassignedTasks = tasks.Count(t => t.AssignedTo == null),

                LoadPerUser = userLoads,
                MostLoadedUser = mostLoaded?.UserName,
                LeastLoadedUser = leastLoaded?.UserName,
                AverageLoad = avgLoad,
                OverloadedUsers = overloaded,
                UnderutilizedUsers = underutilized,
                LoadImbalanceScore = imbalance,
                Recommendation = recommendation,
                OverdueTasks = overdueTasks,

                TasksDueToday = dueTodayTasks,

                UrgentTasks = urgentTasks,

                MyActiveTasks = myActiveTasks
            };

            return Ok(result);
        }
    }
}
