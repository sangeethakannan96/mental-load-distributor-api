using MentalLoadDistributor.Core.Ports;
using System;
using System.Collections.Generic;
using System.Text;

namespace MentalLoadDistributor.Infrastructure.Services
{
    public class FakeAiService : IAiService
    {
        public Task<string> AskAsync(string prompt)
        {
            // Simulate AI response
            return Task.FromResult("Suggested: Dad");
        }
    }
}
