# Segmentation Service

Микросервис для управления пользовательскими сегментами с возможностью массового распределения. Разработан в рамках учебной практики для VK.

## 🔍 О проекте

Сервис решает задачу управления сегментами пользователей для проведения A/B тестирования новых функций:
- Создание/удаление сегментов (например, `MAIL_GPT`, `CLOUD_DISCOUNT_30`)
- Добавление пользователей в сегменты
- **Массовое распределение** сегментов по % пользователей
- Получение активных сегментов пользователя

**Ссылка на документацию API**: [segmentservice.example.com](http://51.250.39.113:5000/swagger/index.html)

## 🌟 Ключевые особенности

### ⚡ Высокопроизводительное распределение сегментов
Реализована пакетная обработка для работы с большими объемами данных (1M+ пользователей):

```csharp
// SegmentService.cs
public async Task AssignSegmentToPercentageAsync(int segmentId, int percent, int batchSize = 1000)
{
    var totalUsers = await _db.Users.CountAsync();
    var usersToAssign = (int)(totalUsers * percent / 100.0);
    
    await _db.Database.ExecuteSqlRawAsync($@"
        INSERT INTO ""UserSegments"" (""UsersId"", ""SegmentsId"")
        SELECT ""Id"", {segmentId}
        FROM (
            SELECT ""Id"", random() as rnd
            FROM ""Users""
            WHERE ""Id"" NOT IN (
                SELECT ""UsersId"" FROM ""UserSegments"" 
                WHERE ""SegmentsId"" = {segmentId}
            )
            ORDER BY rnd
            LIMIT {batchSize}
        ) AS subquery
    ");
}

## Оптимизации

- **Пакетная обработка** (batchSize=1000)
- **Нативный SQL** для массовых вставок
- **Случайная выборка** через `ORDER BY random()`
- **Фоновое выполнение** через Hangfire

## 🛠 Технологический стек

| Компонент       | Технология         |
|-----------------|--------------------|
| Backend         | ASP.NET Core 8     |
| Database        | PostgreSQL 15      |
| ORM             | Entity Framework Core 8 |
| Контейнеризация | Docker + Docker Compose |
| Фоновые задачи  | Hangfire           |
| Документация    | Swagger UI         |

## 📚 API Endpoints

### Сегменты

| Метод   | Путь                          | Описание                               |
|---------|-------------------------------|----------------------------------------|
| `GET`   | `/api/segments`               | Получить все сегменты                 |
| `POST`  | `/api/segments`               | Создать новый сегмент                 |
| `DELETE`| `/api/segments/{name}`        | Удалить сегмент (асинхронно)          |
| `POST`  | `/api/segments/{name}/assign` | Назначить сегмент % пользователей     |

### Пользователи

| Метод   | Путь                              | Описание                               |
|---------|-----------------------------------|----------------------------------------|
| `GET`   | `/api/user`                       | Получить всех пользователей           |
| `POST`  | `/api/user`                       | Создать пользователя                  |
| `GET`   | `/api/user/{id}/segments`         | Получить сегменты пользователя        |
| `POST`  | `/api/user/{id}/segments/{name}`  | Добавить пользователя в сегмент       |
| `DELETE`| `/api/user/{id}/segments/{name}`  | Удалить пользователя из сегмента      |

## 📈 Производительность

| Операция               | 10k users | 100k users | 1M users |
|------------------------|----------|-----------|---------|
| Добавление в сегмент   | 15ms     | 18ms      | 22ms    |
| Распределение 10%      | 1.2s     | 4.5s      | 45s     |
| Распределение 30%      | 3.8s     | 18s       | 2m10s   |
