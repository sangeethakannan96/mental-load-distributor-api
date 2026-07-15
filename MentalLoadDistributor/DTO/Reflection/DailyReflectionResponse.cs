namespace MentalLoadDistributor.DTO.Reflection
{
    public class DailyReflectionResponse
    {
        public Guid Id { get; set; }

        public DateTime ReflectionDate { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public List<ActivityLogResponse> Activities { get; set; } = new();
    }
}
