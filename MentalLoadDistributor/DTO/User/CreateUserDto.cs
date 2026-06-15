using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.DTO.User
{
    public class CreateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public int AvailabilityScore { get; set; } = 100;

        public string Role { get; set; } = "Other";

        public List<string> Skills { get; set; } = new();
    }
}
