using System;
using System.Collections.Generic;

namespace MentalLoadDistributor.Core.Models
{
    public class Family
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public List<User> Members { get; set; } = new();

        public Dictionary<string, string> Preferences { get; set; } = new();
    }
}