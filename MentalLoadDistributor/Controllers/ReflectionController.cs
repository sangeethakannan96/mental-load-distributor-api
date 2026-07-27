using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReflectionController : ControllerBase
    {
        private readonly IReflectionRepository _reflectionRepository;

        public ReflectionController(
            IReflectionRepository reflectionRepository)
        {
            _reflectionRepository = reflectionRepository;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayReflection()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var reflections = await _reflectionRepository
                .GetTodayByUserAsync(Guid.Parse(userId));

            var response = reflections
                .Select(reflection => new DailyReflectionResponse
                {
                    Id = reflection.Id,
                    ReflectionDate = reflection.ReflectionDate,
                    Content = reflection.Content,
                    Summary = reflection.Summary,

                    Activities = reflection.Activities
                        .Select(activity => new ActivityLogResponse
                        {
                            Id = activity.Id,
                            Title = activity.Title,
                            Description = activity.Description,
                            Category = activity.Category,
                            EstimatedMinutes = activity.EstimatedMinutes,
                            MentalLoadScore = activity.MentalLoadScore,
                            OccuredAt = activity.OccurredAt
                        })
                        .ToList()
                })
                .ToList();

            return Ok(response);
        }
    }
}
