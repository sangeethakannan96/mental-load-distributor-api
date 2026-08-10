using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Ports;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.Core.Services
{
    public class TaskBulkActionService : ITaskBulkActionService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TaskBulkActionService(
            ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(
            BulkTaskActionDto request,
            Guid userId)
        {
            if (request.TaskIds == null ||
                request.TaskIds.Count == 0)
            {
                throw new ArgumentException(
                    "No tasks were selected.");
            }

            // Get authenticated user
            var currentUser =
                await _userRepository.GetAsync(userId);

            if (currentUser == null)
                throw new ArgumentException(
                    "User not found.");

            if (currentUser.FamilyId == null)
                throw new ArgumentException(
                    "User has no family.");

            // Validate that every selected task belongs
            // to the user's family
            foreach (var taskId in request.TaskIds)
            {
                var task =
                    await _taskRepository.GetAsync(taskId);

                if (task == null)
                    throw new ArgumentException(
                        $"Task {taskId} was not found.");

                if (task.FamilyId != currentUser.FamilyId.Value)
                {
                    throw new UnauthorizedAccessException(
                        "You are not authorized to modify one or more selected tasks.");
                }
            }


            switch (request.Action)
            {
                case BulkTaskAction.Complete:
                    await CompleteAsync(request.TaskIds);
                    break;

                case BulkTaskAction.Cancel:
                    await CancelAsync(request.TaskIds);
                    break;

                case BulkTaskAction.Delete:
                    await DeleteAsync(request.TaskIds);
                    break;

                case BulkTaskAction.Reassign:
                    await ReassignAsync(
                        request.TaskIds,
                        request.AssignedToId,
                        currentUser.FamilyId.Value);
                    break;

                case BulkTaskAction.Postpone:
                    await PostponeAsync(
                        request.TaskIds,
                        request.DueDate);
                    break;

                default:
                    throw new ArgumentException(
                        "Invalid bulk task action.");
            }
        }

        private async Task CompleteAsync(
            List<Guid> taskIds)
        {
            foreach (var taskId in taskIds)
            {
                var task =
                    await _taskRepository.GetAsync(taskId);

                if (task == null)
                    continue;

                if (task.Status == TaskStatus.Completed)
                    continue;

                task.Status = TaskStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;

                // Create next occurrence for recurring task
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

                        FamilyId = task.FamilyId,

                        CreatedById = task.CreatedById,

                        AssignedToId = task.AssignedToId,

                        Category = task.Category,

                        EstimatedMinutes =
                            task.EstimatedMinutes,

                        MentalLoadEstimate =
                            task.MentalLoadEstimate,

                        DueDate = nextDueDate,

                        Priority = task.Priority,

                        Status = TaskStatus.Pending,

                        CreatedAt = DateTime.UtcNow,

                        Tags = new List<string>(task.Tags),

                        Recurrence = task.Recurrence
                    };

                    await _taskRepository.AddAsync(nextTask);
                }

                await _taskRepository.UpdateAsync(task);
            }
        }

        private async Task CancelAsync(
            List<Guid> taskIds)
        {
            foreach (var taskId in taskIds)
            {
                var task =
                    await _taskRepository.GetAsync(taskId);

                if (task == null)
                    continue;

                if (task.Status == TaskStatus.Completed)
                    continue;

                task.Status = TaskStatus.Cancelled;

                await _taskRepository.UpdateAsync(task);
            }
        }

        private async Task DeleteAsync(
            List<Guid> taskIds)
        {
            foreach (var taskId in taskIds)
            {
                await _taskRepository.RemoveAsync(taskId);
            }
        }

        private async Task ReassignAsync(
    List<Guid> taskIds,
    Guid? assignedToId,
    Guid familyId)
        {
            if (!assignedToId.HasValue)
            {
                throw new ArgumentException(
                    "AssignedToId is required for reassignment.");
            }

            var assignedUser =
                await _userRepository.GetAsync(assignedToId.Value);

            if (assignedUser == null)
            {
                throw new ArgumentException(
                    "Assigned user not found.");
            }

            if (assignedUser.FamilyId != familyId)
            {
                throw new UnauthorizedAccessException(
                    "You can only assign tasks to members of your family.");
            }

            foreach (var taskId in taskIds)
            {
                var task =
                    await _taskRepository.GetAsync(taskId);

                if (task == null)
                    continue;

                task.AssignedToId = assignedToId.Value;

                await _taskRepository.UpdateAsync(task);
            }
        }

        private async Task PostponeAsync(
            List<Guid> taskIds,
            DateTime? dueDate)
        {
            if (!dueDate.HasValue)
            {
                throw new ArgumentException(
                    "DueDate is required for postponing tasks.");
            }

            foreach (var taskId in taskIds)
            {
                var task =
                    await _taskRepository.GetAsync(taskId);

                if (task == null)
                    continue;

                task.DueDate = dueDate.Value;

                await _taskRepository.UpdateAsync(task);
            }
        }
    }
}