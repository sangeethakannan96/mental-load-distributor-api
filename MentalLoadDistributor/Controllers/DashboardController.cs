
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
      
        private readonly IDashboardService _dashboardService;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public DashboardController(
            IDashboardService dashboardService,
        ITaskRepository taskRepository,
        IUserRepository userRepository)
        {
            _dashboardService = dashboardService;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }


        [Authorize]
        [HttpGet("family-review")]
        public async Task<IActionResult> GetFamilyReview()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var myUserId = Guid.Parse(userId);

            var user =
                await _userRepository.GetAsync(myUserId);

            if (user?.FamilyId == null)
                return BadRequest("User is not assigned to a family.");

            var tasks =
                await _taskRepository.GetFamilyReviewTasksAsync(user.FamilyId.Value);

            return Ok(tasks);
        }


        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dashboard =
                await _dashboardService.GetDashboardAsync(Guid.Parse(userId));

            return Ok(dashboard);
        }
    }
}
