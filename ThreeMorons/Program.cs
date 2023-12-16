using Serilog;
using Microsoft.EntityFrameworkCore;
using ThreeMorons.Model;
using FluentValidation;
using ThreeMorons.Validators;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using ThreeMorons.UserInputTypes;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using ThreeMorons.SecurityThings;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();

builder.Services.AddDbContext<ThreeMoronsContext>(o => o.UseSqlServer());

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

builder.Logging.AddSerilog(logger);

builder.Services.AddScoped<IValidator<RegistrationInput>, RegistrationValidator>();
builder.Services.AddScoped<IValidator<AuthorizationInput>, AuthorizationValidator>();
builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };
        });
builder.Services.AddAuthorization();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}




app.MapGet("/", () => "Ётот материал создан лицом, которое признано иностранным агентом на терриотрии –‘");

app.MapPost("/register", [AllowAnonymous] async (IValidator<RegistrationInput> validator, RegistrationInput inp, ThreeMoronsContext db) =>
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


app.MapPost("/authorizeTest", [AllowAnonymous] async (IValidator<AuthorizationInput> Validator, AuthorizationInput inp, ThreeMoronsContext db) =>
    {
        var valres = await Validator.ValidateAsync(inp);
        if (!valres.IsValid)
        {
            return Results.ValidationProblem(valres.ToDictionary());
        }
        if (await db.Users.FirstOrDefaultAsync(user=> user.Login == inp.login) is User authUser)
        {
            byte[] userSalt = Encoding.UTF8.GetBytes(authUser.Salt); //кака€-то забориста€ маги€ с кодировками, см PasswordMegaHasher 12-13 строки.
            var hashedPassword = PasswordMegaHasher.HashPass(authUser.Password, userSalt);
            if (authUser.Password != hashedPassword)
            {
                return Results.Unauthorized();
            }
            var stringToken = JwtIssuer.IssueJwtForUser(builder.Configuration, authUser);
            return Results.Ok(stringToken);

        }
        else
        {
            return Results.Unauthorized();
        }
     
    });



app.MapGet("/SecretInfoTest", () => "among us").RequireAuthorization();


app.UseAuthentication();
app.UseAuthorization();

app.Run();
