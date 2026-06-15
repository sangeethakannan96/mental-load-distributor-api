using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly DelegationService _delegationService;

        public TasksController(ITaskRepository taskRepository, IFamilyRepository familyRepository, DelegationService delegationService)
        {
            _taskRepository = taskRepository;
            _familyRepository = familyRepository;
            _delegationService = delegationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _taskRepository.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var t = await _taskRepository.GetAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItem task)
        {
            await _taskRepository.AddAsync(task);
            var createdTask = await _taskRepository.GetAsync(task.Id);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, TaskItem task)
        {
            if (id != task.Id) return BadRequest();
            var existing = await _taskRepository.GetAsync(id);
            if (existing == null) return NotFound();
            await _taskRepository.UpdateAsync(task);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskRepository.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost("{taskId}/delegate")]
        public async Task<IActionResult> Delegate(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);
            if (task == null) return NotFound();

            var family = await _familyRepository.GetWithMembersAsync(task.CreatedBy.FamilyId);
            if (family == null) return BadRequest("Family not found");

            var allTasks = (await _taskRepository.GetAllAsync()).ToList();
            var assignee = _delegationService.SuggestAssignee(family, task, allTasks);
            if (assignee == null) return BadRequest("No assignee found");

            task.AssignedToId = assignee;
            await _taskRepository.UpdateAsync(task);
            return Ok(await _taskRepository.GetAsync(taskId));
        }
    }
}
