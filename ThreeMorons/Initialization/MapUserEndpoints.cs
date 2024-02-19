using System.Linq.Expressions;

namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapUserEndpoints(WebApplication app, WebApplicationBuilder builder)
        {
            var UserGroup = app.MapGroup("/user");
            UserGroup.MapPost("/register", [AllowAnonymous] async (IValidator<RegistrationInput> validator, RegistrationInput inp, ThreeMoronsContext db) =>
            {
                var valres = await validator.ValidateAsync(inp);
                if (!valres.IsValid)
                {
                    return Results.ValidationProblem(valres.ToDictionary());
                }
                if (await db.Users.AnyAsync(x => x.Login == inp.login))
                {
                    return Results.Conflict("Пользователь с таким логином уже существует");
                }
                var HashingResult = PasswordMegaHasher.HashPass(inp.password);
                try
                {
                    User UserToRegister = new()
                    {
                        Id = Guid.NewGuid(),
                        Login = inp.login,
                        Password = HashingResult.hashpass,
                        Salt = HashingResult.salt,
                        Name = inp.name,
                        Surname = inp.surname,
                        Patronymic = inp.patronymic,
                        UserClassId = inp.UserClassId
                    };
                    await db.Users.AddAsync(UserToRegister);
                    await db.SaveChangesAsync();
                    return Results.Created("", UserToRegister);
                }
                catch (Exception exc)
                {
                    return TypedResults.BadRequest(exc.Message);
                }
            });
            UserGroup.MapPost("/authorize", [AllowAnonymous] async (IValidator<AuthorizationInput> Validator, AuthorizationInput inp, ThreeMoronsContext db) =>
            {
                var valres = await Validator.ValidateAsync(inp);
                if (!valres.IsValid)
                {
                    return Results.ValidationProblem(valres.ToDictionary());
                }
                User UserToAuthorizeInDb = await db.Users.FirstOrDefaultAsync(user => user.Login == inp.login);
                if (UserToAuthorizeInDb is null)
                {
                    return Results.Problem("пользователь в целом конча");
                }
                byte[] userSalt = UserToAuthorizeInDb.Salt;
                var hashedPassword = PasswordMegaHasher.HashPass(inp.password, userSalt);
                if (UserToAuthorizeInDb.Password != hashedPassword)
                {
                    return Results.Unauthorized();
                }
                var stringToken = JwtIssuer.IssueJwtForUser(builder.Configuration, UserToAuthorizeInDb);
                return Results.Ok(stringToken);
            });
            UserGroup.MapGet("/", async (ThreeMoronsContext db) => await db.Users.ToListAsync());
            UserGroup.MapGet("/", async (ThreeMoronsContext db, [FromQuery] Guid id) => await db.Users.FindAsync(id));
            UserGroup.MapDelete("/", async (ThreeMoronsContext db, [FromQuery(Name = "id")] Guid id) =>
            {
                try
                {
                    var deletme = await db.Users.FindAsync(id);
                    deletme.IsDeleted = true;
                    return Results.NoContent();
                }
                catch (Exception exc)
                {
                    return Results.Problem(exc.Message);
                }
            });
            UserGroup.MapGet("/search", async (ThreeMoronsContext db, [FromQuery]string term) =>
            {
                var usersFound = await db.Users.Where(x => x.SearchTerm.Contains(term) && x.IsDeleted == false).ToListAsync();
                return Results.Ok(usersFound);

            });

        }
    }
}
