using System;
using System.Collections.Generic;
using System.Linq;
using MentalLoadDistributor.Core.Models;


namespace MentalLoadDistributor.Core.Services
{
    public class DelegationService
    {
        public Guid? SuggestAssignee(Family family, TaskItem task, List<TaskItem> allTasks)
        {
            if (family == null || task == null || !family.Members.Any())
                return null;

            // 1️⃣ Build candidate list with metrics
            var candidates = family.Members
                .Select(m => new
                {
                    Member = m,

                    // 🔥 Workload = total pending minutes
                    Workload = allTasks
                        .Where(t => t.AssignedToId == m.Id && !t.IsCompleted)
                        .Sum(t => t.EstimatedMinutes),

                    // 🔥 Skill match
                    SkillScore = (task.Tags != null && task.Tags.Any())
                        ? m.Skills.Count(s => task.Tags.Contains(s, StringComparer.OrdinalIgnoreCase))
                        : 0,

                    Availability = m.AvailabilityScore
                })
                .ToList();

            // 2️⃣ Compute average workload
            var avgWorkload = candidates.Any()
                ? candidates.Average(c => c.Workload)
                : 0;

            // 3️⃣ Skill-first boost (if strong match exists)
            if (task.Tags != null && task.Tags.Any())
            {
                var bestSkillMatch = candidates
                    .OrderByDescending(c => c.SkillScore)
                    .ThenBy(c => c.Workload)
                    .FirstOrDefault();

                if (bestSkillMatch != null && bestSkillMatch.SkillScore > 0)
                    return bestSkillMatch.Member.Id;
            }

            // 4️⃣ Emotional load balancing
            if (task.EmotionalLoadEstimate > 50)
            {
                var dad = candidates
                    .Where(c => c.Member.Role == ParentRole.Dad)
                    .OrderBy(c => c.Workload)
                    .FirstOrDefault();

                if (dad != null)
                    return dad.Member.Id;
            }

            // 5️⃣ Smart workload-based ordering
            var ordered = candidates
                .OrderBy(c => c.Workload < avgWorkload ? 0 : 1) // ✅ underloaded first
                .ThenBy(c => c.Workload)                      // ✅ least busy
                .ThenByDescending(c => c.SkillScore)          // ✅ skill match
                .ThenByDescending(c => c.Availability)        // ✅ fallback
                .ToList();

            var best = ordered.FirstOrDefault();

            if (best == null)
                return null;

            // 6️⃣ 🔥 Overload protection
            if (best.Workload > avgWorkload * 1.3)
            {
                var alternative = ordered
                    .FirstOrDefault(c => c.Workload <= avgWorkload);

                if (alternative != null)
                    return alternative.Member.Id;
            }

            // 7️⃣ Final selection
            return best.Member.Id;
        }



        public RecommendationResult ExplainRecommendation(Family family, TaskItem task, List<TaskItem> allTasks)
        {
            if (family == null || task == null || !family.Members.Any())
                return null;

            // 1️⃣ Build candidate list with metrics
            var candidates = family.Members
                .Select(m => new
                {
                    Member = m,

                    // 🔥 Workload = total pending minutes
                    Workload = allTasks
                        .Where(t => t.AssignedToId == m.Id && !t.IsCompleted)
                        .Sum(t => t.EstimatedMinutes),

                    // 🔥 Skill match
                    SkillScore = (task.Tags != null && task.Tags.Any())
                        ? m.Skills.Count(s => task.Tags.Contains(s, StringComparer.OrdinalIgnoreCase))
                        : 0,

                    Availability = m.AvailabilityScore
                })
                .ToList();

            // 2️⃣ Compute average workload
            var avgWorkload = candidates.Any()
                ? candidates.Average(c => c.Workload)
                : 0;

            // 3️⃣ Skill-first boost (if strong match exists)
            if (task.Tags != null && task.Tags.Any())
            {
                var bestSkillMatch = candidates
                    .OrderByDescending(c => c.SkillScore)
                    .ThenBy(c => c.Workload)
                    .FirstOrDefault();

                if (bestSkillMatch != null && bestSkillMatch.SkillScore > 0)
                {


                    var matchingSkills = bestSkillMatch.Member.Skills.Where(s => task.Tags.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList();

                    return new RecommendationResult
                    {
                        UserId = bestSkillMatch.Member.Id,

                        UserName = bestSkillMatch.Member.Name,

                        Reasons = new List<string> {
                                     $"Matches skill(s): {string.Join(", ", matchingSkills)}",

                                     $"Availability Score: {bestSkillMatch.Availability}",

                                     $"Current pending workload: {bestSkillMatch.Workload} mins" }
                    };

                }
            }

            // 4️⃣ Emotional load balancing
            if (task.EmotionalLoadEstimate > 50)
            {
                var dad = candidates
                    .Where(c => c.Member.Role == ParentRole.Dad)
                    .OrderBy(c => c.Workload)
                    .FirstOrDefault();

                if (dad != null)
                {
                    return new RecommendationResult
                    {
                        UserId = dad.Member.Id,
                        UserName = dad.Member.Name,
                        Reasons = new List<string> {
                                    "High emotional load task",

                                    "Assigned to a parent with the lowest workload",

                                    $"Current pending workload: {dad.Workload} mins" }
                    };
                }
            }

            // 5️⃣ Smart workload-based ordering
            var ordered = candidates
                .OrderBy(c => c.Workload < avgWorkload ? 0 : 1) // ✅ underloaded first
                .ThenBy(c => c.Workload)                      // ✅ least busy
                .ThenByDescending(c => c.SkillScore)          // ✅ skill match
                .ThenByDescending(c => c.Availability)        // ✅ fallback
                .ToList();

            var best = ordered.FirstOrDefault();

            if (best == null)
                return null;

            // 6️⃣ 🔥 Overload protection
            if (best.Workload > avgWorkload * 1.3)
            {
                var alternative = ordered
                    .FirstOrDefault(c => c.Workload <= avgWorkload);

                if (alternative != null)
                {
                    return new RecommendationResult
                    {
                        UserId = alternative.Member.Id,
                        UserName = alternative.Member.Name,
                        Reasons = new List<string>
        {
            "Overload protection applied",
            $"Current workload: {alternative.Workload} mins",
            $"Family average workload: {avgWorkload:F0} mins"
        }
                    };
                }
            }

            // 7️⃣ Final selection
            return new RecommendationResult
            {
                UserId = best.Member.Id,
                UserName = best.Member.Name,
                Reasons = new List<string>
{
    "Best workload balance across the family",

    $"Current pending workload: {best.Workload} mins",

    $"Availability Score: {best.Availability}",

    $"Family average workload: {avgWorkload:F0} mins"
}
            };
        }
    }
}