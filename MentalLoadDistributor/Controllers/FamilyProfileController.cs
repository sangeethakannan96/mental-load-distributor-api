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

        public FamilyProfileController(
            IFamilyProfileRepository repository,
            IUserRepository userRepository,
            ITaskSuggestionService taskSuggestionService)
        {
            _repository = repository;
            _userRepository = userRepository;
            _taskSuggestionService = taskSuggestionService;
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

    }
}

