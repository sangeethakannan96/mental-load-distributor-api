using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.DTO;
using Microsoft.AspNetCore.Mvc;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("suggest")]
        public async Task<IActionResult> Suggest([FromBody] AiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                return BadRequest("Input is required");

            var result = await _aiService.AskAsync(request.Input);

            return Ok(result);
        }
    }
}
