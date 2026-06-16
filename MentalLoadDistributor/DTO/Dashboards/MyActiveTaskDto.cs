namespace MentalLoadDistributor.DTO.Dashboards
{
    public class MyActiveTaskDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public int Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
