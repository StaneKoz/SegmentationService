using Microsoft.EntityFrameworkCore;
using SegmentationService.Data;

namespace SegmentationService.Services
{
    public class SegmentService
    {
        private readonly AppDbContext _db;
        private readonly ILogger _logger;
        public SegmentService(AppDbContext db, ILogger<SegmentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AssignSegmentToPercentageAsync(int segmentId, int percent, int batchSize = 1000)
        {
            var totalUsers = await _db.Users.CountAsync();
            var usersToAssign = (int)(totalUsers * percent / 100.0);

            _logger.LogInformation($"Начинаем назначение сегмента {segmentId} для {usersToAssign} пользователей");

            // 2. Пакетная обработка
            var processed = 0;
            while (processed < usersToAssign)
            {
                var currentBatchSize = Math.Min(batchSize, usersToAssign - processed);

                // 3. Точный SQL-запрос для вашей структуры БД
                await _db.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""UserSegments"" (""UsersId"", ""SegmentsId"")
            SELECT ""Id"", {segmentId}
            FROM (
                SELECT ""Id"", random() as rnd
                FROM ""Users""
                WHERE ""Id"" NOT IN (
                    SELECT ""UsersId"" 
                    FROM ""UserSegments"" 
                    WHERE ""SegmentsId"" = {segmentId}
                )
                ORDER BY rnd
                LIMIT {currentBatchSize}
            ) AS subquery
        ");

                processed += currentBatchSize;
                _logger.LogInformation($"Обработано {processed}/{usersToAssign}");
            }
        }
    }
}
