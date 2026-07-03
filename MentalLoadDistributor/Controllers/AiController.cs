using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Models.AI;
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
        private readonly ITaskSuggestionService _taskSuggestionService;
        private readonly IFamilyProfileRepository _familyProfileRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public AiController(IAiService aiService,
            IFamilyProfileRepository familyprofileRepository,
            ITaskSuggestionService taskSuggestionService,
            ITaskRepository taskRepository,
            IUserRepository userRepository)
        {
            _aiService = aiService;
            _taskSuggestionService = taskSuggestionService;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
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

        [HttpPost("generate-household-suggestions")]
        public async Task<IActionResult>
   GenerateHouseholdSuggestions()
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
                await _taskSuggestionService
                    .GenerateHouseholdSuggestionsAsync(
                        profile
                            .HouseholdDescription);

            return Ok(suggestions);
        }


        [HttpPost("generate-task-suggestions")]
        public async Task<IActionResult> GenerateTaskSuggestions(
    [FromBody] GenerateTaskSuggestionsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Prompt is required.");
            }

            var suggestions =
                await _taskSuggestionService
                    .GenerateTaskSuggestionsAsync(request.Prompt);

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

    
}
}
