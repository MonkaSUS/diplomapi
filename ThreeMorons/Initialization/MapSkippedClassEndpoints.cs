
namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapSkippedClassEndpoints(WebApplication app)
        {
            var SkippedClassGroup = app.MapGroup("/skippedClass").RequireAuthorization(); //возможно добавлю валидацию
            SkippedClassGroup.MapGet("/all", async (ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("skips");
                logger.LogInformation("Запрос на получение всех пропусков");
                return await db.SkippedClasses.Where(x => x.IsDeleted == false).ToListAsync();
            });
            SkippedClassGroup.MapGet("/find", async (Guid id, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("skips");
                logger.LogInformation($"Поиск пропуска по id {id}");
                return await db.SkippedClasses.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            });
            SkippedClassGroup.MapGet("/forGroup", async ([FromQuery(Name = "groupName")] string groupName, ThreeMoronsContext db, ILoggerFactory fac) =>  //https://api.../skippedClass/forGroup/?groupName=ис-44К
            {
                var logger = fac.CreateLogger("group");
                var studNumbers = db.Students.Where(x => x.GroupName.ToLower().Trim() == groupName.ToLower().Trim()).Select(x=> x.StudNumber);
                List<SkippedClass> skippedClasses = new();
                await db.SkippedClasses.ForEachAsync(x=>
                {
                    if (studNumbers.Contains(x.StudNumber))
                    {
                        skippedClasses.Add(x);
                    }
                });
                logger.LogInformation($"По группе {groupName} найдено {skippedClasses.Count} записей");
                return Results.Json(skippedClasses, _opt, "application/json", 200);
            });
            SkippedClassGroup.MapPost("", async (SkippedClassInput input, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("skips");
                try
                {
                    logger.LogInformation("Попытка создать пропуск");
                    if (input.DateOfSkip.DayOfWeek == System.DayOfWeek.Saturday || input.DateOfSkip.DayOfWeek == System.DayOfWeek.Sunday)
                    {
                        logger.LogInformation($"{input.DateOfSkip} был выходной день. Пропуск не был создан");
                        return Results.BadRequest(String.Format($"Ты ? {0} был выходной", input.DateOfSkip));
                    }
                    SkippedClass SkipToAdd = new() { Id = input.Id, DateOfSkip = input.DateOfSkip, StudNumber = input.StudNumber };
                    await db.AddAsync(SkipToAdd);
                    await db.SaveChangesAsync();
                    logger.LogInformation($"Был создан пропуск {input.DateOfSkip} студента {input.StudNumber}");
                    return Results.Ok();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.ToString());
                }
            });
            SkippedClassGroup.MapDelete("", async ([FromQuery] Guid id, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("skips");
                try
                {
                    logger.LogInformation($"Попытка удалить пропуск {id}");
                    var toDelete = await db.SkippedClasses.FindAsync(id);
                    toDelete.IsDeleted = true;
                    await db.SaveChangesAsync();
                    logger.LogInformation($"Пропуск {id} от {toDelete.DateOfSkip} удалён успешно");
                    return Results.Ok();
                }
                catch (Exception exc)
                {
                    logger.LogException(exc);
                    return Results.Problem(exc.ToString());
                }
            }).RequireAuthorization(o => o.RequireClaim("userClassId", "2"));

        }
    }
}
