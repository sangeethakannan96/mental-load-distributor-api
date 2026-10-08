namespace MentalLoadDistributor.Core.Domain.Models
{
    public class HouseholdPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FamilyId { get; set; }

        public Family Family { get; set; } = null!;

        public string PlanDescription { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }
}