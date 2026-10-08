using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO.HouseholdPlan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HouseholdPlanController : ControllerBase
    {
        private readonly IHouseholdPlanRepository _repository;
        private readonly IUserRepository _userRepository;

        public HouseholdPlanController(
            IHouseholdPlanRepository repository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
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

            var plan =
                await _repository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (plan == null)
                return NotFound();

            return Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateHouseholdPlanDto dto)
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
                    "Household plan already exists");

            var plan =
                new HouseholdPlan
                {
                    Id = Guid.NewGuid(),

                    FamilyId =
                        user.FamilyId.Value,

                    PlanDescription =
                        dto.PlanDescription,

                    CreatedOn =
                        DateTime.UtcNow,

                    UpdatedOn =
                        DateTime.UtcNow
                };

            await _repository.AddAsync(plan);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] UpdateHouseholdPlanDto dto)
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

            var plan =
                await _repository
                    .GetByFamilyIdAsync(
                        user.FamilyId.Value);

            if (plan == null)
                return NotFound();

            plan.PlanDescription =
                dto.PlanDescription;

            plan.UpdatedOn =
                DateTime.UtcNow;

            await _repository.UpdateAsync(plan);

            return NoContent();
        }
    }
}