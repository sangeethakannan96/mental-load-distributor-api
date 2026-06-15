using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.DTO.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int AvailabilityScore { get; set; } = 100;

        public string Email { get; set; }

        public string Role { get; set; } = "Other";

        public List<string> Skills { get; set; } = new();
    }
}
