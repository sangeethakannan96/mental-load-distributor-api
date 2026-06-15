using MentalLoadDistributor.DTO.User;

namespace MentalLoadDistributor.DTO.Family
{
    public class FamilyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<UserDto> Members { get; set; } = new();
    }
}
