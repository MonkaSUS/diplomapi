namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapGroupEndpoints(WebApplication app)
        {
            var groupsGroup = app.MapGroup("/group").RequireAuthorization();
            groupsGroup.MapGet("/all", async (ThreeMoronsContext db) => await db.Groups.ToListAsync());
            groupsGroup.MapGet("/", async ([FromQuery(Name = "groupName")] string Name, ThreeMoronsContext db) => await db.Groups.FirstOrDefaultAsync(x => x.GroupName == Name));
            groupsGroup.MapPost("/", async (ThreeMoronsContext db, GroupInput created, IValidator<GroupInput> validator) =>
            {
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
                    return Results.Created("group", toCreate);
                }
                catch (Exception excep)
                {
                    return Results.Problem(excep.ToString());
                }
            }).RequireAuthorization(r => r.RequireClaim("userClass", "2"));

            groupsGroup.MapGet("search", async (ThreeMoronsContext db, [FromQuery(Name = "searchTerm")]
            string searchTerm) => await db.Groups.Where(x=> x.GroupName.Contains(searchTerm)).ToListAsync());
        }
    }
}
