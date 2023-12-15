using Microsoft.AspNetCore.Mvc;
using Serilog;
using ThreeMorons;
using Microsoft.EntityFrameworkCore;
using ThreeMorons.Model;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Services.AddDbContext<ThreeMoronsContext>(o=> o.UseSqlServer() &&);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

builder.Logging.AddSerilog(logger);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}




app.MapGet("/", () => "Этот материал создан лицом, которое признано иностранным агентом на терриотрии РФ");

app.MapPost("/register", async ([FromBody]RegistrationInput inp, ThreeMoronsContext db) =>
    {

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
            //await db.SaveChangesAsync();
            return TypedResults.Created(new JsonResult(UserToRegister).Value.ToString());
        }
        catch (Exception exc)
        {
            return TypedResults.BadRequest(exc.Message);
        }
    });

app.Run();
