
namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapSkippedClassEndpoints(WebApplication app)
        {
            var SkippedClassGroup = app.MapGroup("/skippedClass").RequireAuthorization(); //возможно добавлю валидацию
            SkippedClassGroup.MapGet("/all", async (ThreeMoronsContext db) => await db.SkippedClasses.Where(x=> x.IsDeleted==false).ToListAsync());
            SkippedClassGroup.MapGet("/find", async (Guid id, ThreeMoronsContext db) => await db.SkippedClasses.FirstOrDefaultAsync(x => x.Id == id&& x.IsDeleted == false));
            SkippedClassGroup.MapGet("/group", async ([FromQuery(Name = "groupName")] string groupName, ThreeMoronsContext db) =>  //https://api/skippedClass/?groupName=ис-44К
            {
                var studNumbers = db.Students.Where(x => x.GroupName.ToLower().Trim() == groupName.ToLower().Trim()).Select(x=> x.StudNumber);
                List<SkippedClass> skippedClasses = new();
                await db.SkippedClasses.ForEachAsync(x=>
                {
                    if (studNumbers.Contains(x.StudNumber))
                    {
                        skippedClasses.Add(x);
                    }
                });
                return Results.Json(skippedClasses, opt, "application/json", 200);
            });
            SkippedClassGroup.MapPost("", async (SkippedClassInput input, ThreeMoronsContext db) =>
            {
                try
                {
                    if (input.DateOfSkip.DayOfWeek == DayOfWeek.Saturday|| input.DateOfSkip.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return Results.BadRequest(String.Format($"Ты пизданулся? {0} был выходной", input.DateOfSkip));
                    }
                    SkippedClass SkipToAdd = new() { Id = input.Id, DateOfSkip = input.DateOfSkip, StudNumber = input.StudNumber };
                    await db.AddAsync(SkipToAdd);
                    await db.SaveChangesAsync();
                    return Results.Created("/skippedClass", SkipToAdd);
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.ToString());
                }
            });
            SkippedClassGroup.MapDelete("", async ([FromQuery] Guid id, ThreeMoronsContext db) =>
            {
                try
                {
                    var toDelete = await db.SkippedClasses.FindAsync(id);
                    toDelete.IsDeleted = true;
                    await db.SaveChangesAsync();
                    return Results.Ok();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.ToString());
                }
            }).RequireAuthorization(o => o.RequireClaim("userClassId", "2"));

        }
    }
}
