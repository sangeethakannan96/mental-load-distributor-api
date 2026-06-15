using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Core.Services;
using MentalLoadDistributor.DTO.Task;
using MentalLoadDistributor.DTO.User;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly DelegationService _delegation;

        public TasksController(ITaskRepository taskRepository, IUserRepository userRepository, IFamilyRepository familyRepository, DelegationService delegation)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _familyRepository = familyRepository;
            _delegation = delegation;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository
                .GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
            {
                return BadRequest("User has no family");
            }

            var tasks = await _taskRepository
                .GetByFamilyIdAsync(currentUser.FamilyId.Value);

            var result = tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                EstimatedMinutes = t.EstimatedMinutes,
                IsCompleted = t.IsCompleted,
                DueDate = t.DueDate,
                Recurrence = t.Recurrence,
                Priority = t.Priority,
                Tags = t.Tags,
                CreatedBy = new UserDto
                {
                    Id = t.CreatedBy.Id,
                    Name = t.CreatedBy.Name,
                    AvailabilityScore = t.CreatedBy.AvailabilityScore
                },
                AssignedTo = t.AssignedTo != null
                    ? new UserDto
                    {
                        Id = t.AssignedTo.Id,
                        Name = t.AssignedTo.Name,
                        AvailabilityScore = t.AssignedTo.AvailabilityScore
                    }
                    : null
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var t = await _taskRepository.GetAsync(id);

            if (t == null)
                return NotFound();

            var result = new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Recurrence = t.Recurrence,
                Tags = t.Tags,

                CreatedBy = new UserDto
                {
                    Id = t.CreatedBy.Id,
                    Name = t.CreatedBy.Name,
                    AvailabilityScore = t.CreatedBy.AvailabilityScore
                },

                AssignedTo = t.AssignedTo != null
                    ? new UserDto
                    {
                        Id = t.AssignedTo.Id,
                        Name = t.AssignedTo.Name,
                        AvailabilityScore = t.AssignedTo.AvailabilityScore
                    }
                    : null,

                IsCompleted = t.IsCompleted,

                DueDate = t.DueDate,

                Priority = t.Priority
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
     [FromBody] CreateTaskDto dto)
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

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,

                CreatedById = currentUser.Id,

                EstimatedMinutes =
                    dto.EstimatedMinutes,

                EmotionalLoadEstimate =
                    dto.EmotionalLoadEstimate,

                Tags = dto.Tags,

                DueDate = dto.DueDate,

                Priority = dto.Priority,

                Recurrence = dto.Recurrence
            };

            await _taskRepository.AddAsync(task);

            var createdTask = await _taskRepository
                .GetAsync(task.Id);

            var result = new TaskDto
            {
                Id = createdTask.Id,
                Title = createdTask.Title,
                Description =
                    createdTask.Description,

                DueDate = createdTask.DueDate,

                Priority = createdTask.Priority,

                IsCompleted =
                    createdTask.IsCompleted,

                Tags = createdTask.Tags,

                CreatedBy = new UserDto
                {
                    Id = createdTask.CreatedBy.Id,
                    Name =
                        createdTask.CreatedBy.Name,

                    AvailabilityScore =
                        createdTask.CreatedBy
                            .AvailabilityScore
                },

                Recurrence = createdTask.Recurrence
            };

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto)
        {
            var task = await _taskRepository.GetAsync(id);

            if (task == null)
                return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.EstimatedMinutes = dto.EstimatedMinutes;
            task.IsCompleted = dto.IsCompleted;
            task.DueDate = dto.DueDate;
            task.Priority = dto.Priority;
            task.Recurrence = dto.Recurrence;
            task.AssignedToId = dto.AssignedToId;
            task.Tags = dto.Tags;
            task.AssignedTo = null;

            await _taskRepository.UpdateAsync(task);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskRepository.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost("{taskId}/suggest-assignee")]
        public async Task<IActionResult> SuggestAssignee(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);

            if (task == null)
                return NotFound("Task not found");

            if (task.CreatedBy == null)
                return BadRequest("User has no family");

            if (task.CreatedBy.FamilyId == null)
            {
                return BadRequest("User has no family");
            }

            var family = await _familyRepository.GetWithMembersAsync(task.CreatedBy.FamilyId.Value);

            if (family == null)
                return BadRequest("Family not found");

            var allTasks = (await _taskRepository.GetByFamilyIdAsync(task.CreatedBy.FamilyId.Value)).ToList();

            var recommendation = _delegation.ExplainRecommendation(family, task, allTasks);

            return Ok(recommendation);


        }

        [HttpPut("{taskId}/assign/{userId}")]
        public async Task<IActionResult> AssignTask(Guid taskId, Guid userId)
        {
            var task = await _taskRepository.GetAsync(taskId);

            if (task == null)
                return NotFound();

            task.AssignedToId = userId;

            await _taskRepository.UpdateAsync(task);
            var updatedTask = await _taskRepository.GetAsync(task.Id);

            var result = new TaskDto
            {
                Id = updatedTask.Id,
                Title = updatedTask.Title,
                CreatedBy = new UserDto
                {
                    Id = updatedTask.CreatedBy.Id,
                    Name = updatedTask.CreatedBy.Name,
                    AvailabilityScore = updatedTask.CreatedBy.AvailabilityScore
                },
                Tags = updatedTask.Tags,
                Recurrence = updatedTask.Recurrence,
                AssignedTo = updatedTask.AssignedTo != null
                ? new UserDto
                {
                    Id = updatedTask.AssignedTo.Id,
                    Name = updatedTask.AssignedTo.Name,
                    AvailabilityScore = updatedTask.AssignedTo.AvailabilityScore
                }
                : null
            
            };

            return Ok(result);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var task = await _taskRepository.GetAsync(id);

            if (task == null)
                return NotFound();

            if (task.IsCompleted)
            {
                return BadRequest(
                    "Task already completed");
            }

            task.IsCompleted = true;

            if (task.Recurrence != RecurrenceType.None)
            {
                DateTime? nextDueDate = null;

                if (task.DueDate.HasValue)
                {
                    nextDueDate =
                        task.Recurrence switch
                        {
                            RecurrenceType.Daily =>
                                task.DueDate.Value.AddDays(1),

                            RecurrenceType.Weekly =>
                                task.DueDate.Value.AddDays(7),

                            RecurrenceType.Monthly =>
                                task.DueDate.Value.AddMonths(1),

                            _ => task.DueDate
                        };
                }

                var nextTask = new TaskItem
                {
                    Title = task.Title,
                    Description = task.Description,

                    CreatedById =
                        task.CreatedById,

                    AssignedToId =
                        task.AssignedToId,

                    DueDate = nextDueDate,

                    Priority = task.Priority,

                    EstimatedMinutes =
                        task.EstimatedMinutes,

                    EmotionalLoadEstimate =
                        task.EmotionalLoadEstimate,

                    Tags = task.Tags,

                    Recurrence = task.Recurrence
                };

                await _taskRepository
                    .AddAsync(nextTask);
            }

            await _taskRepository.UpdateAsync(task);

            return NoContent();
        }
    }
}
