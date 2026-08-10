using MentalLoadDistributor.Core.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MentalLoadDistributor.Core.Domain.Models
{
    public class BulkTaskActionDto
    {
        public List<Guid> TaskIds { get; set; } = new();

        public BulkTaskAction Action { get; set; }

        public Guid? AssignedToId { get; set; }

        public DateTime? DueDate { get; set; }
    }
}