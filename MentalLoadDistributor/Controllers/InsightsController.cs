using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Core.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsightsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IInsightsService _insightsService;

    public InsightsController(
        IUserRepository userRepository,
        IInsightsService insightsService)
    {
        _userRepository = userRepository;
        _insightsService = insightsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInsights([FromQuery] string period = "month")
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var currentUser = await _userRepository.GetAsync(Guid.Parse(userId));

        if (currentUser == null)
            return Unauthorized();

        if (currentUser.FamilyId == null)
            return BadRequest("User has no family");

        var insights = await _insightsService.GetInsightsAsync(
            currentUser.FamilyId.Value,
            period);

        return Ok(insights);
    }
}