using Humanizer;
using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO.Family;
using MentalLoadDistributor.DTO.User;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliesController : ControllerBase
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IUserRepository _userRepository;

        public FamiliesController(IFamilyRepository familyRepository, IUserRepository userRepository)
        {
            _familyRepository = familyRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var families = await _familyRepository.GetAllAsync();

            var result = families.Select(f => new FamilyDto
            {
               Id = f.Id,
               Name = f.Name,
                Members = f.Members?.Select(m => new UserDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    AvailabilityScore = m.AvailabilityScore
                }).ToList() ?? new List<UserDto>()

            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var f = await _familyRepository.GetAsync(id);
            if (f == null) return NotFound();
            var result = new FamilyDto
            {
                Id = f.Id,
                Name = f.Name,
                Members = f.Members?.Select(m => new UserDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    AvailabilityScore = m.AvailabilityScore
                }).ToList() ?? new List<UserDto>()
            };
            return Ok(result);
        }

        [HttpGet("/api/families/{familyId}/users")]
        public async Task<IActionResult> GetUsersByFamily(Guid familyId)
        {
            var users = await _userRepository.GetByFamilyIdAsync(familyId);

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                AvailabilityScore = u.AvailabilityScore,
                Email = u.Email
            }).ToList();

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFamilyDto dto)
        {
            var userId = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository
                .GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            var family = new Family
            {
                Name = dto.Name
            };

            await _familyRepository.AddAsync(family);

            currentUser.FamilyId = family.Id;

            await _userRepository.UpdateAsync(currentUser);

            var result = new FamilyDto
            {
                Id = family.Id,
                Name = family.Name,
                Members = new List<UserDto>
        {
            new UserDto
            {
                Id = currentUser.Id,
                Name = currentUser.Name,
                AvailabilityScore =
                    currentUser.AvailabilityScore
            }
        }
            };

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilyDto dto)
        {
            var family = await _familyRepository.GetAsync(id);

            if (family == null)
                return NotFound();

            family.Name = dto.Name;

            await _familyRepository.UpdateAsync(family);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _familyRepository.RemoveAsync(id);
            return NoContent();
        }

        [HttpGet("mine/users")]
        public async Task<IActionResult> GetMyFamilyUsers()
        {
            var userId = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository
                .GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family");

            var users = await _userRepository
                .GetByFamilyIdAsync(
                    currentUser.FamilyId.Value
                );

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                AvailabilityScore = u.AvailabilityScore,
                Email = u.Email,
                Role = u.Role.ToString(),
                Skills = u.Skills
            }).ToList();

            return Ok(result);
        }

        [HttpPost("mine/users")]
        public async Task<IActionResult> AddFamilyMember(
    [FromBody] CreateUserDto dto)
        {
            var userId = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository
                .GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                FamilyId = currentUser.FamilyId,
                AvailabilityScore = dto.AvailabilityScore,
                Role = Enum.Parse<ParentRole>(dto.Role),
                Skills = dto.Skills
            };

            await _userRepository.AddAsync(user);

            var result = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                AvailabilityScore =
                    user.AvailabilityScore
            };

            return Ok(result);
        }
    }
}
