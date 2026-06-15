using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.DTO.User
{
    public class UpdateUserDto
    {
        public string Name { get; set; }

        public int AvailabilityScore { get; set; }

        public string Email { get; set; }

        public string Role { get; set; } = "Other";

        public List<string> Skills { get; set; } = new();
    }
}
