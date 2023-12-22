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
using Microsoft.AspNetCore.Mvc;

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
builder.Services.AddScoped<IValidator<Group>, GroupValidator>();
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
                ValidateLifetime = true,
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
            await db.SaveChangesAsync();
            return Results.Created("",UserToRegister);
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



app.MapGet("/SecretInfoTest", () => "among us").RequireAuthorization(o=> o.RequireClaim("userClass", "2"));

var groupsGroup = app.MapGroup("/group").RequireAuthorization();

groupsGroup.MapGet("/all", async(ThreeMoronsContext db)=> await db.Groups.ToListAsync());
groupsGroup.MapGet("/", async ([FromQuery(Name = "groupName")] string Name, ThreeMoronsContext db) => await db.Groups.FirstOrDefaultAsync(x=> x.GroupName==Name));
groupsGroup.MapPost("/create", async (ThreeMoronsContext db, Group created, IValidator<Group> validator) =>
{
    var valres = await validator.ValidateAsync(created);
    if (!valres.IsValid)
    {
        return Results.ValidationProblem(valres.ToDictionary());
    }
    try
    {
        await db.Groups.AddAsync(created);
        await db.SaveChangesAsync();
        return Results.Created("group",created);
    }
    catch (Exception excep)
    {
        return Results.Problem(excep.ToString());
    }
}).RequireAuthorization(r=> r.RequireClaim("userClass", "2"));
groupsGroup.MapPut("/update", async (Group toUpdate, IValidator<Group> validator, ThreeMoronsContext db) =>
{
    var valres = await validator.ValidateAsync(toUpdate);
    if (!valres.IsValid)
    {
        return Results.ValidationProblem(valres.ToDictionary());
    }
    try
    {
        var entity = await db.Groups.FindAsync(toUpdate.GroupName);
        entity = toUpdate;
        await db.SaveChangesAsync();
        return Results.Ok("very good ok nice");
    }
    catch (Exception exc)
    {
        return Results.Problem(exc.ToString());
    }
}).RequireAuthorization(r=> r.RequireClaim("userClass","2"));
groupsGroup.MapDelete("remove", async (ThreeMoronsContext db, Group toDelete) =>
{
    try
    {
        db.Groups.Remove(toDelete);
        await db.SaveChangesAsync();
        return Results.Ok("deleted cool");
    }
    catch (Exception exc)
    {
        return Results.Problem(exc.ToString());
    }
}).RequireAuthorization(r=> r.RequireClaim("userClass", "2"));

app.MapGet("/periods", async (ThreeMoronsContext db) => db.Periods.ToListAsync());


var SkippedClassGroup = app.MapGroup("/skippedClass").RequireAuthorization();

SkippedClassGroup.MapGet("/all", async (ThreeMoronsContext db)=> await db.SkippedClasses.ToListAsync());

SkippedClassGroup.MapGet("/", async ([FromQuery(Name = "id")] Guid id, ThreeMoronsContext db) => await db.SkippedClasses.FirstOrDefaultAsync(x => x.Id == id));
app.UseAuthentication();
app.UseAuthorization();

app.Run();
