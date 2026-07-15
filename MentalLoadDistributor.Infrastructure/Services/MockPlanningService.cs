using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.AI;
using MentalLoadDistributor.Core.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Infrastructure.Services
{
    public class MockPlanningService
    : IPlanningService
    {
        public Task<List<SuggestedTask>>
            GenerateHouseholdPlanAsync(
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


        public Task<List<SuggestedTask>> GenerateEventPlanAsync(string prompt)
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


        public async Task<List<SuggestedTask>> GenerateDailyPlanAsync(
    string prompt)
        {
            await Task.Delay(500);

            return new List<SuggestedTask>
    {
        new SuggestedTask
        {
            Title = "Buy groceries",
            Description = "Purchase groceries needed for the family.",
            Category = "Shopping",
            Recurrence = "None",
            SuggestedAssigneeRole = "Dad",
            EmotionalLoad = 20,
            StartDate = DateTime.UtcNow.Date
        },

        new SuggestedTask
        {
            Title = "Prepare dinner",
            Description = "Prepare dinner for the family.",
            Category = "Cooking",
            Recurrence = "None",
            SuggestedAssigneeRole = "Mom",
            EmotionalLoad = 30,
            StartDate = DateTime.UtcNow.Date
        }
    };
        }




        public async Task<List<SuggestedTask>> GenerateWeeklyPlanAsync(
    string prompt)
        {
            await Task.Delay(500);

            return new List<SuggestedTask>
    {
        new SuggestedTask
        {
            Title = "Buy groceries for the week",

            Description =
                "Purchase groceries needed for the family this week.",

            Category = "Shopping",

            Recurrence = "None",

            SuggestedAssigneeRole = "Dad",

            EmotionalLoad = 20,

            StartDate = DateTime.UtcNow.Date
        },

        new SuggestedTask
        {
            Title = "Prepare for swimming lessons",

            Description =
                "Prepare clothes and other items needed for the children's swimming lessons.",

            Category = "Kids",

            Recurrence = "None",

            SuggestedAssigneeRole = "Mom",

            EmotionalLoad = 25,

            StartDate = DateTime.UtcNow.Date.AddDays(2)
        },

        new SuggestedTask
        {
            Title = "Prepare for weekend guests",

            Description =
                "Clean the guest area and prepare anything needed for weekend visitors.",

            Category = "Home",

            Recurrence = "None",

            SuggestedAssigneeRole = "Dad",

            EmotionalLoad = 35,

            StartDate = DateTime.UtcNow.Date.AddDays(5)
        }
    };
        }


        public async Task<List<SuggestedTask>> GenerateMonthlyPlanAsync(
    string prompt)
        {
            await Task.Delay(500);

            return new List<SuggestedTask>
    {
        new SuggestedTask
        {
            Title = "Renew car insurance",

            Description =
                "Complete the car insurance renewal before it expires.",

            Category = "Administration",

            Recurrence = "None",

            SuggestedAssigneeRole = "Dad",

            EmotionalLoad = 30,

            StartDate = DateTime.UtcNow.Date.AddDays(5)
        },

        new SuggestedTask
        {
            Title = "Plan birthday celebration",

            Description =
                "Plan the upcoming family birthday celebration.",

            Category = "Events",

            Recurrence = "None",

            SuggestedAssigneeRole = "Mom",

            EmotionalLoad = 40,

            StartDate = DateTime.UtcNow.Date.AddDays(10)
        },

        new SuggestedTask
        {
            Title = "Prepare for parents' visit",

            Description =
                "Prepare the house and make arrangements for the upcoming family visit.",

            Category = "Family",

            Recurrence = "None",

            SuggestedAssigneeRole = "Dad",

            EmotionalLoad = 35,

            StartDate = DateTime.UtcNow.Date.AddDays(15)
        }
    };
        }
    }
}
