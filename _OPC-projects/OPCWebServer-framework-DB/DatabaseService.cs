using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OPCWebServer
{
    public class DatabaseService : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<RawData> _collection;
        private readonly ConcurrentQueue<IEnumerable<RawData>> _queue = new ConcurrentQueue<IEnumerable<RawData>>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _workerTask;

        // Строка подключения: Shared позволяет API читать данные, пока фоновый поток пишет
        private const string ConnectionString = "Filename=data.db;Connection=shared";

        public DatabaseService()
        {
            // Открываем базу один раз при старте сервиса
            _db = new LiteDatabase(ConnectionString);

            // Получаем коллекцию и создаем индексы для быстрого поиска
            _collection = _db.GetCollection<RawData>("RawData");
            _collection.EnsureIndex(x => x.TagId);
            _collection.EnsureIndex(x => x.Timestamp);

            // Запускаем фоновый поток обработки очереди
            _workerTask = Task.Run(() => ProcessQueue(_cts.Token));
        }

        /// <summary>
        /// Добавляет порцию данных в очередь на запись
        /// </summary>
        public void EnqueueData(IEnumerable<RawData> data)
        {
            if (data != null && data.Any())
            {
                _queue.Enqueue(data);
            }
        }

        /// <summary>
        /// Фоновый процесс записи в БД
        /// </summary>
        private void ProcessQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested || !_queue.IsEmpty)
            {
                if (_queue.TryDequeue(out var dataBatch))
                {
                    try
                    {
                        // InsertBulk максимально ускоряет запись группы тегов
                        _collection.InsertBulk(dataBatch);
                    }
                    catch (Exception ex)
                    {
                        // Здесь можно добавить запись в ваш txtLog через событие или логгер
                        System.Diagnostics.Debug.WriteLine($"DB Write Error: {ex.Message}");
                    }
                }
                else
                {
                    // Если очередь пуста, спим 100мс
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Метод для API чтения истории
        /// </summary>
        public List<AveragedData> GetStatistics(int tagId, DateTime? from, DateTime? to)
        {
            try
            {
                var col = _db.GetCollection<AveragedData>("AveragedData");
                var query = col.Query().Where(x => x.TagId == tagId);

                // Фильтры по времени (для отчетов используем CalculationTime)
                if (from.HasValue)
                    query = query.Where(x => x.CalculationTime >= from.Value);

                if (to.HasValue)
                {
                    var endLimit = to.Value.TimeOfDay.TotalSeconds == 0
                                   ? to.Value.AddDays(1).AddSeconds(-1)
                                   : to.Value;
                    query = query.Where(x => x.CalculationTime <= endLimit);
                }

                return query.OrderBy(x => x.CalculationTime).ToList();
            }
            catch
            {
                return new List<AveragedData>();
            }
        }

        public List<RawData> GetData(int tagId, DateTime? from, DateTime? to, int step = 1)
        {
            try
            {
                var query = _collection.Query().Where(x => x.TagId == tagId);

                // Если параметры не заданы - возвращаем последние 1000 записей
                if (!from.HasValue && !to.HasValue)
                {
                    return _collection.Query()
                        .Where(x => x.TagId == tagId)
                        .OrderByDescending(x => x.Timestamp)
                        .Limit(1000)
                        .ToList();
                }

                if (from.HasValue)
                    query = query.Where(x => x.Timestamp >= from.Value);

                if (to.HasValue)
                {
                    // Если время не указано (00:00:00), расширяем до конца суток
                    var endLimit = to.Value.TimeOfDay.TotalSeconds == 0
                        ? to.Value.AddDays(1).AddSeconds(-1)
                        : to.Value;
                    query = query.Where(x => x.Timestamp <= endLimit);
                }

                var result = query.OrderBy(x => x.Timestamp).ToList();

                if (step <= 1) return result;

                // Прореживание данных для графиков
                return result.Where((x, i) => i % step == 0).ToList();
            }
            catch
            {
                return new List<RawData>();
            }
        }
      

        /// <summary>
        /// Очистка данных старше 36 часов
        /// </summary>
        public void CleanupOldData()
        {
            try
            {
                var threshold = DateTime.Now.AddHours(-36);
                _collection.DeleteMany(x => x.Timestamp < threshold);
                //_db.Rebuild();
                // ВАЖНО: НЕ вызываем Rebuild() здесь, чтобы не плодить backup-файлы
                // LiteDB сама переиспользует место внутри файла
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Cleanup Error: {ex.Message}");
            }
        }
        public void CalculateAverages(DateTime end, int hoursBack)
        {
            try
            {
                var start = end.AddHours(-hoursBack);
                var rawCol = _db.GetCollection<RawData>("RawData");
                var avgCol = _db.GetCollection<AveragedData>("AveragedData");

                // Принудительно создаем индекс, это инициализирует таблицу, даже если она пуста
                avgCol.EnsureIndex(x => x.CalculationTime);

                // 1. Получаем список всех уникальных TagId, которые есть в сырых данных за период
                var activeTagIds = rawCol.Query()
                    .Where(x => x.Timestamp >= start && x.Timestamp <= end)
                    .Select(x => x.TagId)
                    .ToEnumerable()
                    .Distinct()
                    .ToList();

                if (!activeTagIds.Any()) return;

                var results = new List<AveragedData>();

                foreach (var tagId in activeTagIds)
                {
                    // Выбираем все записи тега за период
                    var points = rawCol.Query()
                        .Where(x => x.TagId == tagId && x.Timestamp >= start && x.Timestamp <= end)
                        .ToList();

                    if (!points.Any()) continue;

                    // Фильтруем точки "работы" (значение > 0.9)
                    var workPoints = points.Where(x => x.Value > 0.9).ToList();

                    double avgValue = 0;
                    double runtimeSeconds = 0;

                    if (workPoints.Any())
                    {
                        // Среднее только среди тех, кто "работает"
                        avgValue = workPoints.Average(x => x.Value);

                        // Расчет времени: (кол-во рабочих точек / общее кол-во точек) * общее время периода
                        // Это самый точный способ, не привязанный к жестким "2 секундам"
                        double totalPeriodSec = (end - start).TotalSeconds;
                        double workRatio = (double)workPoints.Count / points.Count;
                        runtimeSeconds = totalPeriodSec * workRatio;
                    }

                    results.Add(new AveragedData
                    {
                        CalculationTime = end,
                        TagId = tagId,
                        AverageValue = Math.Round(avgValue, 3),
                        RuntimeMinutes = Math.Round(runtimeSeconds / 60.0, 2), // Храним в минутах для удобства
                        TotalPeriodMinutes = hoursBack * 60
                    });
                }

                if (results.Any())
                {
                    avgCol.InsertBulk(results);
                }
            }
            catch (Exception ex)
            {
                // Логируйте ex.Message, чтобы понять, если база заблокирована
            }
        }

        public void CalculateDowntimes(DateTime end, int hoursBack)
        {
            try
            {
                var start = end.AddHours(-hoursBack);
                var rawCol = _db.GetCollection<RawData>("RawData");
                var downCol = _db.GetCollection<DowntimeEvent>("Downtimes");

                downCol.EnsureIndex(x => x.StartTime);

                // Получаем все ID тегов
                var tagIds = rawCol.Query()
                    .Where(x => x.Timestamp >= start && x.Timestamp <= end)
                    .Select(x => x.TagId).ToEnumerable().Distinct().ToList();

                foreach (var id in tagIds)
                {
                    // Получаем ВСЕ точки тега за период, отсортированные по времени
                    var points = rawCol.Query()
                        .Where(x => x.TagId == id && x.Timestamp >= start && x.Timestamp <= end)
                        .OrderBy(x => x.Timestamp)
                        .ToList();

                    if (points.Count < 2) continue;

                    DateTime? currentDowntimeStart = null;

                    for (int i = 0; i < points.Count; i++)
                    {
                        // Условие простоя: значение <= 0.9
                        bool isLow = points[i].Value <= 0.9;

                        if (isLow && currentDowntimeStart == null)
                        {
                            // Начало потенциального простоя
                            currentDowntimeStart = points[i].Timestamp;
                        }
                        else if (!isLow && currentDowntimeStart != null)
                        {
                            // Простой прервался рабочим значением
                            SaveDowntimeIfValid(downCol, id, currentDowntimeStart.Value, points[i].Timestamp);
                            currentDowntimeStart = null;
                        }
                    }

                    // Важно: если период закончился, а тег всё еще в простое
                    if (currentDowntimeStart != null)
                    {
                        SaveDowntimeIfValid(downCol, id, currentDowntimeStart.Value, points.Last().Timestamp);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Downtime Error: " + ex.Message);
            }
        }

        private void SaveDowntimeIfValid(ILiteCollection<DowntimeEvent> col, int tagId, DateTime start, DateTime end)
        {
            var duration = (end - start).TotalMinutes;

            // ВРЕМЕННО: поставьте здесь 1.0 вместо 10.0, чтобы проверить, ловит ли он хоть что-то
            if (duration >= 10.0)
            {
                // Проверка на дубликаты (чтобы не писать один и тот же простой дважды при перезапуске)
                var exists = col.Exists(x => x.TagId == tagId && x.StartTime == start);
                if (!exists)
                {
                    col.Insert(new DowntimeEvent
                    {
                        TagId = tagId,
                        StartTime = start,
                        EndTime = end,
                        DurationMinutes = Math.Round(duration, 2)
                    });
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                // Ждем завершения записи оставшихся в очереди данных (макс 2 сек)
                _workerTask?.Wait(2000);
                _db?.Dispose();
                _cts.Dispose();
            }
            catch { }
        }
    }
}
