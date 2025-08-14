using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SegmentationService.Data;
using SegmentationService.Dto;
using SegmentationService.Models;

namespace SegmentationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db) => _db = db;
        /// <summary>
        /// Создаёт пользователя
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  
            }

            var user = new User
            {
                Name = userDto.Name
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                   nameof(GetUser),
                   new { id = user.Id },
                   new UserDto
                   {
                       Id = user.Id,
                       Name = user.Name
                   });
        }

        /// <summary>
        /// Получить пользователя по ID (внутренний метод)
        /// </summary>
        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            return await _db.Users.FindAsync(id);
        }
        /// <summary>
        /// Добавляет пользователя в сегмент
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="segmentName"></param>
        /// <returns></returns>
        [HttpPost("{userId}/segments/{segmentName}")]
        public async Task<ActionResult> AddUserToSegment(int userId, string segmentName)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound($"Пользователь с ID {userId} не найден");
            }

            var segment = await _db.Segments
                .FirstOrDefaultAsync(s => s.Name == segmentName);

            if (segment == null)
            {
                return NotFound($"Сегмент с именем {segmentName} не найден");
            }

            if (user.Segments.Any(s => s.Name == segmentName))
            {
                return BadRequest($"Пользователь уже находится в сегменте '{segmentName}'");
            }

            user.Segments.Add(segment);
            await _db.SaveChangesAsync();

            return Ok($"Пользователь с ID: {user.Id} добавлен в сегмент: {segment.Name}");
        }
        /// <summary>
        /// Возвращает список сегмента пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/segments")]
        public async Task<ActionResult<List<SegmentDto>>> GetUserSegments(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("ID пользователя должен быть положительным числом");
            }

            var user = await _db.Users
                .Include(u => u.Segments)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound($"Пользователь с ID {userId} не найден");
            }

            return user.Segments
                .Select(s => new SegmentDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();
        }
        /// <summary>
        /// Удаляет пользователя из сегмента
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="segmentName"></param>
        /// <returns></returns>
        [HttpDelete("{userId}/segments/{segmentName}")]
        public async Task<IActionResult> RemoveUserFromSegment(int userId, string segmentName)
        {
            // Валидация ID
            if (userId <= 0)
            {
                return BadRequest("ID пользователя должен быть положительным числом");
            }

            var user = await _db.Users
                .Include(u => u.Segments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound($"Пользователь с ID {userId} не найден");
            }

            var segment = user.Segments.FirstOrDefault(s => s.Name == segmentName);
            if (segment == null)
            {
                return NotFound($"Пользователь не состоит в сегменте '{segmentName}'");
            }

            user.Segments.Remove(segment);
            await _db.SaveChangesAsync();

            return NoContent(); 
        }
    }
}
