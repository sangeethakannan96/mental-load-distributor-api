    using System;
    using System.Collections.Generic;
using System.Text.Json.Serialization;

    namespace MentalLoadDistributor.Core.Models
    {
        public enum ParentRole
        {
            Mom,
            Dad,
            Other
        }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }

        public ParentRole Role { get; set; } = ParentRole.Other;
        public int AvailabilityScore { get; set; } = 100;

        // ✅ REQUIRED (fix this)
        public Guid? FamilyId { get; set; }

        [JsonIgnore]
        public Family Family { get; set; }

        public List<string> Skills { get; set; } = new();
    }
}