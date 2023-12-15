using Serilog;
using ThreeMorons;
using Microsoft.EntityFrameworkCore;
using ThreeMorons.Model;
using FluentValidation;
using ThreeMorons.Validators;
using System.ComponentModel.DataAnnotations;
var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Services.AddDbContext<ThreeMoronsContext>(o=> o.UseSqlServer());
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

builder.Logging.AddSerilog(logger);

builder.Services.AddScoped<IValidator<RegistrationInput>, RegistrationValidator>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}




app.MapGet("/", () => "Этот материал создан лицом, которое признано иностранным агентом на терриотрии РФ");

app.MapPost("/register", async (IValidator<RegistrationInput> validator ,RegistrationInput inp, ThreeMoronsContext db) =>
    {
        var valres = await validator.ValidateAsync(inp);
        if (!valres.IsValid)
        {
            return Results.ValidationProblem(valres.ToDictionary());
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
            //await db.SaveChangesAsync();
            return Results.Ok(UserToRegister);
        }
        catch (Exception exc)
        {
            return TypedResults.BadRequest(exc.Message);
        }
    });

app.Run();
