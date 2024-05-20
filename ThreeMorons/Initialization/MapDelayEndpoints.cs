namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapDelayEndpoints(WebApplication app)
        {
            var StudentDelayGroup = app.MapGroup("/delay").RequireAuthorization();
            StudentDelayGroup.MapGet("/all", async (ThreeMoronsContext db, ILoggerFactory logFac) =>
            {
                logFac.CreateLogger("delay").LogInformation("All delays retrieved");
                return await db.StudentDelays.Where(x => x.IsDeleted == false).ToListAsync();
            });
            StudentDelayGroup.MapGet("/", async (ThreeMoronsContext db,Guid id) => db.StudentDelays.Where(x=>x.IsDeleted==false).FirstOrDefaultAsync(x=>x.Id==id));
            StudentDelayGroup.MapPost("/", async (ThreeMoronsContext db, StudentDelayInput inp, IValidator<StudentDelayInput> val) =>
            {
                var ValidationResult = val.Validate(inp);
                if (!ValidationResult.IsValid)
                {
                    return Results.ValidationProblem((IDictionary<string, string[]>)ValidationResult.Errors);
                }
                StudentDelay delay = new()
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
            }).RequireAuthorization(x => x.RequireClaim("userClass", "2"));
            StudentDelayGroup.MapDelete("/", async (ThreeMoronsContext db, Guid id) =>
            {
                try
                {
                    var delay = await db.StudentDelays.FindAsync(id);
                    delay.IsDeleted = true;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.Message);
                }
            });
            StudentDelayGroup.MapGet("/", async (ThreeMoronsContext db, [FromQuery(Name = "student")]string studNum ) =>await db.StudentDelays.Where(x => x.StudNumber == studNum && x.IsDeleted==false).ToListAsync());

        }
    } 
}
