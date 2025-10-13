using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.Database.Models;

namespace GameStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLibraryController : ControllerBase
    {
        private readonly GameStoreContext _context;

        public UserLibraryController(GameStoreContext context)
        {
            _context = context;
        }

        // GET: api/UserLibrary
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserLibrary>>> GetUserLibraries()
        {
            return await _context.UserLibraries
                .Include(ul => ul.User)
                .Include(ul => ul.Game)
                .ToListAsync();
        }

        // GET: api/UserLibrary/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserLibrary>> GetUserLibrary(int id)
        {
            var userLibrary = await _context.UserLibraries
                .Include(ul => ul.User)
                .Include(ul => ul.Game)
                .FirstOrDefaultAsync(ul => ul.UserLibraryId == id);

            if (userLibrary == null)
            {
                return NotFound();
            }

            return userLibrary;
        }

        // GET: api/UserLibrary/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserLibrary>>> GetUserLibrariesByUser(int userId)
        {
            return await _context.UserLibraries
                .Include(ul => ul.Game)
                .Where(ul => ul.UserId == userId)
                .ToListAsync();
        }

        // POST: api/UserLibrary
        [HttpPost]
        public async Task<ActionResult<UserLibrary>> PostUserLibrary(UserLibrary userLibrary)
        {
            _context.UserLibraries.Add(userLibrary);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserLibrary", new { id = userLibrary.UserLibraryId }, userLibrary);
        }

        // DELETE: api/UserLibrary/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserLibrary(int id)
        {
            var userLibrary = await _context.UserLibraries.FindAsync(id);
            if (userLibrary == null)
            {
                return NotFound();
            }

            _context.UserLibraries.Remove(userLibrary);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserLibraryExists(int id)
        {
            return _context.UserLibraries.Any(e => e.UserLibraryId == id);
        }
    }
}