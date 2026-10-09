using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.AI;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO;
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
        public async Task<IActionResult>
     GenerateHouseholdPlan()
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

            var familyId =
                user.FamilyId.Value;

            var profile =
                await _familyProfileRepository
                    .GetByFamilyIdAsync(
                        familyId);

            if (profile == null)
                return NotFound();

            var householdPlan =
                await _householdPlanRepository
                    .GetByFamilyIdAsync(
                        familyId);

            if (householdPlan == null)
                return NotFound(
                    "Household plan not found");

            var suggestions =
                await _planningService
                    .GenerateHouseholdPlanAsync(
                        profile.HouseholdDescription,
                        householdPlan.PlanDescription);

            return Ok(suggestions);
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

            if (currentUser.FamilyId == null)
                return BadRequest(
                    "User has no family");

            var familyId =
                currentUser.FamilyId.Value;


            // -----------------------------------------
            // 1. Create the Approved Plan
            // -----------------------------------------

            var approvedPlan =
                new ApprovedPlan
                {
                    Id = Guid.NewGuid(),

                    FamilyId = familyId,

                    Title =
                        "Household Responsibility Plan",

                    Description =
                        "Approved household responsibilities " +
                        "generated from the household planning process.",

                    CreatedOn =
                        DateTime.UtcNow,

                    UpdatedOn =
                        DateTime.UtcNow
                };


            // -----------------------------------------
            // 2. Add approved plan items
            // -----------------------------------------

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


                var category =
                    Enum.TryParse<TaskCategory>(
                        suggestion.Category,
                        true,
                        out var parsedCategory)
                        ? parsedCategory
                        : TaskCategory.Other;


                var approvedPlanItem =
                    new ApprovedPlanItem
                    {
                        Id = Guid.NewGuid(),

                        ApprovedPlanId =
                            approvedPlan.Id,

                        Title =
                            suggestion.Title,

                        Description =
                            suggestion.Description,

                        SuggestedAssigneeRole =
                             suggestion.SuggestedAssigneeRole,

                        StartDate =
                             suggestion.StartDate,

                        Priority =
                            suggestion.Priority,

                        Category =
                            category,

                        EstimatedMinutes =
                            suggestion.EstimatedMinutes,

                        MentalLoadEstimate =
                            suggestion.EmotionalLoad,

                        Recurrence =
                            recurrence,

                        Tags =
                            new List<string>
                            {
                        suggestion.Category
                            }
                    };

                approvedPlan.Items.Add(
                    approvedPlanItem);
            }


            // -----------------------------------------
            // 3. Save the Approved Plan
            // -----------------------------------------

            await _approvedPlanRepository
                .AddAsync(approvedPlan);


            // -----------------------------------------
            // 4. Create TaskItems
            // -----------------------------------------

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

                        DueDate =
                            suggestion.StartDate,

                        Priority =
                            suggestion.Priority,

                        Status =
                            TaskStatus.Pending,

                        CreatedAt =
                            DateTime.UtcNow,

                        FamilyId =
                            familyId,

                        Category =
                            Enum.TryParse<TaskCategory>(
                                suggestion.Category,
                                true,
                                out var category)
                                ? category
                                : TaskCategory.Other,

                        EstimatedMinutes =
                            suggestion.EstimatedMinutes,

                        MentalLoadEstimate =
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


    }
}
