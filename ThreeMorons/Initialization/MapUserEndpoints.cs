﻿
using System.Linq;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ThreeMorons.DTOs;

namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        public static void MapUserEndpoints(WebApplication app, WebApplicationBuilder builder)
        {
            var UserGroup = app.MapGroup("/user");
            UserGroup.MapPost("/register", [AllowAnonymous] async (IValidator<RegistrationInput> validator, RegistrationInput inp, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("user");
                logger.LogInformation($"Попытка регистрации {inp.login}");
                var valres = await validator.ValidateAsync(inp);
                if (!valres.IsValid)
                {
                    logger.LogError("Ошибка валидации");
                    return Results.ValidationProblem(valres.ToDictionary());
                }
                if (await db.Users.AnyAsync(x => x.Login == inp.login))
                {
                    logger.LogError($"Пользователь уже существует: {inp.login}");
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
                        UserClassId = inp.UserClassId,
                        DateOfRegistration = DateTime.Now,
                        DateOfBirth = inp.dateOfBirth
                    };
                    await db.Users.AddAsync(UserToRegister);
                    await db.SaveChangesAsync();
                    return Results.Json(UserToRegister, _opt, statusCode: 200, contentType: "application/json");
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, "Ошибка при регистрации");
                    return TypedResults.BadRequest(exc.Message);
                }
            });
            UserGroup.MapPost("/authorize", [AllowAnonymous] async (HttpContext cont, IValidator<AuthorizationInput> Validator, AuthorizationInput inp, ThreeMoronsContext db, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("userauth");
                logger.LogInformation($"Попытка авторизации {inp.login}");
                var valres = await Validator.ValidateAsync(inp);
                if (!valres.IsValid)
                {
                    logger.LogError($"Ошибка валидации {inp.login}");
                    return Results.ValidationProblem(valres.ToDictionary());
                }
                User? UserToAuthorizeInDb = await db.Users.FirstOrDefaultAsync(user => user.Login == inp.login);

                if (UserToAuthorizeInDb is null)
                {
                    logger.LogError($"Пользователя с логином {inp.login} не существует");
                    return Results.Problem("пользователь в целом налл");
                }
                var handler = new JwtSecurityTokenHandler();
                logger.LogInformation("Создается JWT токен");
                foreach (var session in db.Sessions) //ЕСЛИ ДЛЯ ЭТОГО ПОЛЬЗОВАТЕЛЯ УЖЕ СУЩЕСТВУЕТ ОТКРЫТАЯ СЕССИЯ, ТО МЫ ЕЁ ЗАКРЫВАЕМ
                {
                    if (session.SessionEnd is not null && session.SessionEnd <= DateTime.Now && session.IsValid)
                    {
                        var jwt = handler.ReadToken(session.JwtToken) as JwtSecurityToken;
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
                        var jti = jwt.Id;
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
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
                (string jwt, string refresh, int userClass) credsAndClass;

                credsAndClass.jwt = stringToken.jwt;
                credsAndClass.refresh = stringToken.refresh;
                credsAndClass.userClass = UserToAuthorizeInDb.UserClassId;

                Session newSession = new() //ДЛИТЕЛЬНОСТЬ СЕССИИ СОСТАВЛЯЕТ ДВА ДНЯ. ПРИ СОЗДАНИИ СЕССИИ В ПОЛЕ SessionEnd ЗАПИСЫВАЕТСЯ МАКСИМАЛЬНОЕ ВРЕМЯ ОКОНЧАНИЯ СЕССИИ, А ПРИ ДОСРОЧНОМ ЗАКРЫТИИ В ДБ ЗАПИСЫВАЕТСЯ НАСТОЯЩЕЕ ВРЕМЯ ЗАКРЫТИЯ СЕССИИ
                {
                    id = Guid.NewGuid(),
                    JwtToken = stringToken.jwt,
                    RefreshToken = stringToken.refresh,
                    IsValid = true,
                    SessionStart = DateTime.Now.ToUniversalTime(),
                    SessionEnd = DateTime.Now.AddDays(2).ToUniversalTime()
                };
                try
                {
                    db.Sessions.Add(newSession);
                    db.SaveChanges();
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, "Ошибка при авторизации");
                    return Results.Problem("Ошибка при сохранении");
                }
                logger.LogInformation($"Успешная авторизация{inp.login}");
                return Results.Json(credsAndClass, new JsonSerializerOptions() { IncludeFields = true }, "application/json", 200);
            });
            UserGroup.MapGet("/all", async (ThreeMoronsContext db, ILoggerFactory fac, IEasyCachingProvider prov) =>
            {
                if (await prov.ExistsAsync("allUsers"))
                {
                    var allUsersCached = await prov.GetAsync<List<User>>("allUsers");
                    return Results.Json(allUsersCached, _opt, contentType: "application/json", statusCode: 200);
                }
                var logger = fac.CreateLogger("user");
                logger.LogInformation("Получена информация о всех пользователях");
                var allUsers = await db.Users.ToListAsync();
                await prov.SetAsync<List<User>>("allUsers", allUsers, TimeSpan.FromMinutes(30));
                return Results.Json(allUsers, _opt, "application/json", 200);
            }).RequireAuthorization(r => r.RequireClaim("userClassId", ["2", "3"]));
            UserGroup.MapGet("/", async (ThreeMoronsContext db, [FromQuery] Guid id) => await db.Users.FindAsync(id)).RequireAuthorization(r => r.RequireClaim("userClassId", ["2", "3"]));
            UserGroup.MapDelete("/", async (ThreeMoronsContext db, [FromQuery(Name = "id")] Guid id, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("user");
                logger.LogInformation($"Попытка удалить пользователя {id}");
                try
                {
                    var deletme = await db.Users.FindAsync(id);
                    if (deletme is null)
                    {
                        return Results.BadRequest("Пользователь не найден");
                    }
                    deletme.IsDeleted = true;
                    return Results.NoContent();
                }
                catch (Exception exc)
                {

                    logger.LogError(exc, "Ошибка при удалении пользователя");
                    return Results.Problem(exc.Message);
                }
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));
            UserGroup.MapGet("/search", async (ThreeMoronsContext db, [FromQuery] string term, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("user");
                var usersFound = await db.Users.Where(x => x.SearchTerm.Contains(term) && x.IsDeleted == false).ToListAsync();
                logger.LogInformation($"По запросу {term} найдено {usersFound.Count} пользователей");
                return Results.Ok(usersFound);
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));
            UserGroup.MapPost("/refreshPassword", async (ThreeMoronsContext db, ILoggerFactory fac, PasswordRefreshDTO data, IValidator<PasswordRefreshDTO> val) =>
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(data.jwtToken);
                string jti = jwt.Claims.First(c => c.Type == "jti").Value;
                var thisUser = await db.Users.FindAsync(Guid.Parse(jti));
                if (thisUser is null)
                {
                    return Results.NotFound("Пользователь с таким айди не найден");
                }
                var thisUserSalt = thisUser.Salt;
                var validationRes = val.Validate(data);
                if (!validationRes.IsValid)
                {
                    return Results.Json(validationRes.Errors, statusCode: 400);
                }
                if (PasswordMegaHasher.HashPass(data.oldPassword, thisUserSalt) != thisUser.Password)
                {
                    return Results.BadRequest("Введён неправильный пароль. Попробуйте ещё раз");
                }
                var newPassAndSalt = PasswordMegaHasher.HashPass(data.newPassword);
                try
                {
                    var thisUserSession = await db.Sessions.FirstAsync(x => x.JwtToken == data.jwtToken && x.RefreshToken == data.refreshToken);
                    if (!thisUserSession.IsValid)
                    {
                        return Results.Problem("Сессия устарела");
                    }
                    thisUserSession.SessionEnd = DateTime.Now;
                    thisUser.Password = newPassAndSalt.hashpass;
                    thisUser.Salt = newPassAndSalt.salt;
                    thisUserSession.IsValid = false;
                    await db.SaveChangesAsync();
                    return Results.Ok("Пароль изменён успешно. Авторизуйтесь ещё раз");
                }
                catch (Exception)
                {
                    return Results.Problem("Произошла проблема при изменении пароля. Попробуйте ещё раз", statusCode: 500);
                }

            });
        }
    }
}
