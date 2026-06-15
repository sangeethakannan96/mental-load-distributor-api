namespace MentalLoadDistributor.DTO.User
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // Optional
        public Guid FamilyId { get; set; }
    }
}
