using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace MentalLoadDistributor.Core.Domain.Enums
    {
        public enum BulkTaskAction
        {
            Complete = 0,
            Cancel = 1,
            Delete = 2,
            Reassign = 3,
            Postpone = 4
        }
    }

