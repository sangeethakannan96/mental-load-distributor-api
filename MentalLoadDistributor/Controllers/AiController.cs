using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.AI;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IPlanningService _mockPlanningService;
        private readonly IFamilyProfileRepository _familyProfileRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReflectionService _reflectionService;
        private readonly IReflectionRepository _reflectionRepository;
        

        public AiController(IAiService aiService,
            IFamilyProfileRepository familyprofileRepository,
            IPlanningService mockPlanningService,
            ITaskRepository taskRepository,
            IReflectionService reflectionService,
            IReflectionRepository reflectionRepository,
            IUserRepository userRepository)
        {
            _aiService = aiService;
            _mockPlanningService = mockPlanningService;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _reflectionService = reflectionService;
            _reflectionRepository = reflectionRepository;
            _familyProfileRepository = familyprofileRepository;
        }

        [HttpPost("suggest")]
        public async Task<IActionResult> Suggest([FromBody] AiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                return BadRequest("Input is required");

            var result = await _aiService.AskAsync(request.Input);

            return Ok(result);
        }

        [HttpPost("generate-household-plan")]
        public async Task<IActionResult>
   GenerateHouseholdPlan()
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user =
                await _userRepository
                    .GetAsync(
                        Guid.Parse(userId));

            if (user?.FamilyId == null)
                return BadRequest();

            var profile =
                await _familyProfileRepository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (profile == null)
                return NotFound();

            var suggestions =
                await _mockPlanningService
                    .GenerateHouseholdPlanAsync(
                        profile
                            .HouseholdDescription);

            return Ok(suggestions);
        }


        [HttpPost("generate-event-plan")]
        public async Task<IActionResult> GenerateEventPlan(
    [FromBody] GenerateTaskSuggestionsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Prompt is required.");
            }

            var suggestions =
                await _mockPlanningService
                    .GenerateEventPlanAsync(request.Prompt);

            return Ok(suggestions);
        }

        [HttpPost("generate-daily-plan")]
        public async Task<IActionResult> GenerateDailyPlan(
    [FromBody] GenerateDailyPlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Please describe what's happening today.");
            }

            var suggestions = await _mockPlanningService
                .GenerateDailyPlanAsync(request.Prompt);

            return Ok(suggestions);
        }

        [HttpPost("generate-weekly-plan")]
        public async Task<IActionResult> GenerateWeeklyPlan(
     [FromBody] GenerateWeeklyPlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(
                    "Please describe what's happening this week.");
            }

            var suggestions = await _mockPlanningService
                .GenerateWeeklyPlanAsync(request.Prompt);

            return Ok(suggestions);
        }


        [HttpPost("generate-monthly-plan")]
        public async Task<IActionResult> GenerateMonthlyPlan(
    [FromBody] GenerateMonthlyPlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(
                    "Please describe what's happening this month.");
            }

            var suggestions = await _mockPlanningService
                .GenerateMonthlyPlanAsync(request.Prompt);

            return Ok(suggestions);
        }


        [HttpPost("approve-suggestions")]
        public async Task<IActionResult> ApproveSuggestions([FromBody] ApproveSuggestionsRequest request)
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser =
                await _userRepository.GetAsync(
                    Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            foreach (var suggestion
                in request.Suggestions)
            {
                var recurrence =
                    suggestion.Recurrence switch
                    {
                        "Daily" =>
                            RecurrenceType.Daily,

                        "Weekly" =>
                            RecurrenceType.Weekly,

                        "Monthly" =>
                            RecurrenceType.Monthly,

                        _ =>
                            RecurrenceType.None
                    };

                var task =
                    new TaskItem
                    {
                        Title =
                            suggestion.Title,

                        Description =
                            suggestion.Description,

                        CreatedById =
                            currentUser.Id,

                        DueDate = suggestion.StartDate,

                        Priority = suggestion.Priority,


                        IsCompleted =
                            false,

                        EstimatedMinutes = suggestion.EstimatedMinutes,

                        EmotionalLoadEstimate =
                            suggestion.EmotionalLoad,

                        Recurrence =
                            recurrence,

                        Tags =
                            new List<string>
                            {
                        suggestion.Category
                            }
                    };

                await _taskRepository
                    .AddAsync(task);
            }

            return Ok();
        }


        [HttpPost("analyze-reflection")]
        public async Task<IActionResult> AnalyzeReflection(
    [FromBody] AnalyzeReflectionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(
                    "Please enter your reflection.");
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var currentUser = await _userRepository.GetAsync(
                Guid.Parse(userId));

            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.FamilyId == null)
            {
                return BadRequest(
                    "You must belong to a family before adding a reflection.");
            }

            var analysis = await _reflectionService
                .AnalyzeReflectionAsync(request.Content);

            var reflection = new DailyReflection
            {
                Id = Guid.NewGuid(),

                UserId = currentUser.Id,

                FamilyId = currentUser.FamilyId.Value,

                ReflectionDate = DateTime.UtcNow.Date,

                Content = request.Content,

                CreatedAt = DateTime.UtcNow,

                Summary = analysis.Summary,

                Activities = analysis.Activities
                    .Select(activity => new ActivityLog
                    {
                        Id = Guid.NewGuid(),

                        UserId = currentUser.Id,

                        Title = activity.Title,

                        Description = activity.Description,

                        Category = activity.Category,

                        EstimatedMinutes =
                            activity.EstimatedMinutes,

                        MentalLoadScore =
                            activity.MentalLoadScore,

                        ActivityDate = DateTime.UtcNow.Date
                    })
                    .ToList()
            };

            await _reflectionRepository.AddAsync(
                reflection);

            return Ok(analysis);
        }


    }
}
