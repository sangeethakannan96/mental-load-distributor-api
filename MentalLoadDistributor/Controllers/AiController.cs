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
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Please describe what's happening today.");
            }

            var suggestions = await _planningService
                .GenerateDailyPlanAsync(request.Prompt);

            return Ok(suggestions);
        }

        [HttpPost("generate-weekly-plan")]
        public async Task<IActionResult> GenerateWeeklyPlan(
     [FromBody] GenerateWeeklyPlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(
                    "Please describe what's happening this week.");
            }

            var suggestions = await _planningService
                .GenerateWeeklyPlanAsync(request.Prompt);

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



    }
}
