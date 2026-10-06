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
            string householdDescription)
        {
            return await GenerateTasksAsync(householdDescription);
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
    - Break the request into separate actionable tasks.
    - Preserve the person responsible for each task when mentioned.
    - Use reasonable estimates for emotionalLoad and estimatedMinutes.
    - If no recurrence is mentioned, use "None".
    - If a date or time is not known, use null.
    - Do not add explanations outside the structured response.
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
    }
}