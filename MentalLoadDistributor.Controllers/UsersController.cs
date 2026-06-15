using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _store;

        public UsersController(IUserRepository store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _store.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var u = await _store.GetAsync(id);
            if (u == null) return NotFound();
            return Ok(u);
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            await _store.AddAsync(user);
            var created = await _store.GetAsync(user.Id);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, User user)
        {
            if (id != user.Id) return BadRequest();
            var existing = await _store.GetAsync(id);
            if (existing == null) return NotFound();
            await _store.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _store.RemoveAsync(id);
            return NoContent();
        }
    }
}
