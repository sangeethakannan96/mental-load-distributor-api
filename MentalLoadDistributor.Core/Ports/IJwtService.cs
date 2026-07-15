using MentalLoadDistributor.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
