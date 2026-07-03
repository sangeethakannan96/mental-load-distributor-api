using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Models.AI;
using MentalLoadDistributor.Core.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Infrastructure.Services
{
    public class FakeTaskSuggestionService
    : ITaskSuggestionService
    {
        public Task<List<SuggestedTask>>
            GenerateHouseholdSuggestionsAsync(
                string householdDescription)
        {
            var result =
                new List<SuggestedTask>
                {
                new()
                {
                    Title =
                        "Cook Breakfast",

                    Description =
                        "Prepare breakfast for family",

                    Category =
                        "Cooking",

                    Recurrence =
                        "Daily",

                    SuggestedAssigneeRole =
                        "Mom",

                    EmotionalLoad =
                        20,

                    StartDate = DateTime.UtcNow.Date,

                    EstimatedMinutes = 30,

                    Priority = TaskPriority.Medium
                },

                new()
                {
                    Title =
                        "Laundry",

                    Description =
                        "Wash family clothes",

                    Category =
                        "Cleaning",

                    Recurrence =
                        "Weekly",

                    SuggestedAssigneeRole =
                        "Dad",

                    EmotionalLoad =
                        30,

                    StartDate = DateTime.UtcNow.Date,

                    EstimatedMinutes = 30,

                    Priority = TaskPriority.Medium
                }
                };

            return Task.FromResult(
                result);
        }


        public Task<List<SuggestedTask>> GenerateTaskSuggestionsAsync(string prompt)
        {
            // TODO:
            // Replace this with Azure OpenAI implementation.
            // For now we ignore the prompt and return sample tasks.

            var suggestions = new List<SuggestedTask>
    {
        new SuggestedTask
        {
            Title = "Buy Groceries",
            Description = "Purchase groceries for the upcoming event.",
            Category = "Shopping",
            EmotionalLoad = 20,
            Recurrence = "None",
            SuggestedAssigneeRole = "Dad",
            StartDate = DateTime.UtcNow.Date
        },

        new SuggestedTask
        {
            Title = "Clean Living Room",
            Description = "Vacuum and organize the living room.",
            Category = "Cleaning",
            EmotionalLoad = 40,
            Recurrence = "None",
            SuggestedAssigneeRole = "Mom",
            StartDate = DateTime.UtcNow.Date
        },

        new SuggestedTask
        {
            Title = "Buy Birthday Cake",
            Description = "Order or purchase a birthday cake.",
            Category = "Shopping",
            EmotionalLoad = 10,
            Recurrence = "None",
            SuggestedAssigneeRole = "Dad",
            StartDate = DateTime.UtcNow.Date
        },

        new SuggestedTask
        {
            Title = "Prepare Dinner",
            Description = "Prepare dinner for the family.",
            Category = "Cooking",
            EmotionalLoad = 50,
            Recurrence = "None",
            SuggestedAssigneeRole = "Mom",
            StartDate = DateTime.UtcNow.Date
        }
    };

            return Task.FromResult(suggestions);
        }
    }
}
