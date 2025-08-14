using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SegmentationService.Data;
using SegmentationService.Dto;
using SegmentationService.Models;
using SegmentationService.Services;

namespace SegmentationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SegmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IBackgroundJobClient _backgroundJob;
        public SegmentsController(
    AppDbContext db,
    IBackgroundJobClient backgroundJob)
        {
            _db = db;
            _backgroundJob = backgroundJob;
        }


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

            if (segment == null) return NotFound();

            var jobId = _backgroundJob.Enqueue<SegmentDeletionService>(x =>
                x.DeleteSegmentWithRelationsAsync(segment.Id, 1000));

            return Accepted(new
            {
                JobId = jobId,
                Message = $"Начато удаление сегмента {name} и его связей",
                StatusUrl = $"/jobs/{jobId}"
            });
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)] // Скрыть из Swagger
        public async Task<ActionResult<Segment>> GetSegment(int id)
        {
            return await _db.Segments.FindAsync(id);
        }
        /// <summary>
        /// Назначает сегмент определённому проценту пользователей
        /// </summary>
        /// <param name="segmentName"></param>
        /// <param name="percent"></param>
        /// <param name="backgroundJob"></param>
        /// <returns></returns>
        [HttpPost("{segmentName}/assign")]
        public async Task<IActionResult> AssignSegmentToPercentage(string segmentName, [FromBody] int percent, [FromServices] IBackgroundJobClient backgroundJob)
        {
            if (percent < 0 || percent > 100)
            {
                return BadRequest("Процент должен быть в диапазоне 0-100");
            }

            var segment = await _db.Segments.FirstOrDefaultAsync(s => s.Name == segmentName);

            if (segment == null)
            {
                return NotFound($"Сегмент '{segmentName}' не найден");
            }

            string jobId = backgroundJob.Enqueue<SegmentService>(
                x => x.AssignSegmentToPercentageAsync(segment.Id, percent, 1000));

            return Accepted(new
            {
                JobId = jobId,
                Message = $"Сегмент {segmentName} назначается для {percent}% пользователей",
                StatusUrl = $"/hangfire/jobs/details/{jobId}"
            });
        }
    }
}