using MentalLoadDistributor.Core.Domain.Models.AI;
using MentalLoadDistributor.Core.Ports;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentalLoadDistributor.Infrastructure.Services
{

    public class SuggestedTaskResponse
    {
        public List<SuggestedTask> Tasks { get; set; } = new();
    }

    public class AiPlanningService : IPlanningService
    {
        private readonly IAiService _aiService;

        public AiPlanningService(IAiService aiService)
        {
            _aiService = aiService;
        }

        public async Task<List<SuggestedTask>> GenerateHouseholdPlanAsync(
     string householdContext,
     string householdPlan)
        {
            var userInput = $$"""
Household Context:
{{householdContext}}

Current Household Plan:
{{householdPlan}}
""";

            return await GenerateTasksAsync(userInput);
        }

        public async Task<List<SuggestedTask>> GenerateEventPlanAsync(
            string prompt)
        {
            return await GenerateTasksAsync(prompt);
        }

        public async Task<List<SuggestedTask>> GenerateDailyPlanAsync(
            string prompt)
        {
            return await GenerateTasksAsync(prompt);
        }

        public async Task<List<SuggestedTask>> GenerateWeeklyPlanAsync(
            string prompt)
        {
            return await GenerateTasksAsync(prompt);
        }

        public async Task<List<SuggestedTask>> GenerateMonthlyPlanAsync(
            string prompt)
        {
            return await GenerateTasksAsync(prompt);
        }

        private async Task<List<SuggestedTask>> GenerateTasksAsync(
            string userInput)
        {
            var prompt = $$"""
    You are the AI planning assistant for MentalLoadDistributor.

    Convert the user's household request into a list of suggested tasks.

    User request:
    {{userInput}}

    Rules:
   ```text
- Identify the natural recurrence of each ongoing household responsibility.
- Use "Daily" for responsibilities that normally happen every day.
- Use "Weekly" for responsibilities that normally happen every week.
- Use "Monthly" for responsibilities that normally happen every month.
- Use "None" only for genuinely one-time or occasional activities
  that do not follow a regular schedule.
- Do not invent recurring responsibilities when the household context
  does not support them.
- Distinguish ongoing household routines from one-time planning,
  setup, review, or coordination activities.
""";

            var response = await _aiService.AskStructuredAsync(
     prompt,
     GetSuggestedTaskSchema(),
     "SuggestedTaskList");

            Console.WriteLine("----- AI RESPONSE START -----");
            Console.WriteLine(response);
            Console.WriteLine("----- AI RESPONSE END -----");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(
                new JsonStringEnumConverter());

            var result =
    JsonSerializer.Deserialize<SuggestedTaskResponse>(
        response,
        options);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "AI returned an empty or invalid task plan.");
            }

            return result.Tasks;





        }

        private object GetSuggestedTaskSchema()
        {
            return new
            {
                type = "object",

                properties = new
                {
                    tasks = new
                    {
                        type = "array",

                        items = new
                        {
                            type = "object",

                            properties = new
                            {
                                title = new
                                {
                                    type = "string"
                                },

                                description = new
                                {
                                    type = "string"
                                },

                                category = new
                                {
                                    type = "string"
                                },

                                recurrence = new
                                {
                                    type = "string"
                                },

                                startDate = new
                                {
                                    type = new[] { "string", "null" }
                                },

                                suggestedAssigneeRole = new
                                {
                                    type = "string"
                                },

                                emotionalLoad = new
                                {
                                    type = "integer"
                                },

                                estimatedMinutes = new
                                {
                                    type = "integer"
                                },

                                priority = new
                                {
                                    type = "integer",
                                    @enum = new[] { 0, 1, 2 }
                                }
                            },

                            required = new[]
                            {
                        "title",
                        "description",
                        "category",
                        "recurrence",
                        "startDate",
                        "suggestedAssigneeRole",
                        "emotionalLoad",
                        "estimatedMinutes",
                        "priority"
                    },

                            additionalProperties = false
                        }
                    }
                },

                required = new[]
                {
            "tasks"
        },

                additionalProperties = false
            };
        }





    
public async Task<List<SuggestedTask>> RefineHouseholdPlanAsync(
    string householdDescription,
    string householdPlan,
    string currentApprovedBlueprint,
    List<SuggestedTask> currentSuggestions,
    string? refinementInstructions)
        {
            var suggestionsJson = JsonSerializer.Serialize(
                currentSuggestions,
                new JsonSerializerOptions { WriteIndented = true });

            var instructions = string.IsNullOrWhiteSpace(refinementInstructions)
                ? "No additional change instructions were provided."
                : refinementInstructions.Trim();


            var isInitialGeneration =
                string.IsNullOrWhiteSpace(currentApprovedBlueprint) ||
                currentApprovedBlueprint.Contains(
                    "No approved household blueprint exists yet.",
                    StringComparison.OrdinalIgnoreCase);

            var planningMode = isInitialGeneration
                ? """
      INITIAL GENERATION:
      No approved household blueprint exists.
      Create an appropriate initial set of household responsibilities
      based on the household context and current household plan.
      """
                : """
      REFINEMENT:
      An approved household blueprint already exists.
      Use it as the baseline, preserve unaffected responsibilities,
      and apply the user's requested changes.
      """;


            var userInput = $$"""
        Household Context:
        {{householdDescription}}

        Current Household Plan:
        {{householdPlan}}

        Existing Approved Blueprint (authoritative baseline):
        {{currentApprovedBlueprint}}

        Current Suggestions (including user edits):
        {{suggestionsJson}}

        User's Refinement Instructions:
        {{instructions}}
        """;

            var prompt = $$"""
        You are the AI planning assistant for MentalLoadDistributor.

        {{planningMode}}

        Revise the household responsibility suggestions using the
        household information, approved blueprint, current suggestions,
        and user's instructions supplied below.

        Rules:
        - Treat the existing approved blueprint as the baseline.
        - Preserve existing responsibilities that do not need to change.
        - Apply the user's requested changes where appropriate.
        - Do not discard unaffected responsibilities or invent unsupported
          family members, skills, or household circumstances.
        - Use current suggestions as the starting point for refinement.
        - Return the complete revised set of suggestions, not only the changes.
        - Keep titles, categories, assignee roles, recurrence, priorities,
          estimated minutes, and emotional load consistent with the input
          and requested changes.
        - Use Daily, Weekly, or Monthly for naturally recurring
          responsibilities when supported by the household information.
        - Use None for genuinely one-time or occasional activities.
        - Do not invent dates when none are known; use null.
        - Do not save or approve anything. Return suggestions for user review.
        - Return only the structured response matching the supplied schema.
        - For an existing blueprint item, return its exact ID as
          approvedPlanItemId when retaining or modifying that item.
        - For a genuinely new responsibility, return null for approvedPlanItemId.
        - Never invent an ID or assign an ID to a different responsibility.
        - For initial generation, approvedPlanItemId must be null.

        Planning information:
        {{userInput}}
        """;

            var response = await _aiService.AskStructuredAsync(
                prompt,
                GetSuggestedTaskSchema(),
                "SuggestedTaskList");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var result = JsonSerializer.Deserialize<SuggestedTaskResponse>(
                response,
                options);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "AI returned an empty or invalid refined household plan.");
            }

            return result.Tasks;
        }
    }
    }