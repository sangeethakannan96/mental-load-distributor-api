using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO.FamilyProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FamilyProfileController : ControllerBase
    {
        private readonly IFamilyProfileRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskSuggestionService _taskSuggestionService;
        private readonly ITaskRepository _taskRepository;

        public FamilyProfileController(
            IFamilyProfileRepository repository,
            IUserRepository userRepository,
            ITaskSuggestionService taskSuggestionService,
            ITaskRepository taskRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _taskSuggestionService = taskSuggestionService;
            _taskRepository = taskRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user =
                await _userRepository.GetAsync(
                    Guid.Parse(userId));

            if (user?.FamilyId == null)
                return BadRequest(
                    "User has no family");

            var profile =
                await _repository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (profile == null)
                return NotFound();

            return Ok(new FamilyProfileDto
            {
                Id = profile.Id,
                FamilyId = profile.FamilyId,
                HouseholdDescription =
                    profile.HouseholdDescription
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(
    [FromBody]
    CreateFamilyProfileDto dto)
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user =
                await _userRepository.GetAsync(
                    Guid.Parse(userId));

            if (user?.FamilyId == null)
                return BadRequest(
                    "User has no family");

            var existing =
                await _repository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (existing != null)
                return BadRequest(
                    "Profile already exists");

            var profile =
                new FamilyProfile
                {
                    Id = Guid.NewGuid(),

                    FamilyId =
                        user.FamilyId.Value,

                    HouseholdDescription =
                        dto.HouseholdDescription,

                    CreatedOn =
                        DateTime.UtcNow,

                    UpdatedOn =
                        DateTime.UtcNow
                };

            await _repository.AddAsync(
                profile);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update(
    [FromBody] UpdateFamilyProfileDto dto)
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user =
                await _userRepository.GetAsync(
                    Guid.Parse(userId));

            if (user?.FamilyId == null)
                return BadRequest(
                    "User has no family");

            var profile =
                await _repository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (profile == null)
                return NotFound();

            profile.HouseholdDescription =
                dto.HouseholdDescription;

            profile.UpdatedOn =
                DateTime.UtcNow;

            await _repository.UpdateAsync(
                profile);

            return NoContent();
        }


        [HttpPost("generate-tasks")]
        public async Task<IActionResult>
    GenerateTasks()
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
                await _repository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (profile == null)
                return NotFound();

            var suggestions =
                await _taskSuggestionService
                    .GenerateSuggestionsAsync(
                        profile
                            .HouseholdDescription);

            return Ok(suggestions);
        }

        [HttpPost("approve-suggestions")]
        public async Task<IActionResult>ApproveSuggestions([FromBody] ApproveSuggestionsRequest request)
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

                        Priority =
                            TaskPriority.Medium,

                        IsCompleted =
                            false,

                        EstimatedMinutes =
                            30,

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

