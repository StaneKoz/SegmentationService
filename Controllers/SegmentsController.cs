using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SegmentationService.Data;
using SegmentationService.Dto;
using SegmentationService.Models;

namespace SegmentationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SegmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SegmentsController(AppDbContext db) => _db = db;


        /// <summary>
        /// Возвращает все сегменты
        /// </summary>
        [HttpGet("")]
        public async Task<ActionResult<List<SegmentDto>>> GetAllSegments()
        {
            var segments = await _db.Segments
                .Select(s => new SegmentDto
                {
                    Id = s.Id,
                    Name = s.Name,
                })
                .ToListAsync();

            return segments;
        }
        /// <summary>
        /// Создаёт новый сегмент
        /// </summary>
        /// <param name="segmentDto">Сегмент</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<SegmentDto>> CreateSegment([FromBody] CreateSegmentDto segmentDto)
        {
            if (string.IsNullOrWhiteSpace(segmentDto.Name))
            {
                return BadRequest("Название сегмента обязательно");
            }

            if (await _db.Segments.AnyAsync(s => s.Name == segmentDto.Name))
            {
                return BadRequest("Сегмент с таким именем уже существует");
            }

            var segment = new Segment
            {
                Name = segmentDto.Name
            };

            _db.Segments.Add(segment);

            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSegment),
                new { id = segment.Id },
                new SegmentDto { Id = segment.Id, Name = segment.Name }
                );
        }

        /// <summary>
        /// Удаляет сегмент по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteSegment(string name)
        {
            var segment = await _db.Segments
                .FirstOrDefaultAsync(s => s.Name == name);

            if (segment == null)
            {
                return NotFound();
            }

            _db.Segments.Remove(segment);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)] // Скрыть из Swagger
        public async Task<ActionResult<Segment>> GetSegment(int id)
        {
            return await _db.Segments.FindAsync(id);
        }
    }
}