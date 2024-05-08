using Serilog.Data;
using System.Net;

namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapStudentEndpoints(WebApplication app)
        {
            var StudentGroup = app.MapGroup("/student").RequireAuthorization();
            StudentGroup.MapGet("/all", async (ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("student");
                logger.LogInformation("Получение информации о всех студентах");
                return await db.Students.Where(x => x.IsDeleted == false).ToListAsync();
            }).CacheOutput();
            StudentGroup.MapGet("", async (string studId, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("student");
                logger.LogInformation($"Получение информации о {studId}");
                return await db.Students.Where(x => x.IsDeleted == false).FirstOrDefaultAsync(x => x.StudNumber == studId);
            });
            StudentGroup.MapPost("", async (StudentInput inp, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("student");
                try
                {
                    logger.LogInformation($"Попытка создать студента {String.Join(separator: " ", inp.Surname, inp.Name, inp.StudNumber)}");
                    Student StudentToCreate = new()
                    {
                        StudNumber = inp.StudNumber,
                        GroupName = inp.GroupName,
                        Name = inp.Name,
                        Surname = inp.Surname,
                        Patronymic = inp.Patronymic,
                        PhoneNumber = inp.PhoneNumber,
                    };
                    await db.Students.AddAsync(StudentToCreate);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Студент создан успешно");
                    return Results.Json(StudentToCreate, _opt, statusCode: 200, contentType: "application/json");
                }
                catch (Exception exc)
                {
                    logger.LogException(exc);
                    return Results.Problem(exc.ToString());
                }
            });
            StudentGroup.MapPut("", async (StudentInput inp, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("student");
                logger.LogInformation("Попытка обновить инфу о студенте");
                try //12 часов ночи, я знаю, что можно сделать элегантнее. оставлю так до первого рефакторинга
                {
                    var StudentToUpdate = await db.Students.FindAsync(inp.StudNumber);
                    StudentToUpdate.Name = inp.Name;
                    StudentToUpdate.Surname = inp.Surname;
                    StudentToUpdate.PhoneNumber = inp.PhoneNumber;
                    db.Students.Update(StudentToUpdate);
                    await db.SaveChangesAsync();
                    logger.LogInformation($"Студент {inp.StudNumber} обновлён успешно");
                    return Results.Ok();
                }
                catch (Exception exc)
                {
                    logger.LogException(exc);
                    return Results.Problem(exc.ToString());
                }
            });
            StudentGroup.MapDelete("", async (string StudNumber, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger($"Попытка удалить студента {StudNumber}");
                try
                {
                    var StudentToDelete = await db.Students.FindAsync(StudNumber);
                    StudentToDelete.IsDeleted = true;
                    logger.LogInformation($"Студент {StudNumber} удалён успешно");
                    await db.SaveChangesAsync();
                    return Results.Ok(); //ВСЁ ПРОСТО ОК
                }
                catch (Exception exc)
                {
                    logger.LogException(exc);
                    return Results.Problem(exc.ToString()); //мы даже отчислить человека не можем нормально
                }

            });

            StudentGroup.MapGet("/search", async (ThreeMoronsContext db, [FromQuery(Name = "term")] string searchTerm, [FromQuery(Name = "group")] string groupName, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("student");
                if (!string.IsNullOrEmpty(groupName))
                {
                    if (await db.Groups.FindAsync(groupName) is null)
                    {
                        logger.LogInformation($"{groupName} не была найдена");
                        return Results.BadRequest("Такой группы не существует");
                    }
                }
                var searchFilterResult = await db.Students.Where(x => x.GroupName == groupName && x.IsDeleted == false).Where(x => x.SerachTerm.Contains(searchTerm)).ToListAsync();
                logger.LogInformation($"По запросу {searchTerm} в группе {groupName} было найдено {searchFilterResult.Count} студентов");
                return Results.Ok(searchFilterResult);
            });

        }
    }
}
