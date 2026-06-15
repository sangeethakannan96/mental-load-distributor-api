using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliesController : ControllerBase
    {
        private readonly IFamilyRepository _familyRepository;

        public FamiliesController(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _familyRepository.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var f = await _familyRepository.GetAsync(id);
            if (f == null) return NotFound();
            return Ok(f);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Family family)
        {
            await _familyRepository.AddAsync(family);
            var created = await _familyRepository.GetAsync(family.Id);
            return CreatedAtAction(nameof(Get), new { id = family.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Family family)
        {
            if (id != family.Id) return BadRequest();
            var existing = await _familyRepository.GetAsync(id);
            if (existing == null) return NotFound();
            await _familyRepository.UpdateAsync(family);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _familyRepository.RemoveAsync(id);
            return NoContent();
        }
    }
}
