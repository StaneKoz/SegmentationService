using Microsoft.EntityFrameworkCore;
using SegmentationService.Data;

namespace SegmentationService.Services
{
    public class SegmentDeletionService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SegmentDeletionService> _logger;

        public SegmentDeletionService(AppDbContext db, ILogger<SegmentDeletionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task DeleteSegmentWithRelationsAsync(int segmentId, int batchSize = 50_000)
        {
            try
            {
                // 1. Удаляем связи пакетами
                int deletedRelations;
                do
                {
                    deletedRelations = await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"DELETE FROM ""UserSegments"" 
                       WHERE ""SegmentsId"" = {segmentId} 
                       AND ""UsersId"" IN (
                           SELECT ""UsersId"" 
                           FROM ""UserSegments"" 
                           WHERE ""SegmentsId"" = {segmentId} 
                           LIMIT {batchSize}
                       )");

                    _logger.LogInformation($"Удалено связей: {deletedRelations}");
                    await Task.Delay(500); // Даём БД "передохнуть"
                }
                while (deletedRelations > 0);

                // 2. Удаляем сам сегмент
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"DELETE FROM ""Segments"" WHERE ""Id"" = {segmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении сегмента {segmentId}");
                throw;
            }
        }
    }
}
