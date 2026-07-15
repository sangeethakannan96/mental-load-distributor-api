using MentalLoadDistributor.Core.Domain.Models.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IReflectionService
    {
        Task<ReflectionAnalysisResult> AnalyzeReflectionAsync(
            string content);
    }
}
