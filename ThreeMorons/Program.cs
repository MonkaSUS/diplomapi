using Microsoft.AspNetCore.Mvc;
using Serilog;
using ThreeMorons;
using Microsoft.EntityFrameworkCore;
using ThreeMorons.Model;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ThreeMoronsContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();
app.UseDeveloperExceptionPage();

app.MapGet("/", () => "Этот материал создан лицом, которое признано иностранным агентом на терриотрии РФ");

app.MapPost("/register",  (RegistrationInput inp) =>
    {
        if (inp is null)
        {
            return Results.BadRequest();
        }
        string shablon = @"^[a-zA-Z]{1}[a-zA-Z1-9]{1,9}";
        Regex myRegex = new Regex(shablon);
        if (!myRegex.IsMatch(inp.login))
        {
            return Results.ValidationProblem();
        }

        var HashingResult = PasswordMegaHasher.HashPass(inp.password);
    });

app.Run();
