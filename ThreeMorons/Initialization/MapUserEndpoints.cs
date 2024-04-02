
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
                    return Results.Json(UserToRegister, _opt, statusCode: 201);
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
                var handler = new JwtSecurityTokenHandler();

                if (UserToAuthorizeInDb is null)
                {
                    return Results.Problem("пользователь в целом налл");
                }
                foreach (var session in db.Sessions) //ЕСЛИ ДЛЯ ЭТОГО ПОЛЬЗОВАТЕЛЯ УЖЕ СУЩЕСТВУЕТ ОТКРЫТАЯ СЕССИЯ, ТО МЫ ЕЁ ЗАКРЫВАЕМ
                {
                    if (session.SessionEnd is not null && session.SessionEnd <= DateTime.Now)
                    {
                        var jwt = handler.ReadToken(session.JwtToken) as JwtSecurityToken;
                        var jti = jwt.Id;
                        if (jti == UserToAuthorizeInDb.Id.ToString())
                        {
                            session.SessionEnd = DateTime.Now;
                            session.IsValid = false;
                        }
                    }
                }
                byte[] userSalt = UserToAuthorizeInDb.Salt;
                var hashedPassword = PasswordMegaHasher.HashPass(inp.password, userSalt);
                if (UserToAuthorizeInDb.Password != hashedPassword)
                {
                    return Results.Unauthorized();
                }
                var stringToken = JwtIssuer.IssueJwtForUser(builder.Configuration, UserToAuthorizeInDb);
                Session newSession = new() //ДЛИТЕЛЬНОСТЬ СЕССИИ СОСТАВЛЯЕТ ДВА ДНЯ. ПРИ СОЗДАНИИ СЕССИИ В ПОЛЕ SessionEnd ЗАПИСЫВАЕТСЯ МАКСИМАЛЬНОЕ ВРЕМЯ ОКОНЧАНИЯ СЕССИИ, А ПРИ ДОСРОЧНОМ ЗАКРЫТИИ В ДБ ЗАПИСЫВАЕТСЯ НАСТОЯЩЕЕ ВРЕМЯ ЗАКРЫТИЯ СЕССИИ
                {
                    id = Guid.NewGuid(),
                    JwtToken = stringToken.jwt,
                    RefreshToken = stringToken.refresh,
                    IsValid = true,
                    SessionStart = DateTime.Now,
                    SessionEnd = DateTime.Now.AddDays(2)
                };
                db.Sessions.Add(newSession);
                db.SaveChanges();
                return Results.Json(stringToken, new JsonSerializerOptions() { IncludeFields = true}, "application/json", 200);
            });
            UserGroup.MapGet("/all", async (ThreeMoronsContext db) =>
            {   
                return Results.Json(await db.Users.ToListAsync(), new JsonSerializerOptions() { IncludeFields = true }, "application/json", 200);
            });
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
