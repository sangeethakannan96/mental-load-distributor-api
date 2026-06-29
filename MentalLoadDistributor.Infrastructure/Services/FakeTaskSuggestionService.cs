using MentalLoadDistributor.Core.Models;
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
            GenerateSuggestionsAsync(
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
    }
}
