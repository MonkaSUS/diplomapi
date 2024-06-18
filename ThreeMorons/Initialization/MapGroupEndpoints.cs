namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapGroupEndpoints(WebApplication app)
        {
            var groupsGroup = app.MapGroup("/group").RequireAuthorization();
            groupsGroup.MapGet("/all", async (ThreeMoronsContext db, ILoggerFactory logfac) =>
            {
                var logger = logfac.CreateLogger("group");
                logger.LogInformation("All groups retrieved");
                return await db.Groups.ToListAsync();
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));

            groupsGroup.MapGet("/", async ([FromQuery(Name = "groupName")] string Name, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("group");
                logger.LogInformation($"Запрос информации по группе {Name}");
                return await db.Groups.FirstOrDefaultAsync(x => x.GroupName == Name);
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));

            groupsGroup.MapPost("/", async (ThreeMoronsContext db, GroupInput created, IValidator<GroupInput> validator, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("group");
                logger.LogInformation($"Попытка создать группу {created.GroupName}");
                var valres = await validator.ValidateAsync(created);
                if (!valres.IsValid)
                {
                    return Results.ValidationProblem(valres.ToDictionary());
                }
                try
                {
                    Group toCreate = new() { Building = created.Building, GroupCurator = created.groupCurator, GroupName = created.GroupName };
                    await db.Groups.AddAsync(toCreate);
                    await db.SaveChangesAsync();
                    return Results.Json(toCreate, options: _opt, statusCode: 200, contentType: "application/json");
                }
                catch (Exception excep)
                {
                    logger.LogError(excep, "Ошибка при сохранении группы");
                    return Results.Problem(excep.ToString());
                }
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));

            groupsGroup.MapGet("search", async (ThreeMoronsContext db, [FromQuery(Name = "searchTerm")]
            string searchTerm) => await db.Groups.Where(x => x.GroupName.Contains(searchTerm)).ToListAsync()).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));
        }
    }
}
