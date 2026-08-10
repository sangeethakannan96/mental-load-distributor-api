
using MentalLoadDistributor.Core.Ports;
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

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
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

        [HttpGet("family-review")]
        public async Task<IActionResult> GetFamilyReviewTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var tasks = await _dashboardService
                .GetFamilyReviewTasksAsync(Guid.Parse(userId));

            return Ok(tasks);
        }
    }
}
