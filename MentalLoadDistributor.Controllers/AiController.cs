using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AiRequest req)
        {
            var resp = await _aiService.AskAsync(req.Input);
            return Ok(resp);
        }
    }
}
