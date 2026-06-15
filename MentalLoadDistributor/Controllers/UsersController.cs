using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO.Family;
using MentalLoadDistributor.DTO.User;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;

        public UsersController(IUserRepository userRepository, ITaskRepository taskRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userRepository
                .GetAsync(Guid.Parse(userId));

            if (user == null)
                return Unauthorized();

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.FamilyId,
                user.Role,
                user.AvailabilityScore,
                user.Skills
            });
        }

        [HttpGet("me/stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var userId = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var myUserId =
                Guid.Parse(userId);

            var tasks =
                await _taskRepository
                    .GetAllAsync();

            var myTasks = tasks
                .Where(t =>
                    t.AssignedToId == myUserId)
                .ToList();

            var totalTasks =
                myTasks.Count;

            var completedTasks =
                myTasks.Count(t =>
                    t.IsCompleted);

            var pendingTasks =
                totalTasks -
                completedTasks;

            var completionRate =
                totalTasks == 0
                    ? 0
                    : Math.Round(
                        (double)completedTasks /
                        totalTasks * 100,
                        1);

            return Ok(new
            {
                TotalTasks = totalTasks,

                CompletedTasks =
                    completedTasks,

                PendingTasks =
                    pendingTasks,

                CompletionRate =
                    completionRate
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAsync();

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                AvailabilityScore = u.AvailabilityScore
            }).ToList();

            return Ok(result);
        }

        

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var u = await _userRepository.GetAsync(id);
            if (u == null) return NotFound();
            var result = new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                AvailabilityScore = u.AvailabilityScore
            };
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email
            };

            await _userRepository.AddAsync(user);
            var createdUser = await _userRepository.GetAsync(user.Id);
            var result = new UserDto
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                AvailabilityScore = createdUser.AvailabilityScore
            };

            return Ok(result);
           
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userRepository.GetAsync(id);

            if (user == null)
                return NotFound();

            user.Name = dto.Name;

            user.AvailabilityScore =
                dto.AvailabilityScore;

            user.Email = dto.Email;

            user.Role = Enum.Parse<ParentRole>(dto.Role);

            user.Skills = dto.Skills;

            await _userRepository.UpdateAsync(user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userRepository.RemoveAsync(id);
            return NoContent();
        }
    }
}
