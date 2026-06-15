using System;
using System.Collections.Generic;
using System.Text;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IAiService
    {
        Task<string> AskAsync(string prompt);

    }
}
