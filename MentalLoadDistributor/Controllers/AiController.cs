using MentalLoadDistributor.API.DTOs;
using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.AI;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO;
using MentalLoadDistributor.DTOs;
using MentalLoadDistributor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IPlanningService _planningService;
        private readonly IFamilyProfileRepository _familyProfileRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReflectionService _reflectionService;
        private readonly IReflectionRepository _reflectionRepository;
        private readonly IHouseholdPlanRepository _householdPlanRepository;
        private readonly IApprovedPlanRepository _approvedPlanRepository;


        public AiController(IAiService aiService,
            IFamilyProfileRepository familyprofileRepository,
            IPlanningService PlanningService,
            ITaskRepository taskRepository,
            IReflectionService reflectionService,
            IReflectionRepository reflectionRepository,
            IUserRepository userRepository,
            IHouseholdPlanRepository householdPlanRepository,
            IApprovedPlanRepository approvedPlanRepository)
        {
            _aiService = aiService;
            _planningService = PlanningService;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _reflectionService = reflectionService;
            _reflectionRepository = reflectionRepository;
            _familyProfileRepository = familyprofileRepository;
            _householdPlanRepository = householdPlanRepository;
            _approvedPlanRepository = approvedPlanRepository;
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
        public async Task<IActionResult> GenerateHouseholdPlan(
            [FromBody] GenerateHouseholdPlanRequest? request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userRepository.GetAsync(Guid.Parse(userId));

            if (user?.FamilyId == null)
                return BadRequest("User has no family.");

            var familyId = user.FamilyId.Value;

            var profile = await _familyProfileRepository
                .GetByFamilyIdAsync(familyId);

            if (profile == null)
                return NotFound("Household profile not found.");

            var householdPlan = await _householdPlanRepository
                .GetByFamilyIdAsync(familyId);

            if (householdPlan == null)
                return NotFound("Household plan not found.");

            var approvedPlan = await _approvedPlanRepository
                .GetByFamilyIdAsync(familyId);

            // Serialize only the fields AI needs, avoiding navigation-property cycles.
            var currentBlueprint = approvedPlan == null
                ? "No approved household blueprint exists yet."
                : System.Text.Json.JsonSerializer.Serialize(
                            approvedPlan.Items.Where(item => item.IsActive)
                            .Select(item => new
                    {
                        item.Id,
                        item.Title,
                        item.Description,
                        Category = item.Category.ToString(),
                        Recurrence = item.Recurrence.ToString(),
                        item.StartDate,
                        item.SuggestedAssigneeRole,
                        item.EstimatedMinutes,
                        item.MentalLoadEstimate
                    }));

            var changeInstructions =
     string.IsNullOrWhiteSpace(request?.ChangeInstructions)
         ? null
         : request.ChangeInstructions.Trim();

            var suggestions = await _planningService.RefineHouseholdPlanAsync(
                profile.HouseholdDescription,
                householdPlan.PlanDescription,
                currentBlueprint,
                new List<SuggestedTask>(),
                changeInstructions);


            var changedSuggestions = FilterChangedSuggestions(
                suggestions,
                approvedPlan);

            return Ok(changedSuggestions);
        }



        [HttpPost("refine-suggestions")]
        public async Task<IActionResult> RefineSuggestions(
            [FromBody] RefineSuggestionsRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userRepository.GetAsync(Guid.Parse(userId));

            if (user?.FamilyId == null)
                return BadRequest("User has no family.");

            if (request == null ||
                request.Suggestions == null ||
                request.Suggestions.Count == 0)
            {
                return BadRequest("No suggestions were provided.");
            }

            if (string.IsNullOrWhiteSpace(request.RefinementInstructions))
                return BadRequest("Refinement instructions are required.");

            var familyId = user.FamilyId.Value;

            var profile = await _familyProfileRepository
                .GetByFamilyIdAsync(familyId);

            if (profile == null)
                return NotFound("Household profile not found.");

            var householdPlan = await _householdPlanRepository
                .GetByFamilyIdAsync(familyId);

            if (householdPlan == null)
                return NotFound("Household plan not found.");

            var approvedPlan = await _approvedPlanRepository
                .GetByFamilyIdAsync(familyId);

            var currentBlueprint = approvedPlan == null
                ? "No approved household blueprint exists yet."
                : System.Text.Json.JsonSerializer.Serialize(
                    approvedPlan.Items
                        .Where(item => item.IsActive)
                        .Select(item => new
                        {
                            item.Id,
                            item.Title,
                            item.Description,
                            Category = item.Category.ToString(),
                            Recurrence = item.Recurrence.ToString(),
                            item.StartDate,
                            item.SuggestedAssigneeRole,
                            item.EstimatedMinutes,
                            item.MentalLoadEstimate
                        }));

            var refinedSuggestions =
                await _planningService.RefineHouseholdPlanAsync(
                    profile.HouseholdDescription,
                    householdPlan.PlanDescription,
                    currentBlueprint,
                    request.Suggestions,
                    request.RefinementInstructions.Trim());

            var changedSuggestions = FilterChangedSuggestions(
                refinedSuggestions,
                approvedPlan);

            return Ok(changedSuggestions);
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
                await _planningService
                    .GenerateEventPlanAsync(request.Prompt);

            return Ok(suggestions);
        }

        [HttpPost("generate-daily-plan")]
        public async Task<IActionResult> GenerateDailyPlan(
    [FromBody] GenerateDailyPlanRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family.");

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Please describe what's happening today.");
            }

            var planDate = (request.PlanDate ?? DateTime.UtcNow).Date;

            var existingTasks = await _taskRepository.GetByFamilyIdAsync(
                currentUser.FamilyId.Value,
                planDate,
                planDate);

            var dailyPrompt = BuildDailyPlanPrompt(
                request.Prompt.Trim(),
                planDate,
                existingTasks);

            var suggestions = await _planningService
                .GenerateDailyPlanAsync(dailyPrompt);

            var existingTaskTitles = existingTasks
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .Select(x => x.Title.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var dedupedSuggestions = suggestions
                .Where(x => !existingTaskTitles.Contains(x.Title?.Trim() ?? string.Empty))
                .ToList();

            return Ok(dedupedSuggestions);
        }

        [HttpPost("generate-weekly-plan")]
        public async Task<IActionResult> GenerateWeeklyPlan(
     [FromBody] GenerateWeeklyPlanRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family.");

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(
                    "Please describe what's happening this week.");
            }

            var weekStartDate = (request.WeekStartDate ?? DateTime.UtcNow).Date;
            var weekEndDate = weekStartDate.AddDays(6);

            var existingTasks = await _taskRepository.GetByFamilyIdAsync(
                currentUser.FamilyId.Value,
                weekStartDate,
                weekEndDate);

            var weeklyPrompt = BuildWeeklyPlanPrompt(
                request.Prompt.Trim(),
                weekStartDate,
                weekEndDate,
                existingTasks);

            var suggestions = await _planningService
                .GenerateWeeklyPlanAsync(weeklyPrompt);

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

            var suggestions = await _planningService
                .GenerateMonthlyPlanAsync(request.Prompt);

            return Ok(suggestions);
        }





        [HttpPost("approve-suggestions")]
        public async Task<IActionResult> ApproveSuggestions(
            [FromBody] ApproveSuggestionsRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(Guid.Parse(userId));

            if (currentUser?.FamilyId == null)
                return BadRequest("User has no family.");

            if (request?.Suggestions == null || request.Suggestions.Count == 0)
                return BadRequest("No suggestions were provided.");

            var familyId = currentUser.FamilyId.Value;

            // Load the existing approved blueprint, if any.
            var approvedPlan =
                await _approvedPlanRepository.GetByFamilyIdAsync(familyId);

            var isNewPlan = approvedPlan == null;

            if (approvedPlan == null)
            {
                approvedPlan = new ApprovedPlan
                {
                    Id = Guid.NewGuid(),
                    FamilyId = familyId,
                    Title = "Household Responsibility Plan",
                    Description =
                        "Approved household responsibilities generated " +
                        "from the household planning process.",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                };
            }

            if (!isNewPlan)
            {
                await _approvedPlanRepository.CheckPlanItemsAsync(approvedPlan);
            }

            static string BuildDedupKey(
                string? title,
                TaskCategory category,
                RecurrenceType recurrence)
            {
                var normalizedTitle =
                    string.IsNullOrWhiteSpace(title)
                        ? string.Empty
                        : title.Trim().ToLowerInvariant();

                return $"{normalizedTitle}|{category}|{recurrence}";
            }

            var existingItems = approvedPlan.Items
                .Where(x => x.IsActive)
                .ToDictionary(x => x.Id);

            var existingItemsByKey = approvedPlan.Items
                .Where(x => x.IsActive)
                .GroupBy(x => BuildDedupKey(x.Title, x.Category, x.Recurrence))
                .ToDictionary(x => x.Key, x => x.First());

            var processedItemIds = new HashSet<Guid>();
            var newItems = new List<ApprovedPlanItem>();

            foreach (var suggestion in request.Suggestions)
            {
                var recurrence = suggestion.Recurrence switch
                {
                    "Daily" => RecurrenceType.Daily,
                    "Weekly" => RecurrenceType.Weekly,
                    "Monthly" => RecurrenceType.Monthly,
                    _ => RecurrenceType.None
                };

                var category = Enum.TryParse<TaskCategory>(
                    suggestion.Category, true, out var parsedCategory)
                        ? parsedCategory
                        : TaskCategory.Other;

                ApprovedPlanItem item;

                if (suggestion.ApprovedPlanItemId.HasValue)
                {
                    if (!existingItems.TryGetValue(
                            suggestion.ApprovedPlanItemId.Value, out item!))
                    {
                        return BadRequest(
                            "A suggestion references an item that does not belong " +
                            "to the family's current approved blueprint.");
                    }
                }
                else
                {
                    var dedupKey = BuildDedupKey(
                        suggestion.Title,
                        category,
                        recurrence);

                    if (!existingItemsByKey.TryGetValue(dedupKey, out item!))
                    {
                        item = new ApprovedPlanItem
                        {
                            Id = Guid.NewGuid(),
                            ApprovedPlanId = approvedPlan.Id
                        };

                        approvedPlan.Items.Add(item);
                        newItems.Add(item);
                        existingItemsByKey[dedupKey] = item;
                    }
                }

                if (!processedItemIds.Add(item.Id))
                {
                    return BadRequest(
                        "Duplicate suggestions were provided for the same responsibility.");
                }

                // Update the blueprint item.
                item.Title = suggestion.Title;
                item.Description = suggestion.Description;
                item.SuggestedAssigneeRole = suggestion.SuggestedAssigneeRole;
                item.StartDate = suggestion.StartDate;
                item.Priority = suggestion.Priority;
                item.Category = category;
                item.EstimatedMinutes = suggestion.EstimatedMinutes;
                item.MentalLoadEstimate = suggestion.EmotionalLoad;
                item.Recurrence = recurrence;
                item.Tags = new List<string> { suggestion.Category };
            }

            approvedPlan.UpdatedOn = DateTime.UtcNow;

            if (isNewPlan)
            {
                await _approvedPlanRepository.AddAsync(approvedPlan);
            }
            else
            {
                // Existing plan and items are tracked by EF Core.
                // SaveChangesAsync persists their modifications.
                await _approvedPlanRepository.UpdateAsync(approvedPlan,newItems);

            }

            // Create tasks only for newly added blueprint items.
            foreach (var item in newItems)
            {
                var task = new TaskItem
                {
                    Title = item.Title,
                    Description = item.Description,
                    CreatedById = currentUser.Id,
                    DueDate = item.StartDate,
                    Priority = item.Priority,
                    Status = TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    FamilyId = familyId,
                    ApprovedPlanItemId = item.Id,
                    Category = item.Category,
                    EstimatedMinutes = item.EstimatedMinutes,
                    MentalLoadEstimate = item.MentalLoadEstimate,
                    Recurrence = item.Recurrence,
                    Tags = new List<string>(item.Tags)
                };

                await _taskRepository.AddAsync(task);
            }

            return Ok(new
            {
                ApprovedPlanId = approvedPlan.Id,
                Message = isNewPlan
                    ? "Initial approved plan saved."
                    : "Existing approved plan updated."
            });
        }



        [HttpPost("approve-adhoc-suggestions")]
        public async Task<IActionResult> ApproveAdhocSuggestions(
            [FromBody] ApproveSuggestionsRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(Guid.Parse(userId));

            if (currentUser?.FamilyId == null)
                return BadRequest("User has no family.");

            if (request?.Suggestions == null || request.Suggestions.Count == 0)
                return BadRequest("No suggestions were provided.");

            var createdCount = 0;
            var rescheduledCount = 0;
            var processedTaskIds = new HashSet<Guid>();
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var suggestion in request.Suggestions)
            {
                var dueDate = (suggestion.StartDate ?? DateTime.UtcNow).Date;

                if (suggestion.TaskId.HasValue)
                {
                    var taskId = suggestion.TaskId.Value;

                    if (!processedTaskIds.Add(taskId))
                    {
                        continue;
                    }

                    var existingTask = await _taskRepository.GetAsync(taskId);

                    if (existingTask == null)
                    {
                        return BadRequest(
                            $"A suggestion references a task that does not exist: {taskId}");
                    }

                    if (existingTask.FamilyId != currentUser.FamilyId.Value)
                    {
                        return BadRequest(
                            $"A suggestion references a task outside the user's family: {taskId}");
                    }

                    existingTask.DueDate = dueDate;
                    await _taskRepository.UpdateAsync(existingTask);
                    rescheduledCount++;
                    continue;
                }

                var dedupKey = $"{Normalize(suggestion.Title)}|{dueDate:yyyy-MM-dd}";

                if (!processedKeys.Add(dedupKey))
                {
                    continue;
                }

                var task = new TaskItem
                {
                    Title = suggestion.Title,
                    Description = suggestion.Description,
                    CreatedById = currentUser.Id,
                    DueDate = dueDate,
                    Priority = suggestion.Priority,
                    Status = TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    FamilyId = currentUser.FamilyId.Value,
                    ApprovedPlanItemId = null,
                    Category = ParseCategory(suggestion.Category),
                    EstimatedMinutes = suggestion.EstimatedMinutes,
                    MentalLoadEstimate = suggestion.EmotionalLoad,
                    Recurrence = RecurrenceType.None,
                    Tags = string.IsNullOrWhiteSpace(suggestion.Category)
                        ? new List<string>()
                        : new List<string> { suggestion.Category }
                };

                await _taskRepository.AddAsync(task);
                createdCount++;
            }

            return Ok(new
            {
                CreatedTaskCount = createdCount,
                RescheduledTaskCount = rescheduledCount,
                Message = "Adhoc suggestions applied."
            });
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

                        OccurredAt = DateTime.UtcNow.Date
                    })
                    .ToList()
            };

            await _reflectionRepository.AddAsync(
                reflection);

            return Ok(analysis);
        }



        [HttpGet("approved-plan")]
        public async Task<IActionResult> GetApprovedPlan()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(
                Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family.");

            var approvedPlan =
                await _approvedPlanRepository.GetByFamilyIdAsync(
                    currentUser.FamilyId.Value);

            if (approvedPlan == null)
                return NotFound("No approved plan found.");

            var response = new ApprovedPlanResponseDto
            {
                Id = approvedPlan.Id,
                Title = approvedPlan.Title,
                Description = approvedPlan.Description,

                Items = approvedPlan.Items
                        .Where(item => item.IsActive)
                        .Select(item =>
                    new ApprovedPlanItemResponseDto
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Description = item.Description,
                        Category = item.Category.ToString(),
                        Recurrence = item.Recurrence.ToString(),
                        StartDate = item.StartDate,
                        SuggestedAssigneeRole = item.SuggestedAssigneeRole,
                        EstimatedMinutes = item.EstimatedMinutes,
                        MentalLoadEstimate = item.MentalLoadEstimate
                    }).ToList()
            };

            return Ok(response);
        }

        
[HttpDelete("approved-plan/items/{itemId:guid}")]
public async Task<IActionResult> DeleteApprovedPlanItem(Guid itemId)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(
                Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family.");

            var approvedPlan =
                await _approvedPlanRepository.GetByFamilyIdAsync(
                    currentUser.FamilyId.Value);

            if (approvedPlan == null)
                return NotFound("No approved plan found.");

            var item = approvedPlan.Items
                .FirstOrDefault(x => x.Id == itemId);

            if (item == null || !item.IsActive)
                return NotFound("Active plan item not found.");

            // Soft-delete: preserve the record and linked task history.
            item.IsActive = false;
            approvedPlan.UpdatedOn = DateTime.UtcNow;

            await _approvedPlanRepository.UpdateAsync(
                approvedPlan,
                Array.Empty<ApprovedPlanItem>());

            return NoContent();
        }



        [HttpPut("approved-plan/items/{itemId:guid}")]
        public async Task<IActionResult> UpdateApprovedPlanItem(
            Guid itemId,
            [FromBody] UpdateApprovedPlanItemRequest request)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUser = await _userRepository.GetAsync(Guid.Parse(userId));

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.FamilyId == null)
                return BadRequest("User has no family.");

            if (request == null || string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Title is required.");

            if (request.EstimatedMinutes < 0)
                return BadRequest("Estimated minutes cannot be negative.");

            var approvedPlan = await _approvedPlanRepository.GetByFamilyIdAsync(
                currentUser.FamilyId.Value);

            if (approvedPlan == null)
                return NotFound("No approved plan found.");

            var item = approvedPlan.Items.FirstOrDefault(
                x => x.Id == itemId && x.IsActive);

            if (item == null)
                return NotFound("Active plan item not found.");

            item.Title = request.Title.Trim();
            item.Description = request.Description;
            item.Category = request.Category;
            item.Recurrence = request.Recurrence;
            item.EstimatedMinutes = request.EstimatedMinutes;
            item.SuggestedAssigneeRole = request.SuggestedAssigneeRole;
            approvedPlan.UpdatedOn = DateTime.UtcNow;

            await _approvedPlanRepository.UpdateAsync(
                approvedPlan,
                Array.Empty<ApprovedPlanItem>());

            return NoContent();
        }

        private static List<SuggestedTask> FilterChangedSuggestions(
            IEnumerable<SuggestedTask> suggestions,
            ApprovedPlan? approvedPlan)
        {
            if (approvedPlan == null)
            {
                return suggestions.ToList();
            }

            var activeItems = approvedPlan.Items
                .Where(x => x.IsActive)
                .ToList();

            var existingById = activeItems
                .ToDictionary(x => x.Id);

            var existingByKey = activeItems
                .GroupBy(x => BuildSuggestionKey(
                    x.Title,
                    x.Category,
                    x.Recurrence))
                .ToDictionary(x => x.Key, x => x.First());

            var changed = new List<SuggestedTask>();

            foreach (var suggestion in suggestions)
            {
                var recurrence = ParseRecurrence(suggestion.Recurrence);
                var category = ParseCategory(suggestion.Category);

                ApprovedPlanItem? baseline = null;

                if (suggestion.ApprovedPlanItemId.HasValue)
                {
                    existingById.TryGetValue(
                        suggestion.ApprovedPlanItemId.Value,
                        out baseline);
                }
                else
                {
                    existingByKey.TryGetValue(
                        BuildSuggestionKey(
                            suggestion.Title,
                            category,
                            recurrence),
                        out baseline);
                }

                if (baseline == null)
                {
                    changed.Add(suggestion);
                    continue;
                }

                if (HasMeaningfulChanges(suggestion, baseline, category, recurrence))
                {
                    changed.Add(suggestion);
                }
            }

            return changed;
        }

        private static bool HasMeaningfulChanges(
            SuggestedTask suggestion,
            ApprovedPlanItem baseline,
            TaskCategory category,
            RecurrenceType recurrence)
        {
            return !string.Equals(
                       Normalize(suggestion.Title),
                       Normalize(baseline.Title),
                       StringComparison.OrdinalIgnoreCase)
                   || !string.Equals(
                       Normalize(suggestion.Description),
                       Normalize(baseline.Description),
                       StringComparison.OrdinalIgnoreCase)
                   || !string.Equals(
                       Normalize(suggestion.SuggestedAssigneeRole),
                       Normalize(baseline.SuggestedAssigneeRole),
                       StringComparison.OrdinalIgnoreCase)
                   || suggestion.StartDate != baseline.StartDate
                   || suggestion.EstimatedMinutes != baseline.EstimatedMinutes
                   || suggestion.EmotionalLoad != baseline.MentalLoadEstimate
                   || suggestion.Priority != baseline.Priority
                   || category != baseline.Category
                   || recurrence != baseline.Recurrence;
        }

        private static string BuildSuggestionKey(
            string? title,
            TaskCategory category,
            RecurrenceType recurrence)
        {
            return $"{Normalize(title)}|{category}|{recurrence}";
        }

        private static TaskCategory ParseCategory(string? category)
        {
            return Enum.TryParse<TaskCategory>(
                category,
                true,
                out var parsedCategory)
                ? parsedCategory
                : TaskCategory.Other;
        }

        private static RecurrenceType ParseRecurrence(string? recurrence)
        {
            return recurrence?.Trim().ToLowerInvariant() switch
            {
                "daily" => RecurrenceType.Daily,
                "weekly" => RecurrenceType.Weekly,
                "monthly" => RecurrenceType.Monthly,
                _ => RecurrenceType.None
            };
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

        private static string BuildDailyPlanPrompt(
            string userPrompt,
            DateTime planDate,
            IEnumerable<TaskItem> existingTasks)
        {
            var tasksForPrompt = existingTasks
                .OrderBy(x => x.DueDate ?? planDate)
                .ThenBy(x => x.Title)
                .ToList();

            var taskContext = tasksForPrompt.Count == 0
                ? "No tasks exist yet for this date."
                : string.Join(
                    Environment.NewLine,
                    tasksForPrompt.Select(task =>
                        $"- {task.Title} | Status: {task.Status} | Category: {task.Category} | Priority: {task.Priority}"));

            return $$"""
Daily Planning Date: {{planDate:yyyy-MM-dd}}

Existing Tasks For This Date:
{{taskContext}}

Ad hoc request from user:
{{userPrompt}}

Generate only additional ad hoc tasks for this date.
Do not repeat or rename existing tasks.
Prefer one-time tasks (Recurrence should be None unless user explicitly asks for a recurring item).
""";
        }

        private static string BuildWeeklyPlanPrompt(
            string userPrompt,
            DateTime weekStartDate,
            DateTime weekEndDate,
            IEnumerable<TaskItem> existingTasks)
        {
            var tasksForPrompt = existingTasks
                .OrderBy(x => x.DueDate ?? weekStartDate)
                .ThenBy(x => x.Title)
                .ToList();

            var taskContext = tasksForPrompt.Count == 0
                ? "No existing tasks found for this week."
                : string.Join(
                    Environment.NewLine,
                    tasksForPrompt.Select(task =>
                        $"- TaskId: {task.Id} | Title: {task.Title} | DueDate: {(task.DueDate.HasValue ? task.DueDate.Value.ToString("yyyy-MM-dd") : "none")} | Status: {task.Status} | Priority: {task.Priority} | Category: {task.Category} | Recurrence: {task.Recurrence}"));

            return $$"""
Weekly Planning Range: {{weekStartDate:yyyy-MM-dd}} to {{weekEndDate:yyyy-MM-dd}}

Existing Tasks In This Week:
{{taskContext}}

User request:
{{userPrompt}}

Return a task suggestion list for this week where:
- New tasks can be added.
- Existing tasks can be rescheduled only when the user explicitly asks to move/reschedule/postpone/shift an existing task.
- For explicit reschedule intent, match the correct existing task from the list and return that exact TaskId with a new StartDate.
- For existing tasks being rescheduled, keep all other fields aligned with the existing task and only change the date.
- For new tasks, TaskId must be null.

Strict decision rules:
- If intent is unclear, default to creating a new task (TaskId must be null).
- Do not guess TaskId for ambiguous text.
- Never return TaskId for brand-new tasks.

Examples:
- "Move laundry to Friday" => reschedule existing "laundry" task with TaskId and new StartDate.
- "Add meal prep for Thursday" => new task with TaskId null.
- "Week looks busy" => suggest helpful new tasks only; no reschedule unless explicitly requested.
""";
        }



    }
}
