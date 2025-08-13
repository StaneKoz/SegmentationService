using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SegmentationService.Data;
using SegmentationService.Models;

namespace SegmentationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SegmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        
        public SegmentsController(AppDbContext db) => _db  = db;

        /// <summary>
        /// Возвращает сегменты пользователя по его ID
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Segment>>> GetSegments(int id)
        {
            var user = await _db.Users
                .Include(u => u.Segments)
                .FirstOrDefaultAsync(u => u.Id == id);   
            
            if (user == null)
            {
                return NotFound();
            }

            return user.Segments;
        }
    }
}
