using ThreeMorons.Model;
using ThreeMorons.UserInputTypes;

namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapSkippedClassEndpoints(WebApplication app)
        {
            var SkippedClassGroup = app.MapGroup("/skippedClass").RequireAuthorization(); //возможно добавлю валидацию
            SkippedClassGroup.MapGet("", async (ThreeMoronsContext db) => await db.SkippedClasses.ToListAsync());
            SkippedClassGroup.MapGet("", async ([FromQuery(Name = "id")] Guid id, ThreeMoronsContext db) => await db.SkippedClasses.FirstOrDefaultAsync(x => x.Id == id));
            SkippedClassGroup.MapPost("", async (SkippedClassInput input, ThreeMoronsContext db) =>
            {
                try
                {
                    SkippedClass SkipToAdd = new() { Id = input.Id, ClassName = input.className, DateOfSkip = input.DateOfSkip, StudNumber = input.StudNumber };
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
                    db.SkippedClasses.Remove(toDelete);
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
