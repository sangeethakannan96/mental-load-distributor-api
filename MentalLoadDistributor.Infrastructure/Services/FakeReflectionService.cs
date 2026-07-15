using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models.AI;
using MentalLoadDistributor.Core.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Infrastructure.Services
{
    public class FakeReflectionService : IReflectionService
    {
        public async Task<ReflectionAnalysisResult> AnalyzeReflectionAsync(
            string content)
        {
            await Task.Delay(500);

            return new ReflectionAnalysisResult
            {
                Summary = "You handled several family responsibilities today, including childcare, planning and household work.",

                Activities = new List<ActivitySuggestion>
            {
                new ActivitySuggestion
                {
                    Title = "Prepared tomorrow's lunch",

                    Description =
                        "Prepared food in advance for the next day.",

                    Category = ActivityCategory.Cooking,

                    EstimatedMinutes = 30,

                    MentalLoadScore = 25
                },

                new ActivitySuggestion
                {
                    Title = "Helped with homework",

                    Description =
                        "Supported a child with school homework.",

                    Category = ActivityCategory.Childcare,

                    EstimatedMinutes = 45,

                    MentalLoadScore = 35
                },

                new ActivitySuggestion
                {
                    Title = "Planned family meals",

                    Description =
                        "Spent time deciding and organizing upcoming family meals.",

                    Category = ActivityCategory.Planning,

                    EstimatedMinutes = 60,

                    MentalLoadScore = 70
                }
            }
            };
        }
    }
}
