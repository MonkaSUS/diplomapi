namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapDelayEndpoints(WebApplication app)
        {
            var StudentDelayGroup = app.MapGroup("/studentDelay").RequireAuthorization();
            StudentDelayGroup.MapGet("/all", async (ThreeMoronsContext db) => await db.StudentDelays.ToListAsync());
            StudentDelayGroup.MapGet("/", async (ThreeMoronsContext db, [FromQuery(Name = "id")] Guid id) => await db.StudentDelays.FindAsync(id));
            StudentDelayGroup.MapPost("/", async (ThreeMoronsContext db, StudentDelayInput inp, IValidator<StudentDelayInput> val) =>
            {
                var ValidationResult = val.Validate(inp);
                if (!ValidationResult.IsValid)
                {
                    return Results.ValidationProblem((IDictionary<string, string[]>)ValidationResult.Errors);
                }
                StudentDelay delay = new StudentDelay()
                {
                    Id = Guid.NewGuid(),
                    ClassName = inp.className,
                    Delay = inp.Delay,
                    StudNumber = inp.studNumber
                };
                try
                {
                    await db.StudentDelays.AddAsync(delay);
                    await db.SaveChangesAsync();
                    return Results.Created();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.Message);
                }
            }).RequireAuthorization(x => x.RequireClaim("userClassId", "2"));
            StudentDelayGroup.MapDelete("/", async (ThreeMoronsContext db, [FromQuery(Name = "id")] Guid id) =>
            {
                try
                {
                    db.StudentDelays.Remove(await db.StudentDelays.FindAsync(id));
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.Message);
                }
            });
        }
    } 
}
