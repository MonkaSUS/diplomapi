namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapStudentEndpoints(WebApplication app)
        {
            var StudentGroup = app.MapGroup("/student").RequireAuthorization();
            StudentGroup.MapGet("/all", async (ThreeMoronsContext db) => await db.Students.Where(x=> x.IsDeleted==false).ToListAsync());
            StudentGroup.MapGet("", async (string studId, ThreeMoronsContext db) => await db.Students.Where(x=> x.IsDeleted == false).FirstOrDefaultAsync(x=> x.StudNumber == studId));
            StudentGroup.MapPost("", async (StudentInput inp, ThreeMoronsContext db) =>
            {
                try
                {
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
                    return Results.Created("/student", StudentToCreate);
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.ToString());
                }
            });
            StudentGroup.MapPut("", async (StudentInput inp, ThreeMoronsContext db) =>
            {
                try //12 часов ночи, я знаю, что можно сделать элегантнее. оставлю так до первого рефакторинга
                {
                    var StudentToUpdate = await db.Students.FindAsync(inp.StudNumber);
                    StudentToUpdate.Name = inp.Name;
                    StudentToUpdate.Surname = inp.Surname;
                    StudentToUpdate.PhoneNumber = inp.PhoneNumber;
                    db.Students.Update(StudentToUpdate);
                    await db.SaveChangesAsync();
                    return Results.Ok();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.ToString());
                }
            });
            StudentGroup.MapDelete("", async (string StudNumber, ThreeMoronsContext db) =>
            {
                try
                {
                    var StudentToDelete = await db.Students.FindAsync(StudNumber);
                    StudentToDelete.IsDeleted = true;

                    await db.SaveChangesAsync();
                    return Results.Ok(); //ВСЁ ПРОСТО ОК
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.ToString()); //мы даже отчислить человека не можем нормально
                }

            });

            StudentGroup.MapGet("/search", async (ThreeMoronsContext db, [FromQuery(Name = "term")] string searchTerm, [FromQuery(Name = "group")] string groupName) =>
            {

                if (!string.IsNullOrEmpty(groupName))
                {
                    if (await db.Groups.FindAsync(groupName) is null)
                    {
                        return Results.BadRequest("Такой группы не существует");
                    }
                }
                var searchFilterResult = await db.Students.Where(x => x.GroupName == groupName && x.IsDeleted == false).Where(x => x.SerachTerm.Contains(searchTerm)).ToListAsync();
                return Results.Ok(searchFilterResult);
            });

        }
    }
}
